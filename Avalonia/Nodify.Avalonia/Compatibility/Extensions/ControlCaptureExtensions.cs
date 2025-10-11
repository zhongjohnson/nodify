namespace Nodify.Compatibility;

internal static class ControlCaptureExtensions
{
    internal static void PropagateMouseCapturedWithin(this IInputElement start, bool isCaptured)
    {
        var control = start as Control;
        MultiSelector? editor = control.FindAncestorOfType<MultiSelector>(true);
        while (editor != null)
        {
            editor.IsMouseCaptureWithin = isCaptured;
            editor = editor.FindAncestorOfType<MultiSelector>(false);
        }
    }
}