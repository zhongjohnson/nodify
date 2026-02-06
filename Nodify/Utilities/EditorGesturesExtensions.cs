using Nodify.Interactivity;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Windows.Input;

namespace Nodify
{
    internal static class EditorGesturesExtensions
    {
        public static SelectionType GetSelectionType(this EditorGestures.SelectionGestures gestures, RoutedEventArgs e)
        {
            if (gestures.Append.Matches(e.Source, e))
            {
                return SelectionType.Append;
            }

            if (gestures.Invert.Matches(e.Source, e))
            {
                return SelectionType.Invert;
            }

            if (gestures.Remove.Matches(e.Source, e))
            {
                return SelectionType.Remove;
            }

            return SelectionType.Replace;
        }

        public static bool TryGetFocusDirection(this EditorGestures.DirectionalNavigationGestures gestures, RoutedEventArgs e, out Nodify.Interactivity.NavigationDirection direction)
        {
            direction = default;

            if (gestures.Left.Matches(e.Source, e))
            {
                direction = Nodify.Interactivity.NavigationDirection.Left;
                return true;
            }
            if (gestures.Right.Matches(e.Source, e))
            {
                direction = Nodify.Interactivity.NavigationDirection.Right;
                return true;
            }
            if (gestures.Up.Matches(e.Source, e))
            {
                direction = Nodify.Interactivity.NavigationDirection.Up;
                return true;
            }
            if (gestures.Down.Matches(e.Source, e))
            {
                direction = Nodify.Interactivity.NavigationDirection.Down;
                return true;
            }

            return false;
        }

        public static bool TryGetNavigationDirection(this EditorGestures.DirectionalNavigationGestures gestures, RoutedEventArgs e, out Vector direction)
        {
            double y = gestures.Up.Matches(e.Source, e) ? 1 : gestures.Down.Matches(e.Source, e) ? -1 : 0;
            double x = gestures.Left.Matches(e.Source, e) ? -1 : gestures.Right.Matches(e.Source, e) ? 1 : 0;

            direction = new Vector(x, y);

            return x != 0 || y != 0;
        }

        public static bool IsOppositeOf(this Nodify.Interactivity.NavigationDirection direction, Nodify.Interactivity.NavigationDirection other)
        {
            return (direction == Nodify.Interactivity.NavigationDirection.Left && other == Nodify.Interactivity.NavigationDirection.Right)
                || (direction == Nodify.Interactivity.NavigationDirection.Right && other == Nodify.Interactivity.NavigationDirection.Left)
                || (direction == Nodify.Interactivity.NavigationDirection.Up && other == Nodify.Interactivity.NavigationDirection.Down)
                || (direction == Nodify.Interactivity.NavigationDirection.Down && other == Nodify.Interactivity.NavigationDirection.Up)
                || (direction == Nodify.Interactivity.NavigationDirection.Next && other == Nodify.Interactivity.NavigationDirection.Previous)
                || (direction == Nodify.Interactivity.NavigationDirection.Previous && other == Nodify.Interactivity.NavigationDirection.Next)
                || (direction == Nodify.Interactivity.NavigationDirection.First && other == Nodify.Interactivity.NavigationDirection.Last)
                || (direction == Nodify.Interactivity.NavigationDirection.Last && other == Nodify.Interactivity.NavigationDirection.First);
        }
    }
}
