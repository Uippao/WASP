# Workspace Access and Storage Portal (WASP) - [LITE VERSION]

Welcome to the Workspace Access and Storage Portal (WASP)! This program is designed to handle WASP Workspace Files (`.wsp`), which can be used to automate the execution of various files, links, and commands in a structured manner. This means they are basically 'fancy shortcuts', as I like to call them.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Configuration](#configuration)

## Features

- **File Type Registration**: Automatically registers `.wsp` file type with the system.
- **Handling `.wsp` Files**: Reads and executes actions defined in `.wsp` files.
- **Support for Delays**: Supports delays before executing files, links, or commands.
- **Custom Working Directories**: Allows specifying custom working directories for executed files.
- **Shell Execution Options**: Supports various shell execution options for files and commands.
- **Error Handling**: Logs errors encountered during execution.

## Installation

1. Go to Releases, and download the `WASP-lite.zip` file.
2. Extract the ZIP file anywhere you'd like.
3. Run the application. It always requires Administrator so it can do everything it needs to. You can check the code, it doesn't do malicious stuff.
4. Now you're ready to start creating Workspace files!

## Usage

Run a `.wsp` file using WASP Lite. It will do every task defined in the file.

## Configuration

### Workspace File Structure

A `.wsp` file is a YAML file that can define files, links, and commands to be executed. Below is an example structure:

```yaml
files:
  - path: "C:\\path\\to\\file.exe"
    args: "/argument"
    workingDirectory: "C:\\custom\\directory"
    useShellExecute: true
    verb: "runas"
    maximized: true
    delay: 5000 # Delay in milliseconds
    order: 1

links:
  - url: "https://example.com"
    browser: "chrome.exe"
    windowStyle: 1 # 1 for Normal, 2 for Minimized, 3 for Maximized
    delay: 3000 # Delay in milliseconds
    order: 2

commands:
  - type: "powershell"
    script: "Write-Host 'Hello, World!'"
    runAsAdministrator: true
    delay: 2000 # Delay in milliseconds
    order: 3
```

You can remove optional parameters to use default values. More info by running the application itself, and typing 'help'.
You should always check the file content you made for errors using a YAML validator. YAML escape characters need to be doubled, this is why for example the backslashes are doubled.

Thank you for using WASP Lite! I might one day release a full version with a GUI, but you have this console app for now.
