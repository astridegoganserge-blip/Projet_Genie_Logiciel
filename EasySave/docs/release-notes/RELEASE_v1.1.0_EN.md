# Release Note — EasySave v1.1.0

## Version

EasySave v1.1.0

## Release type

Minor version based on EasySave v1.0.

This version keeps the console behavior of version 1.0 and mainly adds the ability to choose the daily log file format.

## Features kept from EasySave v1.0

- Console application.
- Management of up to 5 backup jobs.
- Creation, display, deletion and execution of backup jobs.
- Complete backup.
- Differential backup.
- Single job execution.
- Sequential execution through the menu or command line.
- French and English interface.
- Real-time state file `state.json`.
- Logging through the separate `EasyLog.dll` library.

## New features in EasySave v1.1

- Added daily log format selection: JSON or XML.
- Added a Settings menu.
- User settings are saved in `settings.json`.
- Added repositories to better separate persistence:
  - `IJobRepository`
  - `JsonJobRepository`
  - `ISettingsRepository`
  - `JsonSettingsRepository`
- Refactored the application toward a console MVC architecture:
  - `Controllers`
  - `Views`
  - `Models`
  - `Repositories`
  - `Services`
  - `Strategies`
- Simplified `Program.cs`.
- Added `ConsoleApplicationController` to centralize the console application flow.
- Standardized state statuses:
  - `Actif`
  - `Terminé`
  - `Erreur`
- Added the `Progression` field in `state.json`.
- Prepared EasyLog for future versions with the `EncryptionTimeMs` field.

## Compatibility

EasySave v1.1 remains compatible with the features of version 1.0.