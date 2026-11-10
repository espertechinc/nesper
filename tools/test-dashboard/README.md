# Test run dashboard

Live view of `run-test-batches.ps1` output. Node only, no dependencies, nothing leaves the machine.

    node tools/test-dashboard/server.js            # http://localhost:4747
    node tools/test-dashboard/server.js --port 5000 --results TestResults

It reads `TestResults/<run>/` and pushes changes to the page over SSE. Older runs in the flat
`TestResults/TestResult_<ts>_<Batch>.trx` layout appear as well.

## What the runner writes

`run-test-batches.ps1` writes these into each run folder:

| File | When | Contents |
|---|---|---|
| `run.json` | start, then updated per batch | runner `pid` and `processStart`, `status` (`running`, `completed`, `aborted`), planned and completed batches, current batch and its `childPid` (the `dotnet test` launcher), `NESPER_*` env vars, git branch/commit/dirty, exit code |
| `<Batch>.live.log` | while a batch runs | `dotnet test` console output, flushed per line. Removed once the batch's `TestResult_*.txt` is written |
| `<Batch>.trx`, `TestResult_*.txt` | when a batch ends | as before |
| `summary.txt` | when the run ends normally | as before |

On Ctrl+C or an error the runner marks the run `aborted` and kills its own `dotnet test` process tree.
A runner that is killed outright cannot update `run.json`; the dashboard detects that by checking the recorded pid
and start time, and shows the run as interrupted.

## What it shows

Batch progress with live pass/fail counts for the running batch, live console output, failures split into new and
baseline (with message, stack and captured output), comparison against another run over the batches both have
finished, slowest tests, pass rate history, and a tag per run: single-assembly or default, git branch and commit.

Failures seen in a running batch appear under "In progress". Their class is not in the console output, so they are
classified as new or baseline when the batch's TRX arrives.

## Run state

- running: runner process alive (pid and start time match `run.json`)
- complete: the run wrote `summary.txt` or `run.json` says `completed`
- interrupted: no live runner and the run never finished; unreached batches read "not run"
- no recent output: runner alive but silent for over two hours

Runs without a `run.json` fall back to looking for a live `run-test-batches.ps1` process.

## Baseline

A failure is "baseline" when it matches `knownFailures` in `test-batches.json` or a pattern in `baseline.json`
(regex against `Class.Method`). Everything else is shown as new. Edit `baseline.json` as failures are fixed.

## Limits

- Runs started other than through `run-test-batches.ps1` (plain `dotnet test`, an IDE) are not tracked.
- `-IsolatedOnly` runs write `run.json` but no live log.
- Liveness checks use PowerShell and work on Windows only.
