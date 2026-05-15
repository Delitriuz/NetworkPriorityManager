# WinForms to WinUI 3 Migration Design

**Date:** 2026-05-15
**Scope:** Migrate `NetworkPriorityManager` from .NET 8 WinForms to WinUI 3 with feature parity.
**Approach:** In-place migration (modify existing project files).
**Deployment:** MSIX packaged with framework dependency.

## Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Feature scope | Exact parity | User requirement; no new features added |
| Visual style | Fluent Design + Mica + system theme | Native WinUI 3 default; auto dark/light |
| Title bar | Custom, non-interactive | Extends content into title bar; no interactive controls inside title bar, so `SetTitleBar()` is sufficient (no manual region rects needed) |
| Deployment | MSIX packaged | Small package size via framework dependency; runtime managed by OS/Store |
| Window size | Fixed, non-resizable | Matches original WinForms behavior (`MaximizeBox = false`, compact form) |

## 1. Architecture & Project Structure

### 1.1 `.csproj` Changes

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWinUI>true</UseWinUI>
    <EnableMsixTooling>true</EnableMsixTooling>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <Platforms>x86;x64;ARM64</Platforms>
    <RuntimeIdentifiers>win-x86;win-x64;win-arm64</RuntimeIdentifiers>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.6.*" />
    <PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.*" />
    <Content Include="favicon.ico" />
  </ItemGroup>
</Project>
```

### 1.2 File Mapping

| Original File | Action | New File |
|---------------|--------|----------|
| `Program.cs` | Delete | — |
| `Form1.cs` | Delete | `MainWindow.xaml.cs` |
| `Form1.Designer.cs` | Delete | `MainWindow.xaml` |
| `Form1.resx` | Delete | — |
| `NetworkPriorityManager.csproj` | Modify | Same name |
| `NetworkPriorityManager.sln` | Keep | Same name |
| `favicon.ico` | Keep | Used via `AppWindow.SetIcon` |
| `.gitignore`, `.gitattributes` | Keep | Same |
| — | Create | `App.xaml` / `App.xaml.cs` |
| — | Create | `Package.appxmanifest` |
| — | Create | `app.manifest` |

### 1.3 `Package.appxmanifest` (Minimal)

```xml
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
         xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
         IgnorableNamespaces="uap rescap">
  <Identity Name="NetworkPriorityManager" Publisher="CN=YourName" Version="1.0.0.0" ProcessorArchitecture="x64" />
  <Properties>
    <DisplayName>Network Priority Manager</DisplayName>
    <PublisherDisplayName>YourName</PublisherDisplayName>
    <Logo>favicon.ico</Logo>
  </Properties>
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.26100.0" />
  </Dependencies>
  <Resources>
    <Resource Language="x-generate" />
  </Resources>
  <Applications>
    <Application Id="App" Executable="NetworkPriorityManager.exe" EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements DisplayName="Network Priority Manager" Description="Set network adapter priority"
                          BackgroundColor="transparent" Square150x150Logo="favicon.ico" Square44x44Logo="favicon.ico">
        <uap:DefaultTile Wide310x150Logo="favicon.ico" />
      </uap:VisualElements>
    </Application>
  </Applications>
  <Capabilities>
    <rescap:Capability Name="runFullTrust" />
  </Capabilities>
</Package>
```

### 1.4 `app.manifest`

For DPI awareness (recommended for crisp rendering on high-DPI displays). **Note:** In MSIX, `requestedExecutionLevel` is ignored by the OS; admin elevation is still triggered by the user via "Run as administrator".

```xml
<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <application xmlns="urn:schemas-microsoft-com:asm.v3">
    <windowsSettings>
      <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
      <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true/pm</dpiAware>
    </windowsSettings>
  </application>
</assembly>
```

### 1.5 Entry Point

WinUI 3 replaces `Program.Main()` with `App.OnLaunched()`:

```csharp
protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
{
    m_window = new MainWindow();
    m_window.Activate();
}
```

## 2. UI Layout & Visual Design

### 2.1 Window Structure

Custom title bar (48px) with app icon and title text. Content area below uses Fluent spacing.

```
┌─────────────────────────────────────┐
│ [icon] Network Priority Manager     │ ← AppTitleBar Grid (48px)
├─────────────────────────────────────┤
│                                     │
│  选择网络适配器:                     │
│  ┌──────────────────────────────┐   │
│  │ 适配器下拉框                  │   │
│  └──────────────────────────────┘   │
│                                     │
│  设置优先级（整数）:                 │
│  ┌──────────┐                       │
│  │ 10       │                       │
│  └──────────┘                       │
│                                     │
│  ┌──────────────┐ ┌──────────────┐  │
│  │   设置优先级  │ │   恢复默认    │  │
│  └──────────────┘ └──────────────┘  │
│                                     │
│  等待操作...                         │
│                                     │
└─────────────────────────────────────┘
```

### 2.2 XAML Layout

Root: `Grid` with two rows (Auto for title bar, * for content).

Title bar XAML:
```xml
<Grid x:Name="AppTitleBar" Height="48">
    <Grid.ColumnDefinitions>
        <ColumnDefinition x:Name="LeftPaddingColumn" Width="0"/>
        <ColumnDefinition/>
        <ColumnDefinition x:Name="RightPaddingColumn" Width="0"/>
    </Grid.ColumnDefinitions>
    <Image x:Name="TitleBarIcon" Source="ms-appx:///favicon.ico"
           Grid.Column="1" HorizontalAlignment="Left"
           Width="16" Height="16" Margin="16,0,0,0"/>
    <TextBlock x:Name="TitleBarTextBlock" Text="Network Priority Manager"
               Style="{StaticResource CaptionTextBlockStyle}"
               Grid.Column="1" VerticalAlignment="Center" Margin="40,0,0,0"/>
