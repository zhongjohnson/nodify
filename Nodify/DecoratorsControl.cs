#if Avalonia
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Nodify.Avalonia;
using Nodify.Avalonia.Helpers.Gestures;
using Nodify.Avalonia.Helpers;
using Nodify.Avalonia.Extensions;
using DependencyObject = Avalonia.AvaloniaObject;
using MouseButtonEventArgs = Avalonia.Input.PointerEventArgs;
using MouseEventArgs = Avalonia.Input.PointerEventArgs;
using MouseWheelEventArgs = Avalonia.Input.PointerWheelEventArgs;
using CommandBinding = Nodify.Avalonia.RoutedCommandBinding;
using MultiSelector = Avalonia.Controls.Primitives.SelectingItemsControl;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
#endif
namespace Nodify
{
    /// <summary>
    /// An <see cref="ItemsControl"/> that works with <see cref="DecoratorContainer"/>s.
    /// </summary>
    internal class DecoratorsControl : ItemsControl
    {
#if Avalonia
        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        {
            return new DecoratorContainer();
        }
#else
        /// <inheritdoc />
        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is DecoratorContainer;

        /// <inheritdoc />
        protected override DependencyObject GetContainerForItemOverride()
            => new DecoratorContainer();
#endif
    }
}
