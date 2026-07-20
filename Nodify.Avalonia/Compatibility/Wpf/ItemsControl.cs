// -----------------------------------------------------------------------------
//  WPF ItemsControl / GroupStyle shims (System.Windows.Controls)
// -----------------------------------------------------------------------------
//  Upstream `Node` declares template parts typed as `ItemsControl` and mirrors its
//  `InputGroupStyle` / `OutputGroupStyle` collections into `ItemsControl.GroupStyle`.
//  Avalonia's `ItemsControl` has no `GroupStyle` (it has no WPF-style item grouping),
//  so this shim derives from Avalonia's `ItemsControl` and adds a WPF-shaped
//  `GroupStyle` collection. The collection is a passive carrier today (grouping is a
//  theme/behavior concern deferred to a later phase); it exists so the upstream
//  `Node` source compiles and mirrors styles without modification.
// -----------------------------------------------------------------------------

using System.Collections.ObjectModel;
using AvItemsControl = Avalonia.Controls.ItemsControl;

namespace System.Windows.Controls
{
    /// <summary>
    /// WPF-compatible <see cref="GroupStyle"/> placeholder. Upstream <c>Node</c> only stores and mirrors
    /// instances of this type between its group-style collections; item grouping itself is deferred to the
    /// theme/behavior phase, so this carries no runtime behavior yet.
    /// </summary>
    public class GroupStyle
    {
    }

    /// <summary>
    /// WPF-compatible <see cref="ItemsControl"/> over Avalonia's <see cref="Avalonia.Controls.ItemsControl"/>.
    /// Adds the WPF <see cref="GroupStyle"/> collection that upstream <c>Node</c> populates for its input/output
    /// connector item hosts.
    /// </summary>
    public class ItemsControl : AvItemsControl
    {
        /// <summary>WPF-style item group styles. A passive carrier today; grouping behavior is deferred.</summary>
        public ObservableCollection<GroupStyle> GroupStyle { get; } = new ObservableCollection<GroupStyle>();
    }
}
