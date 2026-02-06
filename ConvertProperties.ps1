# PowerShell script to help convert WPF DependencyProperty to Avalonia StyledProperty
# This generates the Avalonia property declarations from WPF patterns

param(
    [string]$InputFile = "Nodify/Connections/BaseConnection.cs"
)

$content = Get-Content $InputFile -Raw

# Pattern to match WPF DependencyProperty declarations
$pattern = 'public static readonly StyledProperty (\w+)Property = StyledProperty\.Register\(nameof\((\w+)\), typeof\(([^)]+)\), typeof\((\w+)\)(?:, new StyledPropertyMetadata\(([^)]+)(?:, ([^)]+)(?:, ([^)]+))?\))?\);'

$matches = [regex]::Matches($content, $pattern)

Write-Host "Found $($matches.Count) property declarations to convert"
Write-Host ""

foreach ($match in $matches) {
    $propertyName = $match.Groups[1].Value
    $clrPropertyName = $match.Groups[2].Value
    $propertyType = $match.Groups[3].Value
    $ownerType = $match.Groups[4].Value
    $defaultValue = $match.Groups[5].Value
    
    # Generate Avalonia syntax
    $avaloniaDecl = "public static readonly StyledProperty<$propertyType> ${propertyName}Property =`n"
    $avaloniaDecl += "    AvaloniaProperty.Register<$ownerType, $propertyType>(nameof($clrPropertyName)"
    
    if ($defaultValue) {
        # Clean up default value
        $cleanDefault = $defaultValue -replace 'BoxValue\.', 'default' -replace 'new StyledPropertyMetadataOptions.*', ''
        if ($cleanDefault -ne '') {
            $avaloniaDecl += ", defaultValue: $cleanDefault"
        }
    }
    
    $avaloniaDecl += ");"
    
    Write-Host "// Original:"
    Write-Host "// $($match.Value)"
    Write-Host "// Converted to:"
    Write-Host $avaloniaDecl
    Write-Host ""
}

Write-Host "`n=== Attached Properties ==="
$attachedPattern = 'public static readonly StyledProperty (\w+)Property = StyledProperty\.RegisterAttached\("(\w+)", typeof\(([^)]+)\), typeof\((\w+)\)(?:, new StyledPropertyMetadata\(([^)]+)(?:, ([^)]+)(?:, ([^)]+))?\))?\);'

$attachedMatches = [regex]::Matches($content, $attachedPattern)

foreach ($match in $attachedMatches) {
    $propertyName = $match.Groups[1].Value
    $attachedName = $match.Groups[2].Value
    $propertyType = $match.Groups[3].Value
    $ownerType = $match.Groups[4].Value
    $defaultValue = $match.Groups[5].Value
    
    $avaloniaDecl = "public static readonly AttachedProperty<$propertyType> ${propertyName}Property =`n"
    $avaloniaDecl += "    AvaloniaProperty.RegisterAttached<$ownerType, Control, $propertyType>(`"$attachedName`""
    
    if ($defaultValue) {
        $cleanDefault = $defaultValue -replace 'BoxValue\.', 'default'
        if ($cleanDefault -ne '') {
            $avaloniaDecl += ", defaultValue: $cleanDefault"
        }
    }
    
    $avaloniaDecl += ");"
    
    Write-Host "// Original:"
    Write-Host "// $($match.Value)"
    Write-Host "// Converted to:"
    Write-Host $avaloniaDecl
    Write-Host ""
}
