# DAC-70 Setup

A cross-platform app for configuring the [Datavideo DAC-70](https://www.datavideo.com/eu/product/DAC-70)
cross converter over USB. Its settings can be kept as named
configurations and applied to the converter again at any time.

The app is built with .NET MAUI as part of a bachelor's thesis at the Faculty of Informatics,
Masaryk University.


## Features

- Create, rename and delete named configurations, stored locally on the device
- Add settings to the named configurations
- Read the current settings from a connected DAC-70
- Apply a whole configuration, or only selected settings, to a connected DAC-70
- Export a configuration to a YAML or JSON file and import it back
- English and Czech user interface
- Light and dark theme


## Supported platforms

| Platform             | Minimum version                    | Status                                                                 |
|----------------------|------------------------------------|------------------------------------------------------------------------|
| Android              | 5.0 (API 21), with USB OTG         | Supported and tested                                                   |
| Windows              | 10 version 1809 (build 17763), x64 | Supported and tested                                                   |
| macOS (Mac Catalyst) | 12 Monterey                        | Should work, not tested yet                                            |
| iOS                  | 15.0                               | The app runs, but it cannot connect to the device: iOS does not give apps access to USB HID devices |


## Requirements

### Hardware

- A Datavideo DAC-70
- A USB cable to connect to it
- On Android, a device that supports USB host mode (often called USB OTG)

### Development

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- The .NET MAUI workload:
  ```sh
  dotnet workload install maui
  ```
- For Android: the Android SDK (installed together with Visual Studio, or through the workload)
- For macOS and iOS: a Mac with the latest Xcode
- Optionally an IDE: Visual Studio 2026 or JetBrains Rider


## Building and running

The solution is [src/DAC70Setup.slnx](src/DAC70Setup.slnx). In an IDE, open it, select the
`DAC70Setup` project, pick the target platform and run it.

From the command line, build and launch the app project for one platform at a time:

```sh
cd src

# Android
dotnet build DAC70Setup/DAC70Setup.csproj -t:Run -f net10.0-android

# Windows
dotnet build DAC70Setup/DAC70Setup.csproj -t:Run -f net10.0-windows10.0.19041.0

# macOS
dotnet build DAC70Setup/DAC70Setup.csproj -t:Run -f net10.0-maccatalyst
```

Release builds are created with `dotnet publish`:

```sh
dotnet publish DAC70Setup/DAC70Setup.csproj -c Release -f net10.0-android
dotnet publish DAC70Setup/DAC70Setup.csproj -c Release -f net10.0-windows10.0.19041.0
```

The Android build produces an `.apk` for sideloading. The Windows build is an unpackaged,
self-contained x64 app, so it runs without installing .NET or the Windows App SDK.


## Usage

1. Connect the DAC-70 to the computer, phone or tablet with a USB cable.
   On Android, the system offers to open the app when the converter is plugged in and asks for
   permission to access it.
2. Create a configuration on the *Configurations* page. It can be created empty, as a copy of another
   configuration, from the current state of the converter or from
   a YAML or JSON file.
3. Edit the settings you want to change.
4. Open the configuration's operations page and choose *Send to device* to apply it, or
   *Export to file* to save it.


## Project structure

| Project                                       | Purpose                                                        |
|-----------------------------------------------|----------------------------------------------------------------|
| [DAC70Setup](src/DAC70Setup)                  | The app head: platform entry points, app icon, splash screen   |
| [DAC70Setup.BL](src/DAC70Setup.BL)            | Pages, view models, controls and localized resources           |
| [DAC70Setup.Core](src/DAC70Setup.Core)        | Shared models, interfaces and exceptions                       |
| [DAC70Setup.Data](src/DAC70Setup.Data)        | Local storage of configurations in SQLite                      |
| [DAC70Setup.Transport](src/DAC70Setup.Transport) | USB communication with the converter                        |

The DAC-70 communicates as a USB HID device (vendor ID `0x1164`, product ID `0xB246`) using
64-byte reports. On Android the app uses the platform USB host API; on Windows and macOS it uses
[HidSharp](https://www.nuget.org/packages/HidSharp).


## Known limitations

- Firmware updates and factory reset are not supported. Use __DAC-70 Center__, the official
  Windows-only setup utility, for those.
- Settings of the `61` type cannot be read back from the device. The app marks them as
  *not readable*; they can still be edited and sent to the device.
- The macOS build has not been tested with a real device yet.
- iOS cannot communicate with the device at all (see [Supported platforms](#supported-platforms)).


## Acknowledgements

- The implementation of the USB protocol is built on the findings of the thesis
  [Setup utility for Datavideo DAC-70 cross-converter](https://is.muni.cz/th/z4oh4/)
  by Martin Janů.
- [IBM Plex](https://github.com/IBM/plex) fonts, under the
  [SIL Open Font License 1.1](https://openfontlicense.org/).
- [Font Awesome Free](https://fontawesome.com/license/free) icons, under
  [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) (the font files under the SIL OFL 1.1).


## License

Copyright 2026 Marek Balota. Released under the BSD 3-Clause License, see [LICENSE](LICENSE).

This is an independent project, not affiliated with, endorsed by or sponsored by Datavideo.
Datavideo is a registered trademark of Datavideo Technologies Co., Ltd. The names Datavideo,
DAC-70 and DAC-70 Center are used only to identify the supported device and its official software.
