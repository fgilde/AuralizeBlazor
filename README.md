

## AuralizeBlazor

[Live Demo](https://auralizeblazor.azurewebsites.net/)

[![QuickRun](https://quickrun.org/badge.svg)](https://quickrun.org/run?repo=fgilde/AuralizeBlazor)

![alt text](https://raw.githubusercontent.com/fgilde/AuralizeBlazor/master/AuralizeBlazor/screenshot1.png)

AuralizeBlazor provides a audio visualizer component named `Auralizer`.
This a Blazor component that integrates the powerful features of audioMotion, enabling developers to incorporate real-time audio visualization into Blazor applications with ease. This document provides an overview of how to use the component, including its properties and methods.

## Features

- High-resolution, real-time audio spectrum visualization
- Support for various frequency scales (e.g., logarithmic, linear)
- Customizable sensitivity and FFT size
- Dual-channel visualization with adjustable layouts
- Fullscreen and picture-in-picture modes
- Extensible with additional features and visual effects

## Getting Started

To use `Auralizer` in your Blazor application, first ensure the component library is added to your project. Then, you can incorporate the visualizer into your pages or components.

### 1. Installation

Make sure you have installed the `AuralizeBlazor` package in your project. If it's not installed, add it via NuGet package manager, CLI or PackageReference.

```bash
dotnet add package AuralizeBlazor
```
```xml
<PackageReference Include="AuralizeBlazor" Version="*" />
```

### 2. Usage

Add the namespace to your `_Imports.razor`:

```c#
@using AuralizeBlazor
```

### 3. Example
After adding the namespace, you can use the `Auralizer` component in your pages or components. Here's an example of how to use the component:

```html
<Auralizer        
    Radial="true"
    Height="400px"
    Width="100%">    
    <audio src="path/to/audio1.mp3" controls></audio>
</Auralizer>

```

## Web Component (without Blazor)
The Auralizer can also be used in any web page or framework as custom element. Include the loader and use the tag:

```html
<script src="https://auralizeblazor.azurewebsites.net/wc/auralizer.js"></script>

<auralize-blazor-auralizer height="400px" connect-all-audio-sources="true"></auralize-blazor-auralizer>
<audio src="path/to/audio1.mp3" controls></audio>
```

Strings, numbers and booleans are kebab-case attributes, everything else (enums, track list, presets) is set as camelCase JS property and events are functions (`el.onPresetApplied = e => ...`).
For production host the files yourself: `dotnet publish AuralizeBlazor.WebComponent -c Release` and upload `wwwroot`. See the [live documentation](https://auralizeblazor.azurewebsites.net/web-component) for details.


## License

MIT — see [LICENSE](LICENSE).

This package bundles [audioMotion-analyzer](https://github.com/hvianna/audioMotion-analyzer) (`wwwroot/js/lib/audioMotion4.5.4.min.js`), copyright (c) Henrique Avila Vianna, licensed under the [AGPL-3.0-or-later](https://www.gnu.org/licenses/agpl.html). That license applies to that file.
