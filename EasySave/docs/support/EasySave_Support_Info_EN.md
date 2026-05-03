# EasySave — Technical Support Information

## Target version

EasySave v1.1.0

## Application type

EasySave v1.1 is a console application developed in C# with .NET 10.0.

The software allows users to create and execute up to 5 backup jobs in complete or differential mode.

---

## Minimum configuration

- Operating system: Windows 10 or later
- Framework: .NET 10.0
- Required permissions:
  - read access to the source directory;
  - write access to the target directory;
  - write access to the execution directory for configuration, log and state files.

---

## Default application location

The application is executed from the build or installation folder.

In a development environment:

```text
EasySave/bin/Debug/net10.0/
```

In a release environment:

```text
EasySave/bin/Release/net10.0/
```

---

## Generated files

Generated files are placed next to the executable using `AppContext.BaseDirectory`.

---

## Backup jobs configuration file

```text
jobs.config.json
```

This file contains the list of configured backup jobs:

- job identifier;
- backup name;
- source path;
- target path;
- backup type;
- last execution date.

Example:

```json
[
  {
    "Id": 1,
    "Name": "Test_Backup",
    "SourcePath": "C:\\Users\\HP\\Desktop\\source_test",
    "TargetPath": "C:\\Users\\HP\\Desktop\\target_test",
    "Type": 0,
    "LastExecutionTime": "2026-05-01T13:22:00+02:00"
  }
]
```

> `Type = 0`: complete backup  
> `Type = 1`: differential backup

---

## Settings file

```text
settings.json
```

This file contains user settings:

- selected language;
- selected log format: JSON or XML.

Example:

```json
{
  "LogFormat": 1,
  "Language": "fr"
}
```

> `LogFormat = 0`: JSON  
> `LogFormat = 1`: XML

---

## Real-time state file

```text
state.json
```

This file contains the progress status of the backup job.

Recorded information:

- job name;
- timestamp of the last action;
- job status;
- total number of files;
- total file size;
- number of remaining files;
- remaining size;
- current source file;
- current target file;
- progression.

Example:

```json
{
  "BackupName": "Test_Backup",
  "LastActionTime": "2026-05-01T13:22:00+02:00",
  "Status": "Terminé",
  "TotalFiles": 1,
  "TotalSize": 14,
  "RemainingFiles": 0,
  "RemainingSize": 0,
  "CurrentSourceFile": "",
  "CurrentTargetFile": "",
  "Progression": 100
}
```

The statuses used are:

```text
Actif
Terminé
Erreur
```

---

## Daily log files

Logs are stored in the following folder:

```text
logs/
```

Files are generated using the current date.

Possible formats:

```text
YYYY-MM-DD.json
YYYY-MM-DD.xml
```

Each log entry contains:

- timestamp;
- job name;
- full source file path;
- full target file path;
- file size;
- transfer time in milliseconds;
- encryption time with `EncryptionTimeMs`.

JSON example:

```json
[
  {
    "Timestamp": "2026-05-01T13:22:00+02:00",
    "BackupName": "Test_Backup",
    "SourceFile": "C:\\Users\\HP\\Desktop\\source_test\\test.txt",
    "TargetFile": "C:\\Users\\HP\\Desktop\\target_test\\test.txt",
    "FileSize": 14,
    "TransferTimeMs": 1,
    "EncryptionTimeMs": 0
  }
]
```

XML example:

```xml
<?xml version="1.0" encoding="utf-16"?>
<ArrayOfLogEntry>
  <LogEntry>
    <Timestamp>2026-05-01T13:22:00+02:00</Timestamp>
    <BackupName>Test_Backup</BackupName>
    <SourceFile>C:\Users\HP\Desktop\source_test\test.txt</SourceFile>
    <TargetFile>C:\Users\HP\Desktop\target_test\test.txt</TargetFile>
    <FileSize>14</FileSize>
    <TransferTimeMs>1</TransferTimeMs>
    <EncryptionTimeMs>0</EncryptionTimeMs>
  </LogEntry>
</ArrayOfLogEntry>
```

For EasySave v1.1, `EncryptionTimeMs` is `0` because encryption is planned for a future version.

---

## Interpretation codes

### TransferTimeMs

```text
>= 0 : successful transfer
< 0  : transfer error
```

### EncryptionTimeMs

```text
0   : no encryption
> 0 : encryption duration in milliseconds
< 0 : encryption error
```

---

## Command line

EasySave can be executed with an argument to automatically start one or more backup jobs.

Examples:

```bash
EasySave.exe 1
EasySave.exe 1-3
EasySave.exe "1;3"
```

Interpretation:

```text
1     : executes job no. 1 only
1-3   : executes jobs 1, 2 and 3
1;3   : executes jobs 1 and 3
```

---

## Language files location

Language files are stored in:

```text
languages/
```

Available files:

```text
fr.json
en.json
```

They are automatically copied to the output directory through the `EasySave.csproj` file.

---

## Common issues

### The source directory cannot be found

Probable cause:

- the source path does not exist;
- the user does not have read permissions;
- the network drive is not connected.

Recommended action:

- check the source path;
- check access rights;
- manually test opening the folder.

---

### The target directory is not accessible

Probable cause:

- the target path is incorrect;
- the user does not have write permissions;
- the target drive is unavailable.

Recommended action:

- check the target path;
- check write permissions;
- create the target folder if necessary.

---

### No log file is generated

Probable cause:

- no backup job has been executed yet;
- the application does not have write permissions;
- the `logs/` folder could not be created.

Recommended action:

- execute a backup job;
- check write permissions in the execution folder;
- check that the `logs/` folder exists.

---

### Labels are displayed between square brackets

Example:

```text
[AppTitle]
[MenuCreate]
[ChoicePrompt]
```

Probable cause:

- language files are not loaded;
- the `languages/` folder is missing from the execution directory;
- expected keys are missing from `fr.json` or `en.json`.

Recommended action:

Check that the following files exist in the execution directory:

```text
languages/fr.json
languages/en.json
```

---

### The XML format is not applied

Probable cause:

- `settings.json` was not saved correctly;
- the application is still using an old logger instance.

Recommended action:

- check the content of `settings.json`;
- check that `LogFormat` is set to `1` for XML;
- restart a backup job after changing the setting.

Expected example:

```json
{
  "LogFormat": 1,
  "Language": "fr"
}
```

---

## Supported architecture

EasySave v1.1 is organized using a console MVC architecture.

```text
Program.cs
    ↓
ConsoleApplicationController
    ↓
Views + Controllers
    ↓
Repositories + Strategies + Services
```

Layer responsibilities:

- `Program.cs`: lightweight application entry point;
- `ConsoleApplicationController`: orchestration of the console flow;
- `Views`: console display and user input;
- `Controllers`: coordination of user actions;
- `Repositories`: JSON data persistence;
- `Services`: technical services;
- `Strategies`: complete and differential backup algorithms;
- `EasyLog`: separate library for logging.

---

## .NET 10 technical choice

The specification mentions .NET 8.0 as the expected baseline.

The project is built with .NET 10.0 in order to use a more recent environment while preserving the expected principles:

- console MVC architecture for EasySave 1.x;
- separate EasyLog library;
- JSON/XML logs;
- real-time state file;
- architecture prepared for the future graphical MVVM version.

---

## Support contact

In case of an incident, provide the technical support team with:

- the EasySave version used;
- the content of `settings.json`;
- the content of `jobs.config.json`;
- the latest log file in `logs/`;
- the `state.json` file;
- a description of the scenario that caused the error.