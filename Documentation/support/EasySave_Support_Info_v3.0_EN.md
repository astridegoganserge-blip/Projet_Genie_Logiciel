# EasySave v3.0 — Technical support information

## 1. Affected version

EasySave v3.0.0

## 2. Application type

EasySave v3.0 is a concurrent multi-task backup application developed in C# with .NET 10.0.

It contains:

- a WPF graphical interface;
- an MVVM architecture;
- the `EasySave.Core` business library (parallelism, priorities, throttling);
- the `EasyLog` logging library;
- a single-instance external `CryptoSoft` integration;
- a containerized `EasySave.LogServer` centralization service.

---

## 3. Main projects

```text
Projet_Genie_Logiciel/
├── EasySave/              (v1.1 console application)
├── EasySave.Core/         (shared business core)
├── EasySave.GUI/          (v3.0 WPF interface)
├── EasySave.LogServer/    (Docker centralization service)
├── EasyLog/               (logging DLL)
└── CryptoSoft/            (single-instance encryption tool)
```

---

## 4. Minimum requirements

| Item | Value |
|---|---|
| Operating system | Windows 10 64-bit or later |
| Runtime | .NET 10.0 Desktop |
| RAM | 200 MB free minimum |
| Disk space | 200 MB excluding user data |
| Permissions | Read on sources, write on targets, execute `CryptoSoft.exe` |
| Docker (optional) | Docker Desktop or Docker Engine for centralized logging |

---

## 5. Default locations

- Executable: `EasySave.GUI.exe`
- Working directory: executable folder
- CryptoSoft: `CryptoSoft/` sub-folder next to the executable (automatically copied at build time)
- Local logs: `logs/` sub-folder next to the executable
- Centralized logs (if enabled): `centralized-logs` Docker volume of the `easysave-logserver` container

---

## 6. Configuration and log files

All application files are in JSON format and placed in the execution folder.

| File | Role |
|---|---|
| `jobs.config.json` | Backup jobs (Guid, number, name, paths, type, last execution) |
| `settings.json` | Global parameters (language, log format, encryption, priorities, max size, business software, log mode, Docker URL) |
| `state.json` | Real-time job state |
| `logs/yyyy-MM-dd.json` or `.xml` | Daily transfer history |
| `crash.log` | Unhandled exception trace (created only when an incident occurs) |

---

## 7. Starting the Docker centralization service

From the repository root:

```bash
docker compose up -d
```

The service listens on port `5000`. URL to fill in EasySave settings: `http://<server-address>:5000`.

Verification:

```bash
curl http://localhost:5000/
```

Expected response: `{"service":"EasySave Log Server","status":"Running"}`.

---

## 8. Troubleshooting

| Symptom | Check |
|---|---|
| No job listed | Check existence and readability of `jobs.config.json`. |
| Backup refused at launch | Check that no configured business software is currently running. |
| Encryption fails (`EncryptionTimeMs < 0` in the log) | Check that `CryptoSoft.exe` exists in the `CryptoSoft/` sub-folder. |
| Encryption blocked for 30 seconds | Another CryptoSoft instance is running. Expected behavior (single-instance). |
| Missing Docker logs in `Both` mode | Check the server URL in settings and verify the `easysave-logserver` container is running (`docker ps`). |
| Business software watcher no longer responds | The process name was changed in settings. Restart the application. |
| `state.json` looks stuck | Check `crash.log`. Verify no `state.json.tmp` or `state.json.corrupted` is left over. |
| Application appears frozen on a large file | Expected behavior: only one file above the threshold at a time. Other jobs continue with smaller files. |

---

## 9. Maintenance contract

Annual contract with tacit renewal, 5/7 support from 8 AM to 5 PM, updates included.

Pricing: 12% of the unit purchase price (€200 excl. VAT), annually adjusted based on the SYNTEC index.

---

## 10. Contact

ProSoft technical support — EasySave team.
