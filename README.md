# M3U Editor

一个基于 WPF 的 M3U 文件编辑器

## ✨ 特性

- 🎨 **主题支持**: 自动适配深色/浅色模式
- 📁 **多格式支持**: 支持 M3U、JSON、TXT 文件格式
- 🔍 **智能搜索**: 支持按不同字段搜索和筛选
- 🖱️ **拖放操作**: 支持文件拖放打开
- 📋 **数据表格**: 直观的频道管理界面
- ⬆️⬇️ **排序功能**: 支持频道上移/下移操作
- 🏗️ **现代化架构**: 基于 Prism MVVM 模式开发

## 📦 安装

### 方式一：下载预编译版本

前往 [Releases 页面](https://github.com/fallssyj/m3u-editor/releases) 下载最新版本的压缩包，解压后运行 `m3u_editor.exe`。

### 方式二：从源码构建

1. 克隆项目：
```bash
git clone https://github.com/fallssyj/m3u-editor.git
cd m3u-editor
```

2. 构建项目：
```bash
cd src
.\build.ps1 -Configuration Release
```

3. 运行程序：
```bash
.\bin\m3u-editor\m3u_editor.exe
```

## 🔧 系统要求

- **操作系统**: Windows 10/11
- **运行时**: [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

## 🚀 使用方法

### 打开文件
- 点击"打开文件"按钮选择文件
- 或将文件直接拖放到程序窗口
- 支持的文件格式：`.m3u`, `.json`, `.txt`

### 编辑频道
- **添加频道**: 点击"+"按钮在选中位置添加新频道
- **删除频道**: 选中频道后点击"-"按钮删除
- **移动频道**: 使用"↑"和"↓"按钮调整频道顺序
- **编辑信息**: 直接在表格中编辑频道信息

### 搜索功能
- 在搜索框中输入关键词
- 选择搜索字段（名称、分组、链接等）
- 匹配的频道会高亮显示

### 保存文件
- **保存为 M3U**: 点击"保存为m3u"按钮
- **保存为 JSON**: 点击"保存为json"按钮
- 文件将保存在原文件同目录下

## 📋 文件格式示例

### JSON 格式示例 (channels.json)
```json
[
  {
    "Tvgname": "频道名称",
    "Tvgid": "频道ID",
    "Tvglogo": "https://example.com/logo.png",
    "Grouptitle": "分组名称",
    "Name2": "备用名称",
    "Link": "http://example.com/stream.m3u8"
  }
]
```

### TXT 格式示例 (channels.txt)
```bash
Tvgname,Link
央视一套,http://example.com/cctv1.m3u8
央视二套,http://example.com/cctv2.m3u8
```

### M3U 格式示例
```m3u
#EXTM3U
#EXTINF:-1 tvg-id="" tvg-name="央视一套" tvg-logo="https://example.com/cctv1.png" group-title="央视",央视一套
http://example.com/cctv1.m3u8
```

## 🖼️ 界面预览

![m3u-editor](https://raw.githubusercontent.com/fallssyj/m3u-editor/main/img/215411.png)

## 🛠️ 开发者指南

### 项目结构
```
src/
├── Common/           # 公共组件
│   ├── Models/      # 数据模型
│   └── Utils/       # 工具类
├── ViewModels/      # ViewModel 层
├── Views/           # View 层
├── Themes/          # 主题文件
└── Styles/          # 样式文件
```

### 构建项目
```bash
# 使用 PowerShell 构建
cd src
.\build.ps1 -Configuration Release

# 或者使用 dotnet CLI
dotnet build -c Release
```

### 技术栈
- **框架**: .NET 8.0 WPF
- **MVVM**: Prism Library
- **UI组件**: MaterialDesignInXamlToolkit
- **JSON处理**: Newtonsoft.Json
- **依赖注入**: DryIoc

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。


## 🙏 致谢

- [Prism](https://github.com/PrismLibrary/Prism) - MVVM 框架
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) - JSON 处理库
- [MaterialDesignThemes](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) - Material Design UI 组件