# Release note — EasySave v3.0.0

## Affected version

EasySave v3.0.0

## Release type

Major release.

This release turns EasySave into a concurrent, multi-task backup tool, able to run several jobs in parallel, to react in real time to user input, and to centralize daily logs on a Docker service shared between several client servers.

---

## Goals

EasySave v3.0 implements the evolution requirements requested by ProSoft management based on customer feedback on version 2.0. The objective is to leverage multi-core hardware and multi-server deployments without sacrificing the robustness gained in v2.0.

Version 3.0 makes it possible to:

- run backup jobs in parallel;
- prioritize file extensions through a global barrier;
- limit simultaneous transfers of large files;
- pause / resume / stop each job individually or all jobs at once;
- automatically pause every backup job when the business software is detected, and resume automatically when it stops;
- enforce that CryptoSoft only runs as a single instance;
- centralize daily log files on a dedicated Docker service.

---

## Main new features

### Parallel execution

The sequential mode is replaced by parallel execution.

Each job now owns its own `JobExecutionContext`, which carries:

- a `CancellationToken` for immediate stop;
- a pause flag for soft interruption after the current file.

`BackupManager` exposes a new `ExecuteParallel(ids, logger)` method that launches jobs through `Task.WhenAll`.

---

### Priority files

A list of priority extensions is configurable in the general settings.

No non-priority file can be transferred while a priority file is still pending on any active job (global barrier managed by `PriorityFileFilter`).

Priority files are also moved to the head of each job's transfer queue.

---

### Throttling of simultaneous large-file transfers

A size threshold in kilobytes is configurable in the general settings.

When a file exceeds this threshold, its transfer acquires a global `SemaphoreSlim(1, 1)`: only one large file can be transferred at a time, across all backups.

Smaller files keep transferring in parallel with no constraint.

---

### Play / Pause / Stop controls

The GUI now exposes six commands:

- Pause / Resume / Stop per job;
- Pause All / Resume All / Stop All globally.

Pause is applied after the transfer of the current file. Stop interrupts the job immediately and marks the state as `Interrompu`.

---

### Automatic pause on business software

When a business software is configured, the `BusinessSoftwareWatcher.StartContinuousWatch` service continuously monitors its presence.

If the business software starts during a backup, all jobs are automatically paused through `PauseAll`. When the business software stops, jobs automatically resume through `ResumeAll`.

---

### Single-instance CryptoSoft

`CryptoSoft.exe` now refuses a second simultaneous instance.

Uniqueness is guaranteed by a named global mutex `Global\CryptoSoft_ProSoft_Mutex`.

On the EasySave side, the `CryptoService` waits for CryptoSoft availability through the same mutex before starting encryption, with a 30 second timeout.

---

### Docker log centralization

A new containerized `EasySave.LogServer` service is shipped in the repository.

It exposes two HTTP endpoints:

- `GET /`: service status;
- `POST /log`: receives a log entry.

The service writes every entry into a single daily file `yyyy-MM-dd.ndjson` under the `centralized-logs` directory. Concurrent writes are serialized through a `SemaphoreSlim` to prevent line interleaving.

Three operating modes are available on the client side:

- **Local**: logs on the user workstation only;
- **Docker**: logs on the centralized Docker server only;
- **Both**: logs on both at the same time.

Every sent entry carries a `MachineName` field to differentiate originating machines.

---

## Architecture

V3 adds the following classes on top of V2:

| Component | Role |
|---|---|
| `BackupStrategyBase` | Template Method abstract class shared by both strategies. |
| `JobExecutionContext` | Per-job execution context (pause, stop). |
| `PriorityFileFilter` | Global barrier for priority files. |
| `LargeFileThrottle` | Global semaphore for large files. |
| `LogCentralizer` | HTTP client to the Docker service. |
| `EasySave.LogServer` | Docker log centralization service. |

---

## Compatibility

- `jobs.config.json` and `state.json` files remain compatible with version 2.0.
- `settings.json` accepts new optional fields (`PriorityExtensions`, `MaxFileSizeKb`, `LogMode`, `DockerLogServerUrl`). Existing parameters are preserved.
- Previous defaults apply when no explicit configuration is provided (`LogMode = Local`, `MaxFileSizeKb = 0` disables throttling).

---

## Known limitations

- The business software watcher is started at application launch with the initial parameter value. Changing the process name in the settings requires a restart to take effect.
- The XML log format applies a full file rewrite per entry. An append-mode optimization is planned for version 4.0.

---

## Team

- Anelka MAPA
- Astride Gogan
- Luc Dai

---

## License

Academic project — CESI — ProSoft pedagogical fiction.
