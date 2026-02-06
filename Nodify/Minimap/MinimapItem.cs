using Avalonia;
using Avalonia.Controls;

namespace Nodify
{
    public class MinimapItem : ContentControl
    {
        static MinimapItem()
        {
            FocusableProperty.OverrideMetadata(typeof(MinimapItem), new StyledPropertyMetadata(BoxValue.False));
        }

        public static readonly StyledProperty LocationProperty = ItemContainer.LocationProperty.AddOwner(typeof(MinimapItem), new StyledPropertyMetadata(BoxValue.Point, StyledPropertyMetadataOptions.BindsTwoWayByDefault | StyledPropertyMetadataOptions.AffectsParentMeasure));

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
