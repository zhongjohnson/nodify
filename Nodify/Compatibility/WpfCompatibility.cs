using System;

namespace Nodify
{
    // WPF compatibility stubs for types that don't have Avalonia equivalents
    
    /// <summary>
    /// Placeholder for WPF's ResourceKey. In Avalonia, use string keys or custom types.
    /// </summary>
    public class ResourceKey
    {
        public ResourceKey(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }

    /// <summary>
    /// Placeholder for WPF's ComponentResourceKey. In Avalonia, use string keys.
    /// </summary>
    public class ComponentResourceKey : ResourceKey
    {
        public ComponentResourceKey(Type ownerType, object resourceId)
            : base($"{ownerType.Name}.{resourceId}")
        {
            OwnerType = ownerType;
            ResourceId = resourceId;
        }

        public Type OwnerType { get; }
        public object ResourceId { get; }
    }

    /// <summary>
    /// Placeholder for WPF's AdornerLayer. Avalonia doesn't have adorners.
    /// Consider using overlays, popups, or custom rendering instead.
    /// </summary>
    public class AdornerLayer
    {
        public static AdornerLayer? GetAdornerLayer(object element)
        {
            // In Avalonia, adorners don't exist. Return null or implement alternative.
            return null;
        }
    }

    /// <summary>
    /// Placeholder for WPF's FontSizeConverter attribute.
    /// </summary>
    public class FontSizeConverter
    {
    }
}
