using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;

namespace Nodify.Avalonia.Extensions
{
    public static class StreamGeometryContextExtensions
    {
        public static void BezierTo(this StreamGeometryContext e, Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin)
        {
            e.CubicBezierTo(point1, point2, point3);
        }

        public static void LineTo(this StreamGeometryContext e, Point point, bool isStroked, bool isSmoothJoin)
        {
            e.LineTo(point);
        }

        public static void BeginFigure(this StreamGeometryContext e, Point startPoint, bool isFilled, bool isClosed)
        {
            e.BeginFigure(startPoint, isFilled);
        }
    }
}
