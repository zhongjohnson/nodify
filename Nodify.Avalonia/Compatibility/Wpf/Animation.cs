// -----------------------------------------------------------------------------
//  WPF animation shims (System.Windows.Media.Animation)
// -----------------------------------------------------------------------------
//  Upstream Nodify animates dependency properties with WPF's animation clocks:
//
//      element.StartAnimation(SomeProperty, toValue, seconds, completed);
//        -> new DoubleAnimation/PointAnimation { From, To, Duration, Completed += ... }
//           animation.Freeze();
//           element.BeginAnimation(SomeProperty, animation);
//
//      element.CancelAnimation(SomeProperty)  ->  element.BeginAnimation(SomeProperty, null);
//
//  Avalonia has no `BeginAnimation(DependencyProperty, AnimationTimeline)` API (its
//  animation system is timeline/transition based and typed differently). Nodify only
//  needs a small slice: linearly animate a `double` or `Point` DP from its current
//  value to a target over a duration, optionally looping forever, with a completion
//  callback and the ability to cancel.
//
//  This shim provides that slice with a lightweight, UI-thread `DispatcherTimer`
//  driven animator keyed on (element, property). It intentionally does NOT reproduce
//  WPF's full animation/timeline/easing model.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Threading;

namespace System.Windows
{
    /// <summary>
    /// Minimal WPF-compatible <c>Duration</c>. Nodify only ever assigns a <see cref="TimeSpan"/>
    /// (via <c>Duration = TimeSpan.FromSeconds(...)</c>), so the shim supports that implicit
    /// conversion and exposes <see cref="HasTimeSpan"/> / <see cref="TimeSpan"/>.
    /// </summary>
    public readonly struct Duration
    {
        /// <summary>Initializes a duration from a time span.</summary>
        /// <param name="timeSpan">The duration length.</param>
        public Duration(TimeSpan timeSpan)
        {
            TimeSpan = timeSpan;
            HasTimeSpan = true;
        }

        /// <summary>Gets a value indicating whether this duration has a concrete time span.</summary>
        public bool HasTimeSpan { get; }

        /// <summary>Gets the time span (valid only when <see cref="HasTimeSpan"/> is true).</summary>
        public TimeSpan TimeSpan { get; }

        /// <summary>Implicitly converts a <see cref="System.TimeSpan"/> to a <see cref="Duration"/>.</summary>
        public static implicit operator Duration(TimeSpan timeSpan) => new Duration(timeSpan);
    }
}

namespace System.Windows.Media.Animation
{
    /// <summary>
    /// WPF-compatible repeat behavior. Only the <see cref="Forever"/> case (used by Nodify's
    /// looping animations) and a default single-run are meaningful in this shim.
    /// </summary>
    public struct RepeatBehavior : IEquatable<RepeatBehavior>
    {
        /// <summary>Repeats the animation indefinitely.</summary>
        public static readonly RepeatBehavior Forever = new RepeatBehavior { IsForever = true };

        /// <summary>Gets a value indicating whether the animation repeats forever.</summary>
        public bool IsForever { get; private set; }

        /// <inheritdoc />
        public bool Equals(RepeatBehavior other) => IsForever == other.IsForever;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is RepeatBehavior other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => IsForever.GetHashCode();
    }

    /// <summary>
    /// Base for the WPF animation timelines Nodify uses. Carries a duration, an optional
    /// <see cref="RepeatBehavior"/>, and a <see cref="Completed"/> event. <see cref="Freeze"/> is a
    /// no-op retained for source compatibility.
    /// </summary>
    public abstract class AnimationTimeline
    {
        /// <summary>Gets or sets the animation duration.</summary>
        public Duration Duration { get; set; }

        /// <summary>Gets or sets how the animation repeats.</summary>
        public RepeatBehavior RepeatBehavior { get; set; }

        /// <summary>Raised when the (non-looping) animation completes.</summary>
        public event EventHandler? Completed;

        /// <summary>WPF freezes animations to make them immutable/shareable; a no-op here.</summary>
        public void Freeze()
        {
        }

        /// <summary>Computes the animated value at normalized progress <paramref name="t"/> (0..1).</summary>
        /// <param name="from">The starting value.</param>
        /// <param name="t">Normalized progress in the range [0, 1].</param>
        /// <returns>The interpolated value, boxed.</returns>
        internal abstract object Interpolate(object from, double t);

        /// <summary>Gets the configured start value, or null to use the property's current value.</summary>
        internal abstract object? FromBoxed { get; }

        internal void RaiseCompleted() => Completed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>WPF-compatible <see cref="double"/> animation (linear).</summary>
    public sealed class DoubleAnimation : AnimationTimeline
    {
        /// <summary>Gets or sets the start value. When null, the property's current value is used.</summary>
        public double? From { get; set; }

