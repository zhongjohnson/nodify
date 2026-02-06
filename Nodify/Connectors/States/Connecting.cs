using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
    public static partial class ConnectorState
    {
        /// <summary>
        /// Represents the state for handling a connector's connecting operation in the editor, 
        /// enabling drag-based creation of connections between connectors.
        /// </summary>
        public class Connecting : DragState<Connector>
        {
            protected override bool HasContextMenu => Element.HasContextMenu;
            protected override bool CanCancel => Connector.AllowPendingConnectionCancellation;
            protected override bool IsToggle => EnableToggledConnectingMode;

            /// <summary>
            /// Initializes a new instance of the <see cref="Connecting"/> class.
            /// </summary>
            /// <param name="connector">The connector associated with this state.</param>
            public Connecting(Connector connector)
                : base(connector, EditorGestures.Mappings.Connector.Connect, EditorGestures.Mappings.Connector.CancelAction)
            {
                PositionElement = Element.Editor ?? (IInputElement)Element;
            }

            protected override void OnBegin(RoutedEventArgs e)
                => Element.BeginConnecting();

            protected override void OnEnd(RoutedEventArgs e)
                => Element.EndConnecting();

            protected override void OnCancel(RoutedEventArgs e)
                => Element.CancelConnecting();

            protected override void OnMouseMove(MouseEventArgs e)
            {
                Point editorPosition = Element.GetLocationInsideEditor(e);  // could also use Element.Editor.MouseLocation
                Element.UpdatePendingConnection(editorPosition);
            }
        }
    }
}
