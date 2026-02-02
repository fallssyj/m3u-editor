# m3u-editor

<div align="center">

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-Desktop-512BD4?logo=windows)
![License](https://img.shields.io/badge/License-MIT-green.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)

基于 .NET 8 / WPF 的桌面级 IPTV M3U 列表 工具

</div>

![m3u-editor](https://raw.githubusercontent.com/fallssyj/m3u-editor/refs/heads/main/img/2026-02-02-090213.png)

## 功能特性

- 加载/保存 m3u 播放列表
- 支持拖动/上移/下移、插入、删除频道
- 动态列展示：自动解析 EXTINF 属性并生成列
- 列编辑器：支持新增、重命名、排序列/重命名
- 关键词搜索并可选择搜索列
- 一键导出 JSON
- 外部播放器调用（首次播放选择播放器路径）

## 配置文件

```json
{
    "ThemeMode": 1, //主题
    "PlayerPath": "C:\\*\\PotPlayerMini64.exe" //外部播放器路径
}

```

## 致谢

- MiSans
- HandyControls

