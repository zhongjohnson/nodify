using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Input;

namespace Nodify
{
    internal static class AvaloniaObjectExtensions
    {
        public static T? GetParentOfType<T>(this Visual current)
            where T : Visual
        {
            while ((current = current.GetVisualParent<Visual>()) != null)
            {
                if (current is T match)
                {
                    return match;
                }
            }

            return null;
        }

        public static Visual? GetParent(this Visual current, Func<Visual, bool> condition)
        {
            while ((current = current.GetVisualParent<Visual>()) != null)
            {
                if (condition(current))
                {
                    return current;
                }
            }

            return null;
        }

        public static T? GetChildOfType<T>(this Visual? depObj) where T : Visual
        {
            if (depObj == null)
            {
                return default;
            }

            var children = depObj.GetVisualChildren();
            foreach (var child in children)
            {
                if (child is T result)
                {
                    return result;
                }

                if (child is Visual visualChild && GetChildOfType<T>(visualChild) is T r)
                {
                    return r;
                }
            }

            return default;
        }

        public static T? GetElementAtPosition<T>(this Control container, Point position)
            where T : Control
        {
            var element = container.InputHitTest(position);

            // Walk up the visual tree to find element of type T
            var current = element as Visual;
            while (current != null)
            {
                if (current is T result)
                {
                    return result;
                }
                current = current.GetVisualParent<Visual>();
            }

            return default;
        }

        public static List<Control> GetIntersectingElements(this Control container, Geometry geometry, IReadOnlyCollection<Type> supportedTypes)
        {
            var result = new List<Control>();
            var bounds = geometry.Bounds;

            // Manual traversal since Avalonia doesn't have geometry hit testing in the same way
            var stack = new Stack<Visual>();
            stack.Push(container);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current is Control elem && elem.IsHitTestVisible)
                {
                    if (supportedTypes.Contains(elem.GetType()))
                    {
                        result.Add(elem);
                    }
                }

                foreach (var child in current.GetVisualChildren())
                {
                    if (child is Visual visualChild)
                    {
                        stack.Push(visualChild);
                    }
                }
            }

            return result;
        }

        public static IEnumerable<T> GetIntersectingElements<T>(this Control container, Rect area, Func<T, Rect> getBounds)
            where T : Visual
        {
            var stack = new Stack<Visual>();
            stack.Push(container);

            while (stack.Count > 0)
            {
                Visual current = stack.Pop();

                foreach (var child in current.GetVisualChildren())
                {
                    if (child is T tChild)
                    {
                        var bounds = getBounds(tChild);
                        if (bounds.Intersects(area))
                        {
                            yield return tChild;
                            continue;
                        }
                    }

                    if (child is Visual visualChild)
                    {
                        stack.Push(visualChild);
                    }
                }
            }
        }

        #region Animation

        // Note: Avalonia animations work differently from WPF
        // These methods provide a similar API but use Avalonia's animation system
        public static void StartAnimation(this Control animatableElement, AvaloniaProperty property, Point toValue, double animationDurationSeconds, EventHandler? completedEvent = null)
        {
            var animation = new Avalonia.Animation.Animation
            {
                Duration = TimeSpan.FromSeconds(animationDurationSeconds),
                Children =
                {
                    new Avalonia.Animation.KeyFrame
                    {
                        Cue = new Avalonia.Animation.Cue(1.0),
                        Setters =
                        {
                            new Avalonia.Animation.Setter(property, toValue)
                        }
                    }
                }
            };

            animation.RunAsync(animatableElement).ContinueWith(_ =>
            {
                completedEvent?.Invoke(null, EventArgs.Empty);
            });
        }

        public static void StartAnimation(this Control animatableElement, AvaloniaProperty property, double toValue, double animationDurationSeconds, EventHandler? completedEvent = null)
        {
            var animation = new Avalonia.Animation.Animation
            {
                Duration = TimeSpan.FromSeconds(animationDurationSeconds),
                Children =
                {
                    new Avalonia.Animation.KeyFrame
                    {
                        Cue = new Avalonia.Animation.Cue(1.0),
                        Setters =
                        {
                            new Avalonia.Animation.Setter(property, toValue)
                        }
                    }
                }
            };

            animation.RunAsync(animatableElement).ContinueWith(_ =>
            {
                completedEvent?.Invoke(null, EventArgs.Empty);
            });
        }

        public static void StartLoopingAnimation(this Control animatableElement, AvaloniaProperty property, double toValue, double durationInSeconds)
        {
            var animation = new Avalonia.Animation.Animation
            {
                Duration = TimeSpan.FromSeconds(durationInSeconds),
                IterationCount = Avalonia.Animation.IterationCount.Infinite,
                Children =
                {
                    new Avalonia.Animation.KeyFrame
                    {
                        Cue = new Avalonia.Animation.Cue(1.0),
                        Setters =
                        {
                            new Avalonia.Animation.Setter(property, toValue)
                        }
                    }
                }
            };

            animation.RunAsync(animatableElement);
        }

        public static void CancelAnimation(this Control animatableElement, AvaloniaProperty property)
        {
            // In Avalonia, we typically just set the value directly to stop animation
            // A more complete implementation would track running animations
        }

        #endregion
    }
}
