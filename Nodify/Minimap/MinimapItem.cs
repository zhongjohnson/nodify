using Avalonia;
using Avalonia.Controls;

namespace Nodify
{
    public class MinimapItem : ContentControl
    {
        static MinimapItem()
        {
            FocusableProperty.OverrideMetadata(typeof(MinimapItem), new StyledPropertyMetadata<bool>(false));
        }

        public static readonly StyledProperty<Point> LocationProperty = ItemContainer.LocationProperty.AddOwner<MinimapItem>(new StyledPropertyMetadata<Point>(default, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, affects: AffectsFlags.ParentMeasure));

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
