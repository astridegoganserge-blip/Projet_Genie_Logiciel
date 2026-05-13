# EasySave v3.0 — User manual

## Launching the application

Double-click **EasySave.GUI.exe**. The interface language (French / English) can be changed at any time from the **Settings** menu.

## Create a backup job

1. Click **Create job**.
2. Fill in: job name, source folder, target folder, type (**Complete** or **Differential**).
3. Click **Save**.

## Run jobs

- **A single job**: ⚡ button on the job row.
- **All jobs in parallel**: **Execute all** button.
- **Real-time tracking**: progress bar and status are displayed in the grid.

## Control a running job

Available buttons per job and globally:

- ⏸ **Pause** — pause applies after the current file finishes;
- ▶ **Resume** — restarts a paused job;
- ■ **Stop** — immediately interrupts the job.

Jobs are automatically paused when the configured business software starts, and automatically resumed when it stops.

## Main settings

| Setting | Effect |
|---|---|
| Language | French / English (applied immediately). |
| Log format | JSON or XML. |
| Extensions to encrypt | Matching files are encrypted via CryptoSoft. |
| Priority extensions | Files transferred first (global barrier across jobs). |
| Max file size (KB) | Beyond this size, only one file is transferred at a time across all jobs. |
| Business software | Process name that pauses all backups. |
| Log mode | Local / Docker / Both. |
| Docker server URL | URL of the centralized log service. |

## Generated files (next to the executable)

- `jobs.config.json` — backup jobs list;
- `settings.json` — user settings;
- `state.json` — real-time job state;
- `logs/yyyy-MM-dd.json` or `.xml` — daily transfer history.

## Technical support

See **Documentation/support/EasySave_Support_Info_v3.0_EN.md** or contact ProSoft maintenance (5/7 support, 8 AM–5 PM).
