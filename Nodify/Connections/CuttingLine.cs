#if Avalonia
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using Nodify.Avalonia.Extensions;
using UIElement = Avalonia.Controls.Control;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
#endif

namespace Nodify
{
    public class CuttingLine : Shape
    {
#if Avalonia
        public static readonly StyledProperty<Point> StartPointProperty = AvaloniaProperty.Register<CuttingLine, Point>(nameof(StartPoint));
        public static readonly StyledProperty<Point> EndPointProperty = AvaloniaProperty.Register<CuttingLine, Point>(nameof(EndPoint));

        /// <summary>
        /// Will be set for <see cref="BaseConnection"/>s and custom connections when the cutting line intersects with them if <see cref="NodifyEditor.EnableCuttingLinePreview"/> is true.
        /// </summary>
        public static readonly AttachedProperty<bool> IsOverElementProperty = PendingConnection.IsOverElementProperty.AddOwner<CuttingLine>();

        public static bool GetIsOverElement(UIElement elem)
            => (bool)elem.GetValue(IsOverElementProperty);

        public static void SetIsOverElement(UIElement elem, bool value)
            => elem.SetValue(IsOverElementProperty, value);
#else
        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(nameof(StartPoint), typeof(Point), typeof(CuttingLine), new FrameworkPropertyMetadata(BoxValue.Point, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(nameof(EndPoint), typeof(Point), typeof(CuttingLine), new FrameworkPropertyMetadata(BoxValue.Point, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Will be set for <see cref="BaseConnection"/>s and custom connections when the cutting line intersects with them if <see cref="NodifyEditor.EnableCuttingLinePreview"/> is true.
        /// </summary>
        public static readonly DependencyProperty IsOverElementProperty = PendingConnection.IsOverElementProperty.AddOwner(typeof(CuttingLine));

        public static bool GetIsOverElement(UIElement elem)
            => (bool)elem.GetValue(IsOverElementProperty);

        public static void SetIsOverElement(UIElement elem, bool value)
       => elem.SetValue(IsOverElementProperty, value);
#endif

        /// <summary>
        /// Gets or sets whether cancelling a cutting operation is allowed.
        /// </summary>
        public static bool AllowCuttingCancellation { get; set; } = true;

        /// <summary>
        /// Gets or sets the start point.
        /// </summary>
        public Point StartPoint
        {
            get => (Point)GetValue(StartPointProperty);
            set => SetValue(StartPointProperty, value);
        }

        /// <summary>
        /// Gets or sets the end point.
        /// </summary>
        public Point EndPoint
        {
            get => (Point)GetValue(EndPointProperty);
            set => SetValue(EndPointProperty, value);
        }

#if !Avalonia
        private readonly StreamGeometry _geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };
#endif

#if Avalonia
        protected override Geometry? CreateDefiningGeometry()
        {
            var _geometry = new StreamGeometry();
#else
        protected override Geometry DefiningGeometry
        {
            get
            {
#endif
                using (StreamGeometryContext context = _geometry.Open())
                {
                    context.BeginFigure(StartPoint, false, false);
                    context.LineTo(EndPoint, true, true);
                }

                return _geometry;
#if !Avalonia
            }
#endif
        }

        static CuttingLine()
        {
#if !Avalonia
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CuttingLine), new FrameworkPropertyMetadata(typeof(CuttingLine)));
            IsHitTestVisibleProperty.OverrideMetadata(typeof(CuttingLine), new FrameworkPropertyMetadata(BoxValue.False));
#endif
        }

#if !Avalonia

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            drawingContext.DrawEllipse(Fill, null, StartPoint, StrokeThickness * 1.2, StrokeThickness * 1.2);
            drawingContext.DrawEllipse(Fill, null, EndPoint, StrokeThickness * 1.2, StrokeThickness * 1.2);
        }
#endif
    }
}
