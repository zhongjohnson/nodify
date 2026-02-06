using Avalonia.Input;

namespace Nodify.Interactivity
{
    /// <summary>
    /// Focus traversal direction for keyboard navigation.
    /// Avalonia compatibility type for WPF's TraversalRequest.
    /// </summary>
    public class TraversalRequest
    {
        public TraversalRequest(NavigationDirection direction)
        {
            FocusNavigationDirection = direction;
        }

        public NavigationDirection FocusNavigationDirection { get; }
    }
}
