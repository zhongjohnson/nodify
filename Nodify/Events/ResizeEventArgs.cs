using System;

#if Avalonia
using Avalonia;
using Avalonia.Interactivity;
#else
using System.Windows;
#endif

namespace Nodify
{
#if !Avalonia
    /// <summary>
    /// Represents the method that will handle resize related routed events.
    /// </summary>
    /// <param name="sender">The sender of this event.</param>
    /// <param name="e">The event data.</param>
    public delegate void ResizeEventHandler(object sender, ResizeEventArgs e);
#endif

    /// <summary>
    /// Provides data for resize related routed events.
    /// </summary>
    public class ResizeEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResizeEventArgs"/> class with the previous and the new <see cref="Size"/>.
        /// </summary>
        /// <param name="previousSize">The previous size associated with this event.</param>
        /// <param name="newSize">The new size associated with this event.</param>
        public ResizeEventArgs(Size previousSize, Size newSize)
        {
            PreviousSize = previousSize;
            NewSize = newSize;
        }

        /// <summary>
        /// Gets the previous size of the object.
        /// </summary>
        public Size PreviousSize { get; }

        
        /// <summary>
        /// Gets the new size of the object.
        /// </summary>
        public Size NewSize { get; }

#if Avalonia
        protected void InvokeEventHandler(Delegate genericHandler, object genericTarget)
            => ((EventHandler<ResizeEventArgs>)genericHandler)(genericTarget, this);
#else
        protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
            => ((ResizeEventHandler)genericHandler)(genericTarget, this);
#endif
    }
}
