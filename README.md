# ReClass.NET PCSX2 Plugin

A plugin for [ReClass.NET](https://github.com/ReClassNET/ReClass.NET) that adds support for custom pointer resolution in PCSX2 (PlayStation 2 Emulator). This allows EE (Emotion Engine) memory addresses to be resolved and dereferenced within the ReClass.NET interface.
Special thanks to KN4CK3R for creating ReClass and pentoxine for the initial [Frostbite plugin](https://github.com/ReClassNET/ReClass.NET-FrostbitePlugin) which inspired this project.

<p align="center">
<img src="https://github.com/user-attachments/assets/ab66f383-ef69-4418-83d3-06e3447dc6d4">
</p>

## 📦 Features
- Custom pointer node `PCSX2PtrNode` that handles EE virtual memory translation.
- Automatically resolves in-game pointers using an emulated base address (EEMem).
- Designed for PCSX2-Qt (64-bit) emulator builds.
- Seamless integration with ReClass.NET's UI and node system.

## 🔧 Installation
1. Build the project or download the precompiled `.dll` file.
2. Place the compiled `.dll` into the `Plugins` folder of your ReClass.NET directory.
3. Start (or restart) ReClass.NET. The plugin will auto-register.

## 🔍 Usage
1. Attach ReClass.NET to the `pcsx2-qt` process.
2. Add a `PCSX2PtrNode` to your structure.
3. When this node is opened, it will read a 32-bit EE pointer, convert it to the correct mapped base address in PC memory, and render the pointed structure as 32-bit aligned.
4. Memory contents should now reflect PS2 EE memory mappings.

## ❗Notes
- Supports only 64-bit ReClass builds.
- Designed for use with 64-bit PCSX2 builds.
- Address translation assumes EE address space starts at `0x00000000` and is mapped to a known region in the PC process.

## Compiling
If you want to compile the ReClass.NET PCSX2 Plugin just fork the repository and create the following folder structure. If you don't use this structure you need to fix the project references.

```
..\ReClass.NET\
..\ReClass.NET\ReClass.NET\ReClass.NET.csproj
..\ReClass.NET-FrostbitePlugin
..\ReClass.NET-FrostbitePlugin\FrostbitePlugin.csproj
```

## 📜 License

MIT License — use freely, modify, and contribute!

## 🙋 Support

For bug reports, feature requests, or contributions, feel free to open an issue or pull request.
