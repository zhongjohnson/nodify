namespace Nodify.Interactivity
{
    /// <summary>
    /// Specifies the direction of keyboard navigation for focus movement.
    /// Avalonia-compatible replacement for WPF's FocusNavigationDirection.
    /// </summary>
    public enum NavigationDirection
    {
        None = 0,
        Left = 1,
        Right = 2,
        Up = 3,
        Down = 4,
        Next = 5,
        Previous = 6,
        First = 7,
        Last = 8
    }
}