        /// <summary>Gets or sets the target value.</summary>
        public double To { get; set; }

        internal override object? FromBoxed => From;

        internal override object Interpolate(object from, double t)
        {
            double start = (double)from;
            return start + (To - start) * t;
        }
    }

    /// <summary>WPF-compatible <see cref="Point"/> animation (linear).</summary>
    public sealed class PointAnimation : AnimationTimeline
    {
        /// <summary>Gets or sets the start value. When null, the property's current value is used.</summary>
        public Point? From { get; set; }

        /// <summary>Gets or sets the target value.</summary>
        public Point To { get; set; }

        internal override object? FromBoxed => From;

        internal override object Interpolate(object from, double t)
        {
            Point start = (Point)from;
            return new Point(start.X + (To.X - start.X) * t, start.Y + (To.Y - start.Y) * t);
        }
    }

    /// <summary>
    /// Drives <see cref="AnimationTimeline"/> instances against dependency properties using a
    /// UI-thread <see cref="DispatcherTimer"/>. One active animation per (element, property);
    /// starting a new one (or a null one) cancels the previous.
    /// </summary>
    internal static class PropertyAnimator
    {
        private const double FrameIntervalMs = 1000.0 / 60.0;

        private sealed class Running
        {
            public required DispatcherTimer Timer;
            public required AnimationTimeline Animation;
            public required AvaloniaObject Target;
            public required DependencyProperty Property;
            public required object FromValue;
            public required DateTime StartTime;
        }

        private static readonly Dictionary<(AvaloniaObject, DependencyProperty), Running> _running
            = new Dictionary<(AvaloniaObject, DependencyProperty), Running>();

        public static void Begin(AvaloniaObject target, DependencyProperty property, AnimationTimeline? animation)
        {
            Stop(target, property);

            if (animation is null)
            {
                return;
            }

            double durationMs = animation.Duration.HasTimeSpan ? animation.Duration.TimeSpan.TotalMilliseconds : 0;

            object fromValue = animation.FromBoxed ?? target.GetValue(property)!;

            // Zero (or non-positive) duration: snap to the end value immediately.
            if (durationMs <= 0)
            {
                target.SetValue(property, animation.Interpolate(fromValue, 1.0));
                if (!animation.RepeatBehavior.IsForever)
                {
                    animation.RaiseCompleted();
                }
                return;
            }

            var timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(FrameIntervalMs)
            };

            var running = new Running
            {
                Timer = timer,
                Animation = animation,
                Target = target,
                Property = property,
                FromValue = fromValue,
                StartTime = DateTime.UtcNow
            };

            _running[(target, property)] = running;

            timer.Tick += (_, _) => Tick(running, durationMs);
            timer.Start();

            // Apply the initial frame synchronously so the first value is set without a frame delay.
            target.SetValue(property, animation.Interpolate(fromValue, 0.0));
        }

        private static void Tick(Running running, double durationMs)
        {
            double elapsed = (DateTime.UtcNow - running.StartTime).TotalMilliseconds;
            double t = elapsed / durationMs;

            if (t >= 1.0)
            {
                if (running.Animation.RepeatBehavior.IsForever)
                {
                    // Loop: restart from the original start value.
                    running.StartTime = DateTime.UtcNow;
                    running.Target.SetValue(running.Property, running.Animation.Interpolate(running.FromValue, 0.0));
                    return;
                }

                running.Target.SetValue(running.Property, running.Animation.Interpolate(running.FromValue, 1.0));
                Stop(running.Target, running.Property);
                running.Animation.RaiseCompleted();
                return;
            }

            running.Target.SetValue(running.Property, running.Animation.Interpolate(running.FromValue, t));
        }

        private static void Stop(AvaloniaObject target, DependencyProperty property)
        {
            if (_running.TryGetValue((target, property), out Running? running))
            {
                running.Timer.Stop();
                _running.Remove((target, property));
            }
        }
    }

    /// <summary>
    /// WPF-shaped <c>BeginAnimation</c> extension used by upstream animation helpers
    /// (<c>element.BeginAnimation(property, animation)</c> and <c>... , null)</c> to cancel).
    /// Forwards to the <see cref="PropertyAnimator"/>.
    /// </summary>
    public static class AnimatableExtensions
    {
        /// <summary>
        /// Starts <paramref name="animation"/> on <paramref name="element"/>'s
        /// <paramref name="property"/>, or cancels any running animation when <paramref name="animation"/> is null.
        /// </summary>
        public static void BeginAnimation(this AvaloniaObject element, DependencyProperty property, AnimationTimeline? animation)
            => PropertyAnimator.Begin(element, property, animation);
    }
}
