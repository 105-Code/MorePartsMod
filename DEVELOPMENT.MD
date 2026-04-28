# Development Guide

This document covers how to get started developing this mod. Please note that you may encounter environment-specific issues that you will need to resolve on your own.

## Getting Started

### Dependencies

This mod uses many game `.dll` files for development. To obtain these files, go to your game installation folder and navigate to `Spaceflight Simulator_Data/Managed`. Copy all files from this folder and paste them into the `Dependencies` folder.

It is also necessary to have `.NET` installed on your computer. You can follow this official Microsoft [guide](https://dotnet.microsoft.com/en-us/learn/dotnet/hello-world-tutorial/install).

Finally, you must have Unity installed to use the `toolkit`.

### Environment Variables

Create a copy of the `env.props.example` file and rename it to `env.props`. Inside this file, you will find examples of how the values should be set. Below is an explanation of each variable:

- **CommandPath**: Path to the executable that helps automate moving the `.dll` to the `toolkit` and updating the `.pack` with the new code without recompiling it. This project includes the CLI in the `UpdatePackCli` folder, but it was compiled on **Linux**, so it may fail on **Windows**. If that happens, you can download the source code [here](https://github.com/105-Code/UpdatePack) and compile it again. The default value should be kept.

- **ModPack**: Path to the `.pack` already installed in the game. This is used to update the `.pack` code without recompiling it. This value must be changed to match your system path.

- **ModDll**: Path to the `.dll` generated when compiling this mod. Its default value should be kept.

- **ToolkitPath**: Path to the `toolkit`'s `DLLs` folder. It is used to automatically move the `.dll` into Unity.

### Toolkit

This mod adds new parts and buildings, so the Spaceflight Simulator `toolkit` is required. This project uses a modified `toolkit` that you can find at this [link](https://github.com/dani0105/modding-toolkit-sfs). It is the same as the official `toolkit` but includes additional scripts to prevent compilation errors when importing the `.dll` into Unity.

Once the project is opened in Unity and Odin is installed, you need to copy the mod’s `.dll` into the `DLLs` folder. You can obtain this `.dll` by building the project as explained in the `Building` section. At this step, you may encounter many errors, which cannot be covered in this guide due to their specificity. Therefore, you will need to investigate and resolve them yourself.

It is recommended to create a symbolic link of the `Toolkit` folder inside the `toolkit`, although it is not required. You can also copy the contents of this folder into the `toolkit`. If everything works correctly, when opening any mod part, you will see that it uses custom components from the mod.

## Building

### Windows

[This mod was developed in a Linux environment, but that does not mean it cannot be developed on Windows; quite the opposite, development on Windows should be easier. If someone manages to build this mod on Windows, feel free to update this guide.]

### Linux

Assuming `.NET` is installed on your system and you can use the `msbuild` or `xbuild` command.

The build process is simple. Just run `msbuild`, and the `.dll` file will be located at `bin/Debug/MorePartsMod.dll`. If your development environment and CLI are properly set up, the `.dll` will be automatically copied to Unity and updated in the installed `.pack`.