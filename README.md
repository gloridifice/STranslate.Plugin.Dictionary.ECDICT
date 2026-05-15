# STranslate ECDICT Plugin

**[English Version](README_EN.md)** (TODO)

> 声明：本项目由 AI 辅助生成，并由人工校对和测试。若发现错误，欢迎提交 Issue.

为 [STranslate](https://github.com/ZGGSONG/STranslate) 添加本地英汉词典查询服务，基于 [ECDICT](https://github.com/skywind3000/ECDICT) 词典数据库。

## 功能特性

- **离线查询**：无需联网，本地 SQLite 数据库查询，响应迅速
- **76万+ 词条**：涵盖常用词汇、专业术语、地名人名等
- **词形还原**：支持动词时态、名词复数、形容词比较级等变体还原
  - 例如查询 `gave` 可还原到 `give`
- **模糊匹配**：支持 stripword 模糊匹配，例如 `long-time` 可匹配到 `longtime`
- **完整字典信息**：返回音标、中文释义、词性、词形变化、柯林斯星级、牛津三千词标签、BNC/当代语料库词频等

## 前置条件

1. 安装 [STranslate](https://github.com/ZGGSONG/STranslate)（支持插件的版本）

## 安装

1. 打开 STranslate 设置 -> 插件 -> 安装 `.spkg`
2. 选择 `STranslate.Plugin.Dictionary.ECDICT.spkg`

> 备用方案：解压 `.spkg`（本质上是 zip）。将插件文件夹复制到 STranslate 安装目录的 `Plugins` 目录下

## 配置

1. 在 STranslate 中添加本插件
2. STranslate -> 服务 -> 添加 **ECDICT** 并启用
3. （可选）在设置面板中调整：
   - 词典数据库路径（默认使用插件目录下的 `ecdict.db`）
   - 是否启用词形还原
   - 是否启用模糊匹配
   - 最大模糊匹配结果数

## 使用

1. 在 STranslate 中输入英文单词
2. 选择 **ECDICT** 作为字典服务
3. 查看查询结果：音标、中文释义、词形变化等

## 构建

```shell
# Debug 构建（默认）
dotnet build Plugin\Plugin.csproj

# Release 构建
dotnet build Plugin\Plugin.csproj -c Release

# 运行测试
dotnet test Plugin.Tests\Plugin.Tests.csproj
```

- Debug：生成在 `.artifacts/Debug/Plugins/STranslate.Plugin.Dictionary.ECDICT/` 目录下
- Release：生成在 `.artifacts/Release/Plugins/STranslate.Plugin.Dictionary.ECDICT/` 目录下，自动打包为 `.spkg`

## 项目结构

```
STranslate.Plugin.Dictionary.ECDICT/
├── Main.cs                          -- 插件入口，实现 DictionaryPluginBase
├── Settings.cs                      -- 插件配置
├── ECDictService.cs                 -- ECDICT 查询服务核心
├── WordEntry.cs                     -- 词条数据模型
├── plugin.json                      -- 插件元数据
├── icon.png                         -- 插件图标
├── ecdict.db                        -- SQLite 词典数据库
├── lemma.en.txt                     -- 词形还原数据
├── View/
│   └── SettingsView.xaml            -- 设置界面
├── ViewModel/
│   └── SettingsViewModel.cs         -- 设置界面 ViewModel
└── Languages/
    ├── zh-cn.xaml / zh-cn.json      -- 中文语言资源
    └── en.xaml / en.json            -- 英文语言资源
```

## 致谢

- [STranslate](https://github.com/ZGGSONG/STranslate) - 优秀的翻译软件
- [ECDICT](https://github.com/skywind3000/ECDICT) - skywind3000 提供的英汉词典数据库
