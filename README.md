# Network Priority Manager

一个用于管理 Windows 网络适配器优先级的桌面应用。基于 WinUI 3 构建，采用 Fluent Design 设计语言，支持自动暗色/亮色主题切换。

## 功能

- 枚举系统中所有活动的网络适配器（自动排除 Loopback）
- 为指定适配器设置 IPv4 接口跃点数（Metric）
- 一键恢复默认优先级（Metric = 0）
- 自定义标题栏 + Mica 背景效果
- 系统主题自动跟随（暗色/亮色）

## 系统要求

| 项目 | 要求 |
|------|------|
| 操作系统 | Windows 10 版本 1809 (17763) 或更高 |
| 运行时 | Windows App SDK 1.6+ |
| 架构 | x64, x86, ARM64 |
| 权限 | 需要管理员权限才能修改网络适配器优先级 |

## 安装

### 方式一：MSIX 安装包（推荐）

从 [Releases](../../releases) 页面下载最新 `.msix` 文件，双击安装。

> **注意**：首次安装自签名版本时，需要先安装根证书。在管理员 PowerShell 中运行：
> ```powershell
> certutil -addstore -f "Root" "NetworkPriorityManager_TemporaryKey.cer"
> certutil -addstore -f "TrustedPublisher" "NetworkPriorityManager_TemporaryKey.cer"
> ```

### 方式二：从源码构建

```powershell
# 克隆仓库
git clone https://github.com/Delitriuz/NetworkPriorityManager.git
cd NetworkPriorityManager

# 还原依赖
dotnet restore

# 构建（Release + x64）
dotnet build -c Release -r win-x64

# 生成的 MSIX 位于：
# bin/Release/net8.0-windows10.0.19041.0/win-x64/AppPackages/
```

## 使用说明

1. 启动应用（**必须以管理员身份运行**）
2. 从下拉框选择目标网络适配器
3. 输入优先级数值（非负整数，越小优先级越高）
4. 点击**设置优先级**
5. 如需恢复默认，点击**恢复默认**

> 非管理员运行时会收到 `netsh` 权限拒绝错误，这是预期行为。

## 项目结构

```
NetworkPriorityManager/
├── App.xaml / App.xaml.cs          # WinUI 3 应用入口
├── MainWindow.xaml / .xaml.cs      # 主窗口（UI + 业务逻辑）
├── Package.appxmanifest            # MSIX 包清单
├── app.manifest                    # DPI 感知配置
├── NetworkPriorityManager.csproj   # 项目文件
├── favicon.ico                     # 应用图标
├── *.png                           # MSIX 视觉资产
└── docs/superpowers/               # 设计与实施文档
```

## 版本历史

### v1.0.0 (2026-05-15)

- **迁移至 WinUI 3**：完整从 WinForms 迁移到 WinUI 3
- **UI 升级**：Fluent Design + Mica 背景 + 自动主题切换
- **自定义标题栏**：内容延伸至标题栏，48px 高度，应用图标 + 标题
- **MSIX 打包**：单项目 MSIX，支持 sideload 安装
- **主题感知状态色**：成功/错误状态使用系统主题色刷
- **代码质量**：移除 `this.` 冗余、匈牙利命名修正、本地化安全的字符串比较

## 技术栈

- .NET 8
- WinUI 3 (Windows App SDK 1.6)
- MSIX (单项目打包)
- C# 12

## 许可证

MIT