</Grid>
```

Content XAML: `StackPanel` with `Padding="24,16"`, `Spacing="12"`.

Controls:
- `ComboBox` (`x:Name="AdapterComboBox"`, `DropDownStyle` equivalent via `IsEditable="False"`)
- `TextBox` (`x:Name="PriorityTextBox"`, `Text="10"`)
- `Button` (`x:Name="SetPriorityButton"`, `Content="设置优先级"`, `Style="{StaticResource AccentButtonStyle}"`)
- `Button` (`x:Name="RestoreDefaultButton"`, `Content="恢复默认"`)
- `TextBlock` (`x:Name="StatusTextBlock"`, `Text="等待操作..."`)

### 2.3 Visual Properties

- **Mica backdrop:** `this.SystemBackdrop = new MicaBackdrop();` in `MainWindow` constructor. Win10 auto-falls back to solid color.
- **Theme:** No explicit `RequestedTheme`; follows system default.
- **Window sizing:** Fixed size via `MinWidth`/`MaxWidth`/`MinHeight`/`MaxHeight` set to same values to prevent resize while keeping compact form factor. Exact dimensions determined at implementation based on content measurements.
- **Caption buttons:** Transparent background to let Mica show through.

### 2.4 Status Colors (Theme-Aware)

Replace hardcoded `Color.Red` / `Color.Green` with theme-aware brushes:

| State | Original | WinUI 3 Brush |
|-------|----------|---------------|
| Success | `Color.Green` | `SystemFillColorSuccessBrush` |
| Error | `Color.Red` | `SystemFillColorCriticalBrush` |
| Normal | Default black | `TextFillColorPrimaryBrush` |

Implemented in code-behind via resource lookup:
```csharp
StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorSuccessBrush"];
```

## 3. Business Logic Migration

### 3.1 Control Mapping

| Original WinForms | WinUI 3 |
|-------------------|---------|
| `comboBoxAdapter.Items` | `AdapterComboBox.Items` |
| `comboBoxAdapter.SelectedIndex` | `AdapterComboBox.SelectedIndex` |
| `textBoxPriority.Text` | `PriorityTextBox.Text` |
| `statusLabel.Text` | `StatusTextBlock.Text` |
| `buttonSetPriority.Click` | `SetPriorityButton.Click` event |
| `buttonRestoreDefault.Click` | `RestoreDefaultButton.Click` event |

### 3.2 Reusable Code

The following logic is UI-framework-agnostic and copies directly:
- `NetworkInterface.GetAllNetworkInterfaces()` filtering
- `ProcessStartInfo` with `netsh interface ipv4 set interface ...`
- `int.TryParse` validation
- Exception handling blocks

### 3.3 Title Bar Initialization

```csharp
public MainWindow()
{
    this.InitializeComponent();
    ExtendsContentIntoTitleBar = true;
    SetTitleBar(AppTitleBar);
    this.SystemBackdrop = new MicaBackdrop();
    
    // Icon (only .ico supported for string path)
    var appWindow = this.AppWindow;
    appWindow.SetIcon("favicon.ico");
    
    // Set caption button backgrounds transparent for Mica
    if (AppWindowTitleBar.IsCustomizationSupported())
    {
        var titleBar = appWindow.TitleBar;
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
    }
    
    LoadAdapters();
}
```

### 3.4 Admin Privilege Note

MSIX + `runFullTrust` does **not** auto-elevate. The user must still right-click the app and select "Run as administrator" for `netsh` to succeed. This matches the original WinForms behavior.

## 4. Error Handling

- **Input validation:** Same logic (SelectedIndex check, TryParse check), status displayed via `StatusTextBlock`.
- **Process execution:** Same try/catch around `Process.Start`, stderr captured, status updated.
- **No WinUI-specific runtime errors expected:** XAML errors are compile-time; MSIX framework dependency is resolved by the OS.

## 5. Verification Criteria

| # | Check | Method |
|---|-------|--------|
| 1 | Build succeeds | `dotnet build` or Visual Studio Build |
| 2 | MSIX package generated | Check `bin\Release\net8.0-windows10.0.19041.0\msixpublish\` for `.msix` |
| 3 | Sideload installs | Right-click `.msix` → Install, or `Add-AppxPackage` |
| 4 | Adapter list populates | Run as admin, dropdown shows active adapters |
| 5 | Priority setting works | Set value, click "设置优先级", `netsh` succeeds, status green |
| 6 | Restore default works | Click "恢复默认", metric resets to 0, status green |
| 7 | Invalid input handled | Empty adapter / negative value → status red, no crash |
| 8 | Custom title bar renders | Icon + title visible, Mica background visible (Win11) |
| 9 | Theme switching | Change Windows dark/light mode, app auto-follows, text remains readable |
| 10 | Window non-resizable | Maximize disabled, borders do not resize |

## References

Official documentation downloaded locally:
- `docs/winui3-official/unpackage-winui-app.md`
- `docs/winui3-official/project-properties.md`
- `docs/winui3-official/use-windows-app-sdk-in-existing-project.md`
- `docs/winui3-official/simple-photo-viewer-winui3.md`
- `docs/winui3-official/title-bar.md`
- `docs/winui3-official/titlebar-design.md`
- `docs/winui3-official/tutorial-unpackaged-deployment.md`
- `docs/winui3-official/deploy-unpackaged-apps.md`
- `docs/winui3-official/desktop-winui3-app-with-basic-interop.md`
- `docs/winui3-official/winui3-index.md`
- `docs/winui3-official/start-here.md`
- `docs/winui3-official/title-bar-control.md`
