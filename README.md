> M3U 编辑器

导入现有的 M3U || json || txt 文件 
通过直观的界面<!-- more -->
对其中的频道进行编辑排序或添加、删除、重命名频道 
您可以根据自己的喜好 对播放列表进行定制 以满足您的观看需求 
打造完美的 IPTV 播放列表吧

### 示例
>json格式示例 channels.json

```json

[
  {
    "Tvgname": "xx",
    "Tvgid": "",
    "Tvglogo": "https://**********",
    "Grouptitle": "xx",
    "Name2": "xx",
    "Link": "http://**********"
  },
  {
    "Tvgname": "xx",
    "Tvgid": "",
    "Tvglogo": "https://**********",
    "Grouptitle": "xx",
    "Name2": "xx",
    "Link": "http://**********"
  }
]
```

>txt格式示例 channels.txt

```bash

Tvgname,Link
Tvgname,Link
Tvgname,Link

```

![m3u-editor](images/m3u-editor.png)

### 依赖
[Microsoft .NET 8.0 Desktop Runtime ](https://download.visualstudio.microsoft.com/download/pr/b280d97f-25a9-4ab7-8a12-8291aa3af117/a37ed0e68f51fcd973e9f6cb4f40b1a7/windowsdesktop-runtime-8.0.0-win-x64.exe)

### 源码

[GitHub](https://github.com/fallssyj/m3u-editor)

### 分流

[蓝奏](https://fallssyj.lanzoul.com/b014bnd0d) 


### Thanks to

-  [Prism](https://github.com/PrismLibrary/Prism)
-  [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
-  [MaterialDesignThemes](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
