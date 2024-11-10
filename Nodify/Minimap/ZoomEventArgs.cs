using System;

#if Avalonia
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Nodify.Avalonia;
using Nodify.Avalonia.Helpers.Gestures;
using Nodify.Avalonia.Helpers;
using Nodify.Avalonia.Extensions;
using CommandBinding = Nodify.Avalonia.RoutedCommandBinding;
using MultiSelector = Avalonia.Controls.Primitives.SelectingItemsControl;
#else
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
#endif

namespace Nodify
{
    /// <summary>
    /// Represents the method that will handle <see cref="Minimap.Zoom"/> routed event.
    /// </summary>
    /// <param name="sender">The object where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    public delegate void ZoomEventHandler(object sender, ZoomEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Minimap.Zoom"/> routed event.
    /// </summary>
    public class ZoomEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZoomEventArgs"/> class using the specified <see cref="Zoom"/> and <see cref="Location"/>.
        /// </summary>
        public ZoomEventArgs(double zoom, Point location)
        {
            Zoom = zoom;
            Location = location;
        }

        /// <summary>
        /// Gets the zoom amount.
        /// </summary>
        public double Zoom { get; }

        /// <summary>
        /// Gets the location where the editor should zoom in.
        /// </summary>
        public Point Location { get; }

#if !Avalonia
        protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
            => ((ZoomEventHandler)genericHandler)(genericTarget, this);
#endif
    }
}
