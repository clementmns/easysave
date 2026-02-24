# ProSoft EasySave

```
███████╗ █████╗ ███████╗██╗   ██╗███████╗ █████╗ ██╗   ██╗███████╗
██╔════╝██╔══██╗██╔════╝╚██╗ ██╔╝██╔════╝██╔══██╗██║   ██║██╔════╝
█████╗  ███████║███████╗ ╚████╔╝ ███████╗███████║██║   ██║█████╗  
██╔══╝  ██╔══██║╚════██║  ╚██╔╝  ╚════██║██╔══██║╚██╗ ██╔╝██╔══╝  
███████╗██║  ██║███████║   ██║   ███████║██║  ██║ ╚████╔╝ ███████╗
╚══════╝╚═╝  ╚═╝╚══════╝   ╚═╝   ╚══════╝╚═╝  ╚═╝  ╚═══╝  ╚══════╝
```

**EasySave** is a backup job manager developed by **ProSoft**. It lets you define, run, monitor, and encrypt backup jobs — either through a cross-platform graphical interface or a fully interactive command-line application.

---

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Usage](#usage)
  - [GUI](#gui)
  - [CLI](#cli)
  - [Command-line Arguments](#command-line-arguments)
- [File Encryption with CryptoSoft](#file-encryption-with-cryptosoft)
- [Remote Logging with EasyLog.Server](#remote-logging-with-easylogserver)
- [Configuration](#configuration)
- [Running Tests](#running-tests)
- [Contributors](#contributors)
- [License](#license)

---

## Features

- **Full & Differential backups** — copy everything or only changed files.
- **Parallel execution** — run multiple backup jobs concurrently with configurable concurrency limits.
- **Priority file ordering** — files matching configured extensions (e.g. `.pdf`, `.docx`) are transferred before all others.
- **Large-file serialization** — files above a configurable size threshold are transferred one at a time to protect bandwidth.
- **Pause / Resume / Stop** — full lifecycle control over every running job.
- **Business software monitoring** — all running jobs are automatically paused when a configured business application (e.g. your ERP) is detected, and automatically resumed when it exits.
- **File encryption** — encrypt backed-up files on the fly using the companion **CryptoSoft** tool (XOR or AES-256).
- **Structured logging** — write logs in JSON or XML format, locally or to a remote TCP log server.
- **Multi-language** — English, French, Spanish, German, and Italian.
- **Two front-ends sharing one core** — a desktop GUI (Avalonia) and a keyboard-navigable console TUI.

---

## Architecture

The solution is organised as six projects:

| Project | Type | Role |
|---|---|---|
| `EasySave.Core` | Class Library | Shared business logic — models, services, strategies |
| `EasySave.GUI` | WinExe (Avalonia) | Cross-platform desktop GUI (MVVM) |
| `EasySave.CLI` | Exe | Interactive console TUI |
| `EasyLog` | Class Library | Pluggable JSON/XML logger with optional TCP remote output |
| `EasyLog.Server` | Exe / Docker | TCP log aggregation server |
| `CryptoSoft` | Exe | Standalone file encryption tool (required for encryption) |

Design patterns used throughout: **Strategy** (backup types, log formats), **Observer** (real-time job state), **Singleton** (settings, job factory, transfer limiter), **MVVM** (both frontends).

---

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022+ or JetBrains Rider (optional, for IDE support)
- **CryptoSoft** — required if you want to encrypt files during backup (see [File Encryption with CryptoSoft](#file-encryption-with-cryptosoft))
- Docker (optional, for running `EasyLog.Server`)

---

## Installation

```bash
# 1. Clone the repository
git clone https://github.com/your-org/easysave.git
cd easysave

# 2. Build the entire solution
dotnet build

# 3. (Optional) Run the tests
dotnet test
```

---

## Usage

### GUI

```bash
dotnet run --project EasySave.GUI
```

The main window shows all configured backup jobs in a data grid. Use the toolbar to:

- **Add / Edit / Delete** jobs
- **Play / Pause / Stop** individual or all jobs
- Open **Settings** to configure encryption, logging, language, and business software monitoring

### CLI

```bash
dotnet run --project EasySave.CLI
```

Navigate the interactive menu with arrow keys and `Enter`. Available actions:

- View all jobs and their status
- Add or delete a job
- Execute one or more selected jobs
- Execute all jobs
- Change language or log format
- Add the application to your system `PATH`

### Command-line Arguments

Both the GUI and CLI executables support headless execution via arguments. Pass job indices (1-based) or ranges:

```bash
# Run job 1
EasySave.GUI 1

# Run jobs 2 and 4
EasySave.GUI 2,4

# Run jobs 1 through 3
EasySave.GUI 1-3

# Combine ranges and individual indices
EasySave.GUI 1-3,5
```

When arguments are provided the application runs the specified jobs non-interactively, prints results to stdout, and exits without opening the UI.

---

## File Encryption with CryptoSoft

To encrypt files during a backup, you must **download and install CryptoSoft**.

> **CryptoSoft is a required companion tool for encryption.** EasySave will not encrypt files if the CryptoSoft executable is not present or not configured.

### Setup

1. Download the latest `CryptoSoft` release and place the executable somewhere on your machine.
2. Open **Settings** in EasySave and set the **CryptoSoft path** to point to the executable.
3. Set the **encrypted file extensions** list (e.g. `.txt,.docx,.pdf`) — only files matching these extensions will be encrypted.

### How it works

- During a backup, EasySave calls `CryptoSoft.exe` as a subprocess for each file that matches the configured extension list.
- The call is **security-hardened**: EasySave signs a UTC Unix timestamp with a 4096-bit RSA private key and passes the signature to CryptoSoft via environment variables. CryptoSoft verifies the signature and rejects any call older than 2 seconds, preventing replay attacks.
- CryptoSoft enforces **single-instance** per machine via a named Mutex — concurrent encryption requests are queued automatically.
- Keys are stored in `%AppData%\ProSoft\EasySave\keys\` and are protected via OS-level ACL (Windows) or `chmod 700` (Unix).

### Encryption algorithms (configured in CryptoSoft)

| Algorithm | Notes                                                                                           |
|---|-------------------------------------------------------------------------------------------------|
| **XOR** | Fast, lightweight — suitable for obfuscation                                                    |
| **AES-256** | Strong encryption; Initialization Vector is added at the beginning of the encrypted output file |

---

## Remote Logging with EasyLog.Server

EasySave can ship logs to a remote TCP server for centralised collection. Ensure that Docker is already downloaded and started.

### Start the server with Docker

```bash
docker-compose up -d
```

The server listens on port **5092** and writes received log entries to date-based files on disk.

### Configure EasySave

In **Settings**, set:

- **Log mode**: `Remote` or `Both`
- **Remote host**: the IP/hostname of the server (default `localhost`)
- **Remote port**: `5092`

---

## Configuration

All settings are stored in `%AppData%\ProSoft\EasySave\settings.json`.

| Setting | Description |
|---|---|
| Language | UI language (`en-US`, `fr-FR`, `es-ES`, `de-DE`, `it-IT`) |
| Log format | `JSON` or `XML` |
| Log mode | `Local`, `Remote`, or `Both` |
| Remote log host / port | Address of the `EasyLog.Server` instance |
| Encrypted file extensions | Comma-separated list of extensions to encrypt (requires CryptoSoft) |
| CryptoSoft path | Absolute path to the `CryptoSoft` executable |
| Priority file extensions | Files with these extensions are transferred first within a job |
| Business software process name | Process name(s) that trigger automatic job pause |
| Max parallel transfer size (KB) | Files larger than this value are serialized (one at a time) |

---

## Running Tests

```bash
dotnet test EasySave.Tests
```

The test suite covers `BackupExecutor`, `BackupJobFactory`, and `TransferLimitService` using xUnit.

---

## Contributors

- MIGNOT--PILON Antonin
- FARDELLA Timothé
- BOUZJALLIKHT Yanis
- OMNÈS Clément

---

## License

Distributed under the MIT License. See [`LICENSE`](LICENSE) for more information.
