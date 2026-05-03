# EasySave v1.1 — User Manual

## Overview

EasySave is a console application used to create and execute complete or differential backup jobs.

Version 1.1 keeps the behavior of version 1.0 and adds the ability to choose the daily log file format: JSON or XML.

## Launching the application

From the application folder, run:

```bash
EasySave.exe
```

or in development mode:

```bash
dotnet run
```

At startup, choose the language:

```text
1. French
2. English
```

## Main menu

```text
1. Create a backup job
2. Execute a backup job
3. Sequential execution
4. Show configured jobs
5. Delete a job
6. Settings
0. Exit application
```

## Creating a backup job

Choose option `1`, then enter:

- the job identifier;
- the backup name;
- the source path;
- the target path;
- the backup type.

Available backup types:

```text
1 = complete backup
2 = differential backup
```

A complete backup copies all files from the source directory.  
A differential backup copies only new or modified files.

## Executing a backup job

Choose option `2`, then enter the job identifier.

## Executing multiple jobs

Choose option `3`, then enter a command.

Examples:

```text
1-3   executes jobs 1, 2 and 3
1;3   executes jobs 1 and 3
```

## Changing the log format

Choose option `6`, then select:

```text
1 = JSON
2 = XML
```

The selected format is saved in `settings.json`.

## Generated files

Files are generated next to the executable:

```text
jobs.config.json   backup job configuration
settings.json      user settings
state.json         real-time state file
logs/              daily log files
```

Daily logs are created using the selected format:

```text
YYYY-MM-DD.json
YYYY-MM-DD.xml
```

## Exiting the application

Choose option `0`.

## Note

EasySave v1.1 does not include a graphical interface.  
The graphical interface is planned for EasySave v2.0.