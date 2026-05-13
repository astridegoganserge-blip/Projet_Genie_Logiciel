# Release note — EasySave v2.0.0

## Target version

EasySave v2.0.0

## Release type

Major release.

This version introduces the WPF graphical interface based on the MVVM architecture, while keeping compatibility with the previous EasySave v1.1 console features.

---

## Version objective

EasySave v2.0 aims to make the application more ergonomic, more extensible and more aligned with the expected graphical version requirements.

Version 2.0 allows users to:

- manage an unlimited number of backup jobs;
- execute one or several backup jobs from a graphical interface;
- run backup jobs sequentially;
- monitor execution state in real time;
- select the interface language;
- configure file extensions to encrypt;
- integrate CryptoSoft for encryption;
- block execution when a business software is detected.

---

## Main new features

### WPF graphical interface

EasySave now includes a WPF graphical interface.

The interface allows users to:

- display the backup job list;
- create new backup jobs;
- execute a selected job;
- execute all jobs sequentially;
- monitor job execution;
- edit general settings.

---

### MVVM architecture

Version 2.0 introduces a clear separation between:

- WPF views;
- ViewModels;
- application services;
- business core;
- repositories;
- backup strategies.

This structure prepares the application for future improvements.

---

### EasySave.Core project

A new `EasySave.Core` project has been added.

It contains the business logic for version 2.0:

- models;
- repositories;
- services;
- strategies;
- backup manager.

The graphical project `EasySave.GUI` now depends on `EasySave.Core`.

---

### Unlimited backup jobs

The five-job limit is no longer applied in the graphical version.

Each backup job has:

- a technical `Guid` identifier;
- a visible auto-incremented number;
- a name;
- a source path;
- a target path;
- a backup type;
- a last execution date.

In the interface, jobs are displayed with readable numbering:

```text
01
02
03