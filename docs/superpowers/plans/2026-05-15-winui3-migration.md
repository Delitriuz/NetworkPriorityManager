# WinForms to WinUI 3 Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate NetworkPriorityManager from .NET 8 WinForms to WinUI 3 with feature parity, MSIX packaging, custom title bar, Mica backdrop, and system theme support.

**Architecture:** In-place migration: modify existing `.csproj`, replace WinForms entry point and `Form1` with WinUI 3 `App`/`App.xaml`/`MainWindow`, add MSIX manifest and app manifest, keep all business logic intact.

**Tech Stack:** .NET 8, WinUI 3 (Windows App SDK 1.6+), MSIX (single-project), C# 12

---

## File Structure

| File | Action | Responsibility |
|------|--------|---------------|
| `NetworkPriorityManager.csproj` | Modify | SDK references, target framework, WinUI/MSIX properties |
| `Package.appxmanifest` | Create | MSIX package identity, capabilities (`runFullTrust`), visual assets |
| `app.manifest` | Create | DPI awareness (PerMonitorV2) |
| `App.xaml` | Create | Application root XAML, WinUI resources |
| `App.xaml.cs` | Create | Application entry point (`OnLaunched`) |
| `MainWindow.xaml` | Create | Window chrome, custom title bar, content layout |
| `MainWindow.xaml.cs` | Create | Business logic: adapter enumeration, netsh execution, status updates |
| `Program.cs` | Delete | Replaced by `App.xaml.cs` |
| `Form1.cs` | Delete | Replaced by `MainWindow.xaml.cs` |
| `Form1.Designer.cs` | Delete | Replaced by `MainWindow.xaml` |
| `Form1.resx` | Delete | WinForms resource file, no longer needed |
| `NetworkPriorityManager.sln` | Keep | References same `.csproj`, no changes needed |
| `favicon.ico` | Keep | Window icon via `AppWindow.SetIcon`, XAML Image source |

---

## Task 1: Project Configuration

**Files:**
- Modify: `NetworkPriorityManager.csproj`
- Create: `app.manifest`
- Create: `Package.appxmanifest`

