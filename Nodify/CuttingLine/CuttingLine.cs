using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Nodify
{
    public class CuttingLine : Shape
    {
        public static readonly StyledProperty<Point> StartPointProperty = 
            AvaloniaProperty.Register<CuttingLine, Point>(nameof(StartPoint), defaultValue: default(Point));

        public static readonly StyledProperty<Point> EndPointProperty = 
            AvaloniaProperty.Register<CuttingLine, Point>(nameof(EndPoint), defaultValue: default(Point));

        /// <summary>
        /// Will be set for <see cref="BaseConnection"/>s and custom connections when the cutting line intersects with them if <see cref="NodifyEditor.EnableCuttingLinePreview"/> is true.
        /// </summary>
        public static readonly AttachedProperty<bool> IsOverElementProperty = 
            AvaloniaProperty.RegisterAttached<CuttingLine, Control, bool>("IsOverElement", defaultValue: false);

        public static bool GetIsOverElement(Control elem)
            => elem.GetValue(IsOverElementProperty);

        public static void SetIsOverElement(Control elem, bool value)
            => elem.SetValue(IsOverElementProperty, value);

        /// <summary>
        /// Gets or sets the start point.
        /// </summary>
        public Point StartPoint
        {
            get => GetValue(StartPointProperty);
            set => SetValue(StartPointProperty, value);
        }

        /// <summary>
        /// Gets or sets the end point.
        /// </summary>
        public Point EndPoint
        {
            get => GetValue(EndPointProperty);
            set => SetValue(EndPointProperty, value);
        }

        private StreamGeometry? _geometry;

        protected override Geometry? CreateDefiningGeometry()
        {
            _geometry = new StreamGeometry();
            using (var context = _geometry.Open())
            {
                context.BeginFigure(StartPoint, false);
                context.LineTo(EndPoint);
                context.EndFigure(false);
            }

            return _geometry;
        }

        static CuttingLine()
        {
            AffectsGeometry<CuttingLine>(StartPointProperty, EndPointProperty);
            IsHitTestVisibleProperty.OverrideDefaultValue<CuttingLine>(false);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (Fill != null && StrokeThickness > 0)
            {
                var radius = StrokeThickness * 1.2;
                drawingContext.DrawEllipse(Fill, null, StartPoint, radius, radius);
                drawingContext.DrawEllipse(Fill, null, EndPoint, radius, radius);
            }
        }
    }
}
