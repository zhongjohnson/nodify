#if Avalonia
using Avalonia;
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
    public class MinimapItem : ContentControl
    {
#if Avalonia
        public static readonly StyledProperty<Point> LocationProperty = ItemContainer.LocationProperty.AddOwner<MinimapItem>();
#else
        public static readonly DependencyProperty LocationProperty = ItemContainer.LocationProperty.AddOwner(typeof(MinimapItem), new FrameworkPropertyMetadata(BoxValue.Point, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsParentMeasure));
#endif

        /// <summary>
        /// Gets or sets the location of this <see cref="MinimapItem"/> inside the <see cref="Minimap"/>.
        /// </summary>
        public Point Location
        {
            get => (Point)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }
    }
}
