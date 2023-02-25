# FVPR Installer

A utility to easily install the FVPR repository to the VRChat Creator Companion.

<!-- Windows -->
<!--  https://github.com/FVPR/installer/releases/latest/download/windows.exe -->

<!-- Linux -->
<!--  https://github.com/FVPR/installer/releases/latest/download/linux -->

<!-- MacOS Intel -->
<!--  https://github.com/FVPR/installer/releases/latest/download/macos-x64 -->

<!-- MacOS Apple Silicon -->
<!-- https://github.com/FVPR/installer/releases/latest/download/macos-arm64 -->

[![Windows](https://img.shields.io/badge/Windows-blue?style=for-the-badge&logo=windows)](https://github.com/FVPR/installer/releases/latest/download/windows.exe) [![Linux](https://img.shields.io/badge/Linux-FCC624?style=for-the-badge&logo=linux&logoColor=black)](https://github.com/FVPR/installer/releases/latest/download/linux) [![macOS](https://img.shields.io/badge/macOS-Intel-blue?style=for-the-badge&logo=apple)](https://github.com/FVPR/installer/releases/latest/download/macos-x64) [![macOS](https://img.shields.io/badge/macOS-Apple%20Silicon-black?style=for-the-badge&logo=apple)](https://github.com/FVPR/installer/releases/latest/download/macos-arm64)



## Installation

### Windows

1. Download the latest installer from the Windows badge above.
2. Run the installer and follow the prompts.

### Mac & Linux

**Notes**
- In the instructions below, replace `./installer` with the name of the installer you downloaded
- Use the `x64` version for **Intel** Macs, and the `arm64` version for **Apple Silicon** Macs
- [Check if your Mac is using an Intel or Apple Silicon processor](https://www.howtogeek.com/706226/how-to-check-if-your-mac-is-using-an-intel-or-apple-silicon-processor)

**Instructions**
1. Download the latest installer from the Linux or MacOS badge above.
2. Open a terminal and navigate to the directory where the installer is located.
3. Run the `chmod +x ./installer` command to make the installer executable.
4. Run the `./installer` command to run it, and follow the prompts.

## Usage

| Argument | Description |
|----------|-------------|
| `--dev` | Installs the development version of the repository. <br/> Should only be user by developers and testers of FVPR. |

## Building

- Make sure you have the [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) installed.
- Clone the repository (`git clone https://github.com/FVPR/installer.git`)
- Open a terminal and navigate to the directory where the repository is located.
- Run the corresponding command for your platform from the list below.

| Platform | Architecture | Command |
|----------|---------|---------|
| Windows | x86-64 | `dotnet publish -c Release -r win-x64 --self-contained true` |
| Linux | x86-64 | `dotnet publish -c Release -r linux-x64 --self-contained true` |
| MacOS | Intel | `dotnet publish -c Release -r osx-x64 --self-contained true` |
| MacOS | Apple Silicon | `dotnet publish -c Release -r osx-arm64 --self-contained true` |

> You can safely ignore any `Trim analysis` warnings that appear in the terminal