- [ ] **Step 1: Rewrite `.csproj` for WinUI 3**

  Replace the entire contents of `NetworkPriorityManager.csproj`:

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
      <ApplicationIcon>favicon.ico</ApplicationIcon>
    </PropertyGroup>
    <ItemGroup>
      <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.6.*" />
      <PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.*" />
      <Content Include="favicon.ico" />
    </ItemGroup>
  </Project>
  ```

- [ ] **Step 2: Create `app.manifest`**

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

- [ ] **Step 3: Create `Package.appxmanifest`**

  ```xml
  <?xml version="1.0" encoding="utf-8"?>
  <Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
           xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
           xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
           IgnorableNamespaces="uap rescap">
    <Identity Name="NetworkPriorityManager"
              Publisher="CN=NetworkPriorityManager"
              Version="1.0.0.0"
              ProcessorArchitecture="x64" />
    <Properties>
      <DisplayName>Network Priority Manager</DisplayName>
      <PublisherDisplayName>NetworkPriorityManager</PublisherDisplayName>
      <Logo>favicon.ico</Logo>
    </Properties>
    <Dependencies>
      <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.26100.0" />
      <PackageDependency Name="Microsoft.WindowsAppRuntime.1.6" MinVersion="6000.311.2004.0" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US" />
    </Dependencies>
    <Resources>
      <Resource Language="x-generate" />
    </Resources>
    <Applications>
      <Application Id="App"
                   Executable="NetworkPriorityManager.exe"
                   EntryPoint="Windows.FullTrustApplication">
        <uap:VisualElements DisplayName="Network Priority Manager"
                            Description="Set network adapter priority"
                            BackgroundColor="transparent"
                            Square150x150Logo="favicon.ico"
                            Square44x44Logo="favicon.ico">
          <uap:DefaultTile Wide310x150Logo="favicon.ico" />
        </uap:VisualElements>
      </Application>
    </Applications>
    <Capabilities>
      <rescap:Capability Name="runFullTrust" />
    </Capabilities>
  </Package>
  ```

- [ ] **Step 4: Commit**

  ```bash
  git add NetworkPriorityManager.csproj app.manifest Package.appxmanifest
  git commit -m "chore: configure project for WinUI 3 and MSIX"
  ```

---

## Task 2: Application Entry Point

**Files:**
- Create: `App.xaml`
- Create: `App.xaml.cs`

- [ ] **Step 1: Create `App.xaml`**

  ```xml
  <Application
      x:Class="NetworkPriorityManager.App"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
      <Application.Resources>
          <ResourceDictionary>
              <ResourceDictionary.MergedDictionaries>
                  <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
              </ResourceDictionary.MergedDictionaries>
          </ResourceDictionary>
      </Application.Resources>
  </Application>
  ```

- [ ] **Step 2: Create `App.xaml.cs`**

  ```csharp
  using Microsoft.UI.Xaml;

  namespace NetworkPriorityManager
  {
      public partial class App : Application
      {
          private Window? m_window;

          public App()
          {
              this.InitializeComponent();
          }

          protected override void OnLaunched(LaunchActivatedEventArgs args)
          {
              m_window = new MainWindow();
              m_window.Activate();
          }
      }
  }
  ```

- [ ] **Step 3: Commit**

  ```bash
  git add App.xaml App.xaml.cs
  git commit -m "feat: add WinUI 3 application entry point"
  ```

---

## Task 3: Main Window XAML

**Files:**
- Create: `MainWindow.xaml`

- [ ] **Step 1: Create `MainWindow.xaml`**

  ```xml
  <Window
      x:Class="NetworkPriorityManager.MainWindow"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      Title="Network Priority Manager">

      <Grid>
          <Grid.RowDefinitions>
              <RowDefinition Height="Auto"/>
              <RowDefinition/>
          </Grid.RowDefinitions>

          <Grid x:Name="AppTitleBar" Height="48">
              <Grid.ColumnDefinitions>
                  <ColumnDefinition x:Name="LeftPaddingColumn" Width="0"/>
                  <ColumnDefinition/>
                  <ColumnDefinition x:Name="RightPaddingColumn" Width="0"/>
              </Grid.ColumnDefinitions>
              <Image x:Name="TitleBarIcon"
                     Source="ms-appx:///favicon.ico"
                     Grid.Column="1"
                     HorizontalAlignment="Left"
                     Width="16" Height="16"
                     Margin="16,0,0,0"/>
              <TextBlock x:Name="TitleBarTextBlock"
                         Text="Network Priority Manager"
                         Style="{StaticResource CaptionTextBlockStyle}"
                         Grid.Column="1"
                         VerticalAlignment="Center"
                         Margin="40,0,0,0"/>
          </Grid>

          <StackPanel Grid.Row="1" Padding="24,16" Spacing="12">
              <TextBlock Text="选择网络适配器:"/>
              <ComboBox x:Name="AdapterComboBox"/>

              <TextBlock Text="设置优先级（整数）："/>
              <TextBox x:Name="PriorityTextBox" Text="10"/>

              <StackPanel Orientation="Horizontal" Spacing="12">
                  <Button x:Name="SetPriorityButton"
                          Content="设置优先级"
                          Style="{StaticResource AccentButtonStyle}"
                          Click="SetPriorityButton_Click"/>
                  <Button x:Name="RestoreDefaultButton"
                          Content="恢复默认"
                          Click="RestoreDefaultButton_Click"/>
              </StackPanel>

              <TextBlock x:Name="StatusTextBlock" Text="等待操作..."/>
          </StackPanel>
      </Grid>
  </Window>
  ```

- [ ] **Step 2: Commit**

  ```bash
  git add MainWindow.xaml
  git commit -m "feat: add MainWindow XAML with custom title bar and layout"
  ```

---

## Task 4: Main Window Code-Behind

**Files:**
- Create: `MainWindow.xaml.cs`

- [ ] **Step 1: Create `MainWindow.xaml.cs`**

  ```csharp
  using Microsoft.UI;
  using Microsoft.UI.Windowing;
  using Microsoft.UI.Xaml;
  using Microsoft.UI.Xaml.Controls;
  using Microsoft.UI.Xaml.Media;
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Net.NetworkInformation;
  using Windows.Graphics;

  namespace NetworkPriorityManager
  {
      public sealed partial class MainWindow : Window
      {
          private List<NetworkInterface> _adapters = new List<NetworkInterface>();
          private AppWindow m_AppWindow = null!;

          public MainWindow()
          {
              this.InitializeComponent();

              ExtendsContentIntoTitleBar = true;
              SetTitleBar(AppTitleBar);
              this.SystemBackdrop = new Microsoft.UI.Composition.SystemBackdrops.MicaBackdrop();

              m_AppWindow = this.AppWindow;
              m_AppWindow.SetIcon("favicon.ico");

              if (AppWindowTitleBar.IsCustomizationSupported())
              {
                  var titleBar = m_AppWindow.TitleBar;
                  titleBar.ButtonBackgroundColor = Colors.Transparent;
                  titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
              }

              SetFixedWindowSize(540, 380);
              Activated += MainWindow_Activated;
              LoadAdapters();
          }

          private void SetFixedWindowSize(int width, int height)
          {
              m_AppWindow.Resize(new SizeInt32(width, height));
              if (m_AppWindow.Presenter is OverlappedPresenter presenter)
              {
                  presenter.IsResizable = false;
                  presenter.IsMaximizable = false;
              }
          }

          private void LoadAdapters()
          {
              _adapters = NetworkInterface.GetAllNetworkInterfaces()
                  .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                               !ni.Name.ToLower().Contains("loopback"))
                  .ToList();

              AdapterComboBox.Items.Clear();
              foreach (var adapter in _adapters)
              {
                  AdapterComboBox.Items.Add(adapter.Name);
              }

              if (AdapterComboBox.Items.Count > 0)
                  AdapterComboBox.SelectedIndex = 0;
          }

          private void SetPriorityButton_Click(object sender, RoutedEventArgs e)
          {
              try
              {
                  if (AdapterComboBox.SelectedIndex == -1)
                  {
                      StatusTextBlock.Text = "⚠️ 请选择一个网络适配器";
                      StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
                      return;
                  }

                  if (!int.TryParse(PriorityTextBox.Text, out int metric) || metric < 0)
                  {
                      StatusTextBlock.Text = "⚠️ 优先级必须是非负整数";
                      StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
                      return;
                  }

                  string adapterName = _adapters[AdapterComboBox.SelectedIndex].Name;
                  SetAdapterPriority(adapterName, metric);
              }
              catch (Exception ex)
              {
                  StatusTextBlock.Text = $"⚠️ 错误: {ex.Message}";
                  StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
              }
          }

          private void RestoreDefaultButton_Click(object sender, RoutedEventArgs e)
          {
              try
              {
                  if (AdapterComboBox.SelectedIndex == -1)
                  {
                      StatusTextBlock.Text = "⚠️ 请选择一个网络适配器";
                      StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
                      return;
                  }

                  string adapterName = _adapters[AdapterComboBox.SelectedIndex].Name;
                  SetAdapterPriority(adapterName, 0);
                  PriorityTextBox.Text = "10";
              }
              catch (Exception ex)
              {
                  StatusTextBlock.Text = $"⚠️ 错误: {ex.Message}";
                  StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
              }
          }

          private void SetAdapterPriority(string adapterName, int metric)
          {
              try
              {
                  ProcessStartInfo startInfo = new ProcessStartInfo
                  {
                      FileName = "netsh",
                      Arguments = $"interface ipv4 set interface \"{adapterName}\" metric={metric}",
                      RedirectStandardOutput = true,
                      RedirectStandardError = true,
                      UseShellExecute = false,
                      CreateNoWindow = true
                  };

                  using (Process process = new Process { StartInfo = startInfo })
                  {
                      process.Start();
                      string output = process.StandardOutput.ReadToEnd();
                      string error = process.StandardError.ReadToEnd();
                      process.WaitForExit();

                      if (process.ExitCode == 0)
                      {
                          StatusTextBlock.Text = $"✅ {adapterName} 的优先级已设置为 {metric}";
                          StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorSuccessBrush"];
                      }
                      else
                      {
                          StatusTextBlock.Text = $"❌ 错误: {error.Trim()}";
                          StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
                      }
                  }
              }
              catch (Exception ex)
              {
                  StatusTextBlock.Text = $"❌ 错误: {ex.Message}";
                  StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
              }
          }

          private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
          {
              if (args.WindowActivationState == WindowActivationState.Deactivated)
              {
                  TitleBarTextBlock.Foreground =
                      (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
              }
              else
              {
                  TitleBarTextBlock.Foreground =
                      (SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
              }
          }
      }
  }
  ```

- [ ] **Step 2: Commit**

  ```bash
  git add MainWindow.xaml.cs
  git commit -m "feat: add MainWindow code-behind with adapter logic and title bar"
  ```

---

## Task 5: Remove Legacy WinForms Files

**Files:**
- Delete: `Program.cs`
- Delete: `Form1.cs`
- Delete: `Form1.Designer.cs`
- Delete: `Form1.resx`

- [ ] **Step 1: Delete old WinForms files**

  ```bash
  git rm Program.cs Form1.cs Form1.Designer.cs Form1.resx
  ```

- [ ] **Step 2: Commit**

  ```bash
  git commit -m "chore: remove legacy WinForms files"
  ```

---

## Task 6: Build Verification

**Files:**
- Verify: `NetworkPriorityManager.csproj`
- Verify: all new files compile together

- [ ] **Step 1: Restore NuGet packages**

  ```bash
  dotnet restore
  ```
  Expected: succeeds, downloads `Microsoft.WindowsAppSDK` and `Microsoft.Windows.SDK.BuildTools`.

- [ ] **Step 2: Build project**

  ```bash
  dotnet build
  ```
  Expected: Build succeeds with 0 errors, 0 warnings.

- [ ] **Step 3: Verify MSIX packaging output**

  Check that `bin\Debug\net8.0-windows10.0.19041.0\msixpublish\` or `bin\Debug\net8.0-windows10.0.19041.0\AppPackages\` contains `.msix` artifacts.

  If `dotnet build` does not produce MSIX, try:
  ```bash
  dotnet publish -c Release -p:PublishProfile=Properties\PublishProfiles\win10-x64.pubxml
  ```
  Or use Visual Studio: Build → Publish → Sideloading.

- [ ] **Step 4: Commit (if build succeeds)**

  ```bash
  git commit --allow-empty -m "build: verify WinUI 3 migration compiles"
  ```

---

## Task 7: Manual Functional Verification

**Prerequisites:** Windows Developer Mode enabled (Settings → Privacy & security → For developers → Developer Mode), or a code-signing certificate for the MSIX.

- [ ] **Step 1: Install MSIX locally**

  From an elevated PowerShell:
  ```powershell
  Add-AppxPackage -Path "bin\Release\net8.0-windows10.0.19041.0\AppPackages\NetworkPriorityManager_*_x64.msix"
  ```

- [ ] **Step 2: Run as Administrator**

  Launch from Start Menu or run:
  ```powershell
  Start-Process "shell:AppsFolder\$(Get-AppxPackage -Name NetworkPriorityManager | Select-Object -ExpandProperty PackageFamilyName)!App" -Verb runAs
  ```

- [ ] **Step 3: Functional checks**

  1. Window opens with custom title bar showing icon and "Network Priority Manager"
  2. Dropdown lists active network adapters (excluding loopback)
  3. Enter `5`, click **设置优先级** → status shows success in green
  4. Click **恢复默认** → status shows success, metric resets to 0
  5. Clear dropdown selection, click **设置优先级** → status shows error in red
  6. Enter `-1`, click **设置优先级** → status shows validation error in red
  7. Window cannot be maximized or resized
  8. Switch Windows theme dark/light → app follows automatically
  9. Mica background visible on Windows 11

- [ ] **Step 4: Uninstall test package**

  ```powershell
  Remove-AppxPackage -Package (Get-AppxPackage -Name NetworkPriorityManager).PackageFullName
  ```

---

## Self-Review

### Spec Coverage

| Spec Section | Plan Task |
|--------------|-----------|
| `.csproj` changes (`UseWinUI`, `EnableMsixTooling`) | Task 1, Step 1 |
| `Package.appxmanifest` with `runFullTrust` | Task 1, Step 3 |
| `app.manifest` for DPI | Task 1, Step 2 |
| `App.xaml` / `App.xaml.cs` entry point | Task 2 |
| Custom title bar + `SetTitleBar` | Task 3 (XAML), Task 4 (constructor) |
| Mica backdrop | Task 4, `MainWindow` constructor |
| Fixed window size + non-resizable | Task 4, `SetFixedWindowSize` |
| `favicon.ico` via `AppWindow.SetIcon` | Task 4, constructor |
| Adapter enumeration logic | Task 4, `LoadAdapters` |
| netsh process execution | Task 4, `SetAdapterPriority` |
| Theme-aware status brushes | Task 4, uses `SystemFillColorSuccessBrush` / `SystemFillColorCriticalBrush` |
| Title bar active/inactive dimming | Task 4, `MainWindow_Activated` |
| Delete old WinForms files | Task 5 |
| Build verification | Task 6 |
| Manual functional tests | Task 7 |

**No gaps found.**

### Placeholder Scan

- No "TBD", "TODO", "implement later" found.
- No vague instructions like "add appropriate error handling" — all error handling is explicit in the code.
- No "similar to Task N" shortcuts — each task is self-contained.
- All code blocks contain complete, compilable code.

### Type Consistency

- `m_AppWindow` defined in Task 4 and used consistently.
- `AdapterComboBox`, `PriorityTextBox`, `StatusTextBlock` names match XAML (`x:Name`) and code-behind references.
- `AppTitleBar`, `TitleBarTextBlock`, `TitleBarIcon` names match XAML and code-behind.
- `SetPriorityButton_Click` and `RestoreDefaultButton_Click` event handler names match XAML `Click` attributes.

**All consistent.**
