
<center>
<h1 style="background: linear-gradient(to right, #0bc, #2cb, #0bc, #09c, #36b, #2cb); background-clip: text; color: transparent;">
  AuralizeBlazor
</h1>
</center>

![alt text](https://raw.githubusercontent.com/fgilde/AuralizeBlazor/master/AuralizeBlazor/screenshot1.png)

AuralizeBlazor provides a component named `BlazorAudioVisualizer`.
This a Blazor component that integrates the powerful features of the audioMotion.js library, enabling developers to incorporate real-time audio visualization into Blazor applications with ease. This document provides an overview of how to use the component, including its properties and methods.

## Features

- High-resolution, real-time audio spectrum visualization
- Support for various frequency scales (e.g., logarithmic, linear)
- Customizable sensitivity and FFT size
- Dual-channel visualization with adjustable layouts
- Fullscreen and picture-in-picture modes
- Extensible with additional features and visual effects

## Getting Started

To use `BlazorAudioVisualizer` in your Blazor application, first ensure the component library is added to your project. Then, you can incorporate the visualizer into your pages or components.

### 1. Installation

Make sure you have installed the `AuralizeBlazor` package in your project. If it's not installed, add it via NuGet package manager or CLI.

### 2. Usage

Add the namespace to your `_Imports.razor`:

```c#
@using AuralizeBlazor
```

### 3. Example
After adding the namespace, you can use the `BlazorAudioVisualizer` component in your pages or components. Here's an example of how to use the component:

```html
<BlazorAudioVisualizer        
    Radial="true"
    Height="400px"
    Width="100%">    
    <audio src="path/to/audio1.mp3" controls></audio>
</BlazorAudioVisualizer>

```