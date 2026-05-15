# STranslate ECDICT Plugin

**[中文版本](README.md)**

> Disclaimer: This project was generated with AI assistance, with manual review and testing. Feel free to submit an Issue if you find any bugs.

Add a local English-Chinese dictionary query service for [STranslate](https://github.com/ZGGSONG/STranslate), based on the [ECDICT](https://github.com/skywind3000/ECDICT) dictionary database.

## Features

- **Offline Query**: No internet required. Local SQLite database queries with fast response
- **760,000+ Entries**: Covers common vocabulary, professional terms, place names, etc.
- **Lemmatization**: Supports verb tenses, noun plurals, adjective comparatives, and other variants
  - e.g., querying `gave` can be lemmatized to `give`
- **Fuzzy Matching**: Supports stripword fuzzy matching, e.g., `long-time` matches `longtime`
- **Full Dictionary Info**: Returns phonetic symbols, Chinese definitions, part of speech, word forms, Collins star rating, Oxford 3000 label, BNC/contemporary corpus frequency, etc.

## Prerequisites

1. Install [STranslate](https://github.com/ZGGSONG/STranslate) (version that supports plugins)

## Installation

1. Open STranslate Settings → Plugins → Install `.spkg`
2. Select `STranslate.Plugin.Dictionary.ECDICT.spkg`

> Fallback: Extract the `.spkg` (essentially a zip). Copy the plugin folder to the `Plugins` directory under the STranslate installation directory.

## Configuration

1. Add this plugin in STranslate
2. STranslate → Services → Add **ECDICT** and enable it
3. (Optional) Adjust in the settings panel:
   - Dictionary database path (defaults to `ecdict.db` in the plugin directory)
   - Enable lemmatization
   - Enable fuzzy matching
   - Maximum fuzzy match results

## Usage

1. Enter an English word in STranslate
2. Select **ECDICT** as the dictionary service
3. View the query results: phonetic symbols, Chinese definitions, word forms, etc.

## Build

```shell
# Debug build (default)
dotnet build Plugin\Plugin.csproj

# Release build
dotnet build Plugin\Plugin.csproj -c Release

# Run tests
dotnet test Plugin.Tests\Plugin.Tests.csproj
```

- Debug: output to `.artifacts/Debug/Plugins/STranslate.Plugin.Dictionary.ECDICT/`
- Release: output to `.artifacts/Release/Plugins/STranslate.Plugin.Dictionary.ECDICT/`, auto-packaged as `.spkg`

## Project Structure

```
STranslate.Plugin.Dictionary.ECDICT/
├── Main.cs                          -- Plugin entry, implements DictionaryPluginBase
├── Settings.cs                      -- Plugin configuration
├── ECDictService.cs                 -- ECDICT query service core
├── WordEntry.cs                     -- Entry data model
├── plugin.json                      -- Plugin metadata
├── icon.png                         -- Plugin icon
├── ecdict.db                        -- SQLite dictionary database
├── lemma.en.txt                     -- Lemmatization data
├── View/
│   └── SettingsView.xaml            -- Settings UI
├── ViewModel/
│   └── SettingsViewModel.cs         -- Settings ViewModel
└── Languages/
    ├── zh-cn.xaml / zh-cn.json      -- Chinese language resources
    └── en.xaml / en.json            -- English language resources
```

## Acknowledgements

- [STranslate](https://github.com/ZGGSONG/STranslate) - Excellent translation software
- [ECDICT](https://github.com/skywind3000/ECDICT) - English-Chinese dictionary database by skywind3000
