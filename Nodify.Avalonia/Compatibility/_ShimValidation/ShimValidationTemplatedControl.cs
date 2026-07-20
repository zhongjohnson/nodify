// -----------------------------------------------------------------------------
//  Phase 6b template/style/presenter-shim smoke test (NOT a port; validation only)
// -----------------------------------------------------------------------------
//  The real upstream consumers of these primitives ¡ª Node / GroupingNode / StateNode ¡ª
//  are still deferred: they additionally pull in NodifyEditor, ItemContainer, the
//  Connector stack, and (Node) a GroupStyle/ItemsControl.GroupStyle surface Avalonia has
//  no equivalent for. Linking any of them now would break the bottom-up port order.
//
//  Instead, this tiny local control mirrors exactly how those controls consume the new
//  templating/styling/presenter primitives, so the shims are compiled and exercised in
//  isolation:
//    * derives from the WPF `System.Windows.Controls.ContentControl` base and overrides
//      `DefaultStyleKeyProperty` + `FocusableProperty` in a static ctor (as KnotNode/Node do),
//    * reuses `ContentPresenter.ContentProperty` / `ContentTemplateProperty` and
//      `Border.CornerRadiusProperty` via `AddOwner(...)` (as StateNode does),
//    * declares `DataTemplate` / `ControlTemplate` / `Style`-typed dependency properties
//      (as Node/GroupingNode do for their container/thumb templates).
//
//  This validates that the deferred templated controls will compile verbatim against the
//  shim layer. It can be removed once a real templated control is linked.
// -----------------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;

namespace Nodify.Avalonia.Compatibility
{
    internal sealed class ShimValidationTemplatedControl : ContentControl
    {
        // Reuse framework-control DP statics via AddOwner (mirrors StateNode).
        public static readonly DependencyProperty ContentTemplateProperty =
            ContentPresenter.ContentTemplateProperty.AddOwner(typeof(ShimValidationTemplatedControl));

        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(typeof(ShimValidationTemplatedControl));

        // Template/style-typed DPs (mirrors Node.InputConnectorTemplate / GroupingNode.ResizeThumbTemplate
        // / Node.ContentContainerStyle). These exercise the DataTemplate/ControlTemplate/Style aliases.
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(ShimValidationTemplatedControl));

        public static readonly DependencyProperty ThumbTemplateProperty =
            DependencyProperty.Register(nameof(ThumbTemplate), typeof(ControlTemplate), typeof(ShimValidationTemplatedControl));

        public static readonly DependencyProperty ContainerStyleProperty =
            DependencyProperty.Register(nameof(ContainerStyle), typeof(Style), typeof(ShimValidationTemplatedControl));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public ControlTemplate? ThumbTemplate
        {
            get => (ControlTemplate?)GetValue(ThumbTemplateProperty);
            set => SetValue(ThumbTemplateProperty, value);
        }

        public Style? ContainerStyle
        {
            get => (Style?)GetValue(ContainerStyleProperty);
            set => SetValue(ContainerStyleProperty, value);
        }

        static ShimValidationTemplatedControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ShimValidationTemplatedControl), new FrameworkPropertyMetadata(typeof(ShimValidationTemplatedControl)));
            FocusableProperty.OverrideMetadata(typeof(ShimValidationTemplatedControl), new FrameworkPropertyMetadata(BoxValue.False));
        }
    }
}
