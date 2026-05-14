# STranslate Anki Plugin

**[English Version](README_EN.md)**

> 声明：本项目由 DeepSeek v4 Pro 生成，并由我轻微校对和测试。若发现错误，尽管提交 Issue.

将 STranslate 翻译结果一键保存为 [Anki](https://apps.ankiweb.net/) 卡片。

## 前置条件

1. 安装并运行 Anki
2. 在 Anki 中安装 [AnkiConnect](https://ankiweb.net/shared/info/2055492159) 插件（代码: `2055492159`）
3. 重启 Anki，访问 `http://127.0.0.1:8765` 确认 AnkiConnect 运行正常

## 安装

打开 STranlate 设置 -> 插件 -> 安装 .spkg

> 备用方案：解压 .spkg（本质上是个 .zip）。将插件文件夹输出目录复制到 STranslate 安装目录的 Plugins 目录下（官方插件的位置）

## 配置

1. 在 STranslate 中添加本插件
2. STranslate -> 服务 -> 添加 Anki 并启用
3. 服务面板选中 Anki，点击「测试连接」确认与 Anki 通信正常
4. 选择目标牌组和笔记类型
5. 配置字段映射（源文本→哪个字段，翻译→哪个字段）
6. （可选）设置标签、去重策略

## 使用

1. 在 STranslate 中翻译文本
2. 点击翻译结果旁的「保存」按钮
3. 文本将自动添加为 Anki 卡片

![Add to Anki](images/add_to_anki.png)


## 去重

关闭「允许重复」后，插件添加前会检查目标字段是否已存在相同内容。已存在则跳过并提示。

# 构建

```shell
dotnet build Plugin\Plugin.csproj
```

插件生成在 `../.artifacts` 目录下。