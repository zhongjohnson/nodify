// -----------------------------------------------------------------------------
//  WPF DependencyPropertyChangedEventArgs shim (System.Windows)
// -----------------------------------------------------------------------------

namespace System.Windows
{
    /// <summary>
    /// Provides data for a dependency property change, mirroring WPF's struct so the
    /// linked Nodify property-changed callbacks compile and run unchanged.
    /// </summary>
    public readonly struct DependencyPropertyChangedEventArgs
    {
        public DependencyPropertyChangedEventArgs(DependencyProperty property, object? oldValue, object? newValue)
        {
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>The dependency property that changed.</summary>
        public DependencyProperty Property { get; }

        /// <summary>The value before the change.</summary>
        public object? OldValue { get; }

        /// <summary>The value after the change.</summary>
        public object? NewValue { get; }
    }
}
