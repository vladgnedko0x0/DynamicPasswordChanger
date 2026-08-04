<div align="center">

# 🔑 DynamicPasswordChanger

**Automatically rotates your Windows login password on every system startup**

[![C#](https://img.shields.io/badge/C%23-.NET_4.8-512BD4?style=flat-square&logo=csharp)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?style=flat-square&logo=windows)](https://www.microsoft.com/windows)
[![Service](https://img.shields.io/badge/Type-Windows_Service-grey?style=flat-square)](https://learn.microsoft.com/en-us/dotnet/framework/windows-services/)

<br/>

> On every system startup, the Windows Service automatically sets your account password
> to today's date in `yyyyMMdd` format. You always know the password — it's today's date.
> Anyone else who knows a previous password can't reuse it after the next restart.

</div>

---

## 📋 Table of Contents

- [How It Works](#-how-it-works)
- [Components](#-components)
- [Setup](#-setup)
- [Usage](#-usage)
- [Configuration](#-configuration)
- [Project Structure](#-project-structure)
- [Bug Fixes](#-bug-fixes)

---

## 🔬 How It Works

```
System boots
     │
     ▼
Windows Service Manager starts
"Dynamic Password Changer" (auto-start, runs as SYSTEM)
     │
     ▼
Service1.OnStart() fires
     │
     ├── Reads target username from App.config
     ├── Computes new password = DateTime.Now.ToString("yyyyMMdd")
     │                           e.g. "20240908" for September 8, 2024
     ├── Opens PrincipalContext(ContextType.Machine)
     ├── Finds the user by username
     └── Calls user.SetPassword(newPassword) + user.Save()
          │
          ▼
     Login screen appears
     User types today's date as password → access granted ✅
```

The password formula is public to the owner and secret to everyone else.
No file, database, or network call stores the password — it is always derivable
from the current date.

---

## 📦 Components

| Project | Type | Role |
|---|---|---|
| `DynamicPasswordService` | Windows Service (.NET 4.8) | Changes the password on startup via `PrincipalContext` |
| `DynamicPasswordService_Uploader` | Console App (.NET 4.7.2) | Installs and starts the service with `sc create` / `sc start` |
| `DynamicPassword` *(legacy)* | Console App | Original standalone password changer — superseded by the service |

---

## 🚀 Setup

### Prerequisites

- Windows 10 / 11
- .NET Framework 4.8 (pre-installed on Windows 10+)
- Administrator privileges

### 1 — Build

Open each `.sln` in Visual Studio and build in **Release** mode, or from the Developer Command Prompt:

```cmd
msbuild DynamicPasswordService\DynamicPasswordService.sln /p:Configuration=Release
msbuild DynamicPasswordService_Uploader\DynamicPasswordService_Uploader.sln /p:Configuration=Release
```

### 2 — Configure the target username

Edit `DynamicPasswordService\DynamicPasswordService\App.config` before building:

```xml
<appSettings>
  <add key="TargetUsername" value="YourWindowsUsername" />
</appSettings>
```

Replace `YourWindowsUsername` with the exact Windows account name
(the one shown on the login screen).

### 3 — Place the files

Put both executables in the **same folder**, for example:

```
C:\DynamicPasswordChanger\
├── DynamicPasswordService.exe     ← the service
└── DynamicPasswordService_Uploader.exe  ← the installer
```

### 4 — Run the installer

Right-click `DynamicPasswordService_Uploader.exe` → **Run as administrator**.

The installer will:
- Register `DynamicPasswordService.exe` as the `"Dynamic Password Changer"` Windows Service with `start= auto`
- Start the service immediately
- Log the result to the Windows Event Log (source: `DynamicPasswordChanger`)

### 5 — Verify

Open **Services** (`services.msc`) — you should see `Dynamic Password Changer` with status **Running**.

To check the Event Log: open **Event Viewer** → Windows Logs → Application, filter by source `DynamicPasswordChanger`.

---

## 🖥️ Usage

After installation, on every restart:

| Day | Password |
|---|---|
| September 8, 2024 | `20240908` |
| September 9, 2024 | `20240909` |
| January 1, 2025 | `20250101` |

The password is always **today's date in `yyyyMMdd` format**.

### Uninstall

Open an **Administrator** command prompt:

```cmd
sc stop "Dynamic Password Changer"
sc delete "Dynamic Password Changer"
```

---

## ⚙️ Configuration

`DynamicPasswordService\App.config`:

```xml
<appSettings>
  <!-- Windows account whose password is rotated on each startup -->
  <add key="TargetUsername" value="YourWindowsUsername" />
</appSettings>
```

---

## 📁 Project Structure

```
DynamicPasswordChanger/
├── DynamicPassword/                       # Legacy standalone changer (not used)
│   └── Program.cs                         # Changes password via PrincipalContext
│
├── DynamicPasswordService/                # Windows Service
│   ├── Service1.cs                        # OnStart: reads config, calls SetPassword
│   ├── Program.cs                         # ServiceBase.Run entry point
│   └── App.config                         # TargetUsername setting
│
└── DynamicPasswordService_Uploader/       # One-time installer
    └── Program.cs                         # sc create + sc start with WaitForExit
```

---

## 🐛 Bug Fixes

The following issues were fixed from the original version:

### 1 — Session 0 Isolation (critical)

**Original:** `Service1.OnStart()` launched `DynamicPassword.exe` using
`UseShellExecute = true` and `Verb = "runas"`. Windows Services run in
**Session 0** — a non-interactive system session with no desktop.
`UseShellExecute` requires an interactive shell, so the process either
failed silently or was invisible and did nothing.

**Fix:** Password-change logic moved directly into `Service1.OnStart()` using
`PrincipalContext`. The service runs as SYSTEM and already has the required
privileges — no subprocess or elevation is needed.

### 2 — Race condition in Uploader (critical)

**Original:**
```csharp
Process.Start(createInfo);  // sc create (async, returns immediately)
Process.Start(startInfo);   // sc start  (fires before create finishes)
```
`sc start` ran before `sc create` completed, causing a "service not found" error.

**Fix:**
```csharp
using (var p = Process.Start(createInfo))
{
    p.WaitForExit(); // wait for registration to complete
}
Thread.Sleep(500);  // let SCM process the new entry
Process.Start(startInfo);
```

### 3 — Wrong Desktop path in service context

**Original:**
```csharp
Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
// Returns C:\Windows\System32\config\systemprofile\Desktop for SYSTEM
```
`DynamicPassword.exe` was never found at that path.

**Fix:** Service executable path now resolved relative to the installer's own
directory via `AppDomain.CurrentDomain.BaseDirectory`.

### 4 — Hardcoded username

**Original:** `string username = "test";` — only worked for an account literally named "test".

**Fix:** Username read from `App.config` → `<add key="TargetUsername" value="..." />`.

---

<div align="center">

Made with C# · .NET 4.8 · Windows Service · PrincipalContext

</div>
