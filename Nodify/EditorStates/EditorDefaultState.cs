#if Avalonia
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Nodify.Avalonia.Extensions;
using MouseButtonEventArgs = Avalonia.Input.PointerEventArgs;
using MouseEventArgs = Avalonia.Input.PointerEventArgs;
using MouseWheelEventArgs = Avalonia.Input.PointerWheelEventArgs;
using Nodify.Avalonia.Helpers;
#else
using System.Windows;
using System.Windows.Input;
#endif
using static Nodify.SelectionHelper;

namespace Nodify
{
    /// <summary>
    /// The default state of the editor.
    /// <br />
    /// <br />  Default State
    /// <br />  	- mouse left down  	-> Selecting State
    /// <br />  	- mouse right down  -> Panning State
    /// <br /> 	
    /// <br />  Selecting State
    /// <br />  	- mouse left up 	-> Default State
    /// <br />  	- mouse right down 	-> Panning State
    /// <br /> 
    /// <br />  Panning State
    /// <br />  	- mouse right up	-> previous state (Selecting State or Default State)
    /// <br />  	- mouse left up		-> Panning State
    /// <br />	
    /// </summary>
    public class EditorDefaultState : EditorState
    {
        /// <summary>Constructs an instance of the <see cref="EditorDefaultState"/> state.</summary>
        /// <param name="editor">The owner of the state.</param>
        public EditorDefaultState(NodifyEditor editor) : base(editor)
        {
        }

        /// <inheritdoc />
#if Avalonia
        public override void HandleMouseDown(PointerPressedEventArgs e)
#else
        public override void HandleMouseDown(MouseButtonEventArgs e)
#endif
        {
            EditorGestures.NodifyEditorGestures gestures = EditorGestures.Mappings.Editor;
            if (gestures.Cutting.Matches(e.Source, e))
            {
                PushState(new EditorCuttingState(Editor));
            }
            else if (gestures.Selection.Select.Matches(e.Source, e))
            {
                SelectionType selectionType = GetSelectionType(e);
                var selecting = new EditorSelectingState(Editor, selectionType);
                PushState(selecting);
            }
            else if (!Editor.DisablePanning && gestures.Pan.Matches(e.Source, e))
            {
                PushState(new EditorPanningState(Editor));
            }
        }
    }
}
