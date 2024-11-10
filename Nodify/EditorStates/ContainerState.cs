﻿#if Avalonia
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Nodify.Avalonia.Extensions;
using MouseButtonEventArgs = Avalonia.Input.PointerEventArgs;
using MouseEventArgs = Avalonia.Input.PointerEventArgs;
using MouseWheelEventArgs = Avalonia.Input.PointerWheelEventArgs;
#else
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
#endif

namespace Nodify
{
    /// <summary>The base class for container states.</summary>
    public abstract class ContainerState
    {
        /// <summary>Constructs a new <see cref="ContainerState"/>.</summary>
        /// <param name="container">The owner of the state.</param>
        public ContainerState(ItemContainer container)
        {
            Container = container;
        }

        /// <summary>The owner of the state.</summary>
        protected ItemContainer Container { get; }

        /// <summary>The owner of the state.</summary>
        protected NodifyEditor Editor => Container.Editor;

#if Avalonia
        /// <inheritdoc cref="ItemContainer.OnMouseDown(MouseButtonEventArgs)"/>
        public virtual void HandleMouseDown(PointerPressedEventArgs e) { }

        /// <inheritdoc cref="ItemContainer.OnMouseDown(MouseButtonEventArgs)"/>
        public virtual void HandleMouseUp(PointerReleasedEventArgs e) { }

        /// <inheritdoc cref="ItemContainer.OnMouseMove(MouseEventArgs)"/>
        public virtual void HandleMouseMove(PointerEventArgs e) { }

        /// <inheritdoc cref="ItemContainer.OnMouseWheel(MouseWheelEventArgs)"/>
        public virtual void HandleMouseWheel(PointerWheelEventArgs e) { }
#else
        /// <inheritdoc cref="ItemContainer.OnMouseDown(MouseButtonEventArgs)"/>
        public virtual void HandleMouseDown(MouseButtonEventArgs e) { }

        /// <inheritdoc cref="ItemContainer.OnMouseDown(MouseButtonEventArgs)"/>
        public virtual void HandleMouseUp(MouseButtonEventArgs e) { }

        /// <inheritdoc cref="ItemContainer.OnMouseMove(MouseEventArgs)"/>
        public virtual void HandleMouseMove(MouseEventArgs e) { }

        /// <inheritdoc cref="ItemContainer.OnMouseWheel(MouseWheelEventArgs)"/>
        public virtual void HandleMouseWheel(MouseWheelEventArgs e) { }
#endif

        /// <inheritdoc cref="ItemContainer.OnKeyUp(KeyEventArgs)"/>
        public virtual void HandleKeyUp(KeyEventArgs e) { }

        /// <inheritdoc cref="ItemContainer.OnKeyDown(KeyEventArgs)"/>
        public virtual void HandleKeyDown(KeyEventArgs e) { }

        /// <summary>Called when <see cref="ItemContainer.PushState(ContainerState)"/> or <see cref="ItemContainer.PopState"/> is called.</summary>
        /// <param name="from">The state we enter from (is null for root state).</param>
        public virtual void Enter(ContainerState? from) { }

        /// <summary>Called when <see cref="ItemContainer.PopState"/> is called.</summary>
        public virtual void Exit() { }

        /// <summary>Called when <see cref="ItemContainer.PopState"/> is called.</summary>
        /// <param name="from">The state we re-enter from.</param>
        public virtual void ReEnter(ContainerState from) { }

        /// <summary>Pushes a new state into the stack.</summary>
        /// <param name="newState">The new state.</param>
        public virtual void PushState(ContainerState newState) => Container.PushState(newState);

        /// <summary>Pops the current state from the stack.</summary>
        public virtual void PopState() => Container.PopState();
    }
}
