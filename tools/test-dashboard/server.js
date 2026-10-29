#!/usr/bin/env node
// Live dashboard for run-test-batches.ps1 output. No dependencies.
//
//   node tools/test-dashboard/server.js [--port 4747] [--results TestResults] [--config test-batches.json]
//
// Reads TestResults/<run>/<Batch>.trx as the runner writes them and pushes changes to the page over SSE.
// A run directory is "running" until the runner writes summary.txt.

'use strict';

const fs = require('fs');
const http = require('http');
const { execFile } = require('child_process');
const path = require('path');

const args = Object.fromEntries(
  process.argv.slice(2).reduce((acc, a, i, all) => {
    if (a.startsWith('--')) acc.push([a.slice(2), all[i + 1]]);
    return acc;
  }, [])
);

const REPO = path.resolve(__dirname, '..', '..');
const PORT = Number(args.port || 4747);
const RESULTS = path.resolve(REPO, args.results || 'TestResults');
const CONFIG = path.resolve(REPO, args.config || 'test-batches.json');
const BASELINE = path.join(__dirname, 'baseline.json');
const STALL_MS = 20 * 60 * 1000; // a run with no new batch output for this long is "stalled"

const RUN_DIR = /^\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}$/;
const LEGACY_TRX = /^TestResult_(\d{8}_\d{6})_(.+)\.trx$/;

// ---------------------------------------------------------------- config

function readJson(file, fallback) {
  try {
    return JSON.parse(fs.readFileSync(file, 'utf8').replace(/^﻿/, ''));
  } catch (e) {
    if (e.code !== 'ENOENT') console.error(`cannot read ${file}: ${e.message}`);
    return fallback;
  }
}

// run.json is written by run-test-batches.ps1 (pid, processStart, env, git, current batch, live log name).
const metaCache = new Map();
function readRunMeta(run) {
  if (run.dir === RESULTS) return null; // legacy flat layout has no per-run folder
  const file = path.join(run.dir, 'run.json');
  try {
    const st = fs.statSync(file);
    const hit = metaCache.get(file);
    if (hit && hit.sig === st.mtimeMs) return hit.meta;
    const meta = JSON.parse(fs.readFileSync(file, 'utf8').replace(/^\uFEFF/, ''));
    metaCache.set(file, { sig: st.mtimeMs, meta });
    return meta;
  } catch {
    return null; // missing, or caught mid-write
  }
}

function publicMeta(meta) {
  if (!meta) return null;
  const env = meta.env || {};
  return {
    pid: meta.pid,
    childPid: meta.childPid,
    status: meta.status,
    mode: meta.mode,
    framework: meta.framework,
    currentBatch: meta.currentBatch,
    exitCode: meta.exitCode,
    stoppedEarly: meta.stoppedEarly,
    env,
    singleAssembly: env.NESPER_SINGLE_ASSEMBLY === '1',
    git: meta.git || null,
  };
}

// dotnet test console output at "normal" verbosity: "  Passed Name [12 ms]", "  Failed Name [..]" then detail lines.
function parseLive(text) {
  const lines = text.split(/\r?\n/);
  let passed = 0, failed = 0, skipped = 0;
  const failures = [];
  let cur = null;
  for (const ln of lines) {
    const m = /^ {2}(Passed|Failed|Skipped) (.+?)(?: \[[^\]]*\])?\s*$/.exec(ln);
    if (m) {
      cur = null;
      if (m[1] === 'Passed') passed++;
      else if (m[1] === 'Skipped') skipped++;
      else { failed++; cur = { name: m[2], lines: [] }; failures.push(cur); }
      continue;
    }
    if (cur && cur.lines.length < 80) cur.lines.push(ln);
  }
  return {
    passed, failed, skipped,
    failures: failures.map(f => ({ name: f.name, detail: f.lines.join('\n').trim().slice(0, 3000) })),
    tail: lines.slice(-60).join('\n'),
  };
}

function liveInfo(run, meta, state) {
  if (!meta || !meta.currentLog || state !== 'running') return null;
  try {
    return { batch: meta.currentBatch, ...parseLive(fs.readFileSync(path.join(run.dir, meta.currentLog), 'utf8')) };
  } catch {
    return null;
  }
}

function batchConfig() {
  const cfg = readJson(CONFIG, { batches: [], knownFailures: { failures: [] } });
  return (cfg.batches || []).filter(b => b.enabled !== false).map(b => b.name);
}

function baselineMatchers() {
  const cfg = readJson(CONFIG, { knownFailures: { failures: [] } });
  const matchers = [];
  for (const f of (cfg.knownFailures && cfg.knownFailures.failures) || []) {
    matchers.push({ re: new RegExp('^' + f.test.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + '$'), reason: f.reason });
  }
  for (const f of readJson(BASELINE, { patterns: [] }).patterns || []) {
    matchers.push({ re: new RegExp(f.pattern), reason: f.reason });
  }
  return matchers;
}

// ---------------------------------------------------------------- TRX parsing

const ENTITIES = { '&lt;': '<', '&gt;': '>', '&amp;': '&', '&quot;': '"', '&apos;': "'" };
const unescapeXml = s =>
  s
    .replace(/&#x([0-9a-fA-F]+);/g, (_, h) => String.fromCodePoint(parseInt(h, 16)))
    .replace(/&#(\d+);/g, (_, d) => String.fromCodePoint(Number(d)))
    .replace(/&(lt|gt|amp|quot|apos);/g, m => ENTITIES[m])
    .replace(/\r\n?/g, '\n');

function attrs(tag) {
  const out = {};
  const re = /([A-Za-z:]+)="([^"]*)"/g;
  let m;
  while ((m = re.exec(tag))) out[m[1]] = unescapeXml(m[2]);
  return out;
}

function seconds(duration) {
  // 00:00:00.1779710
  const m = /^(\d+):(\d+):(\d+(?:\.\d+)?)$/.exec(duration || '');
  return m ? Number(m[1]) * 3600 + Number(m[2]) * 60 + Number(m[3]) : 0;
}

function shortClass(className) {
  // com.x.TestSuiteClientDeploy+TestClientDeployListDependencies -> TestClientDeployListDependencies
  const last = (className || '').split('.').pop();
  return last.split('+').pop();
}

const trxCache = new Map(); // file -> {sig, parsed}

function parseTrx(file) {
  const st = fs.statSync(file);
  const sig = st.mtimeMs + ':' + st.size;
  const hit = trxCache.get(file);
  if (hit && hit.sig === sig) return hit.parsed;

  const xml = fs.readFileSync(file, 'utf8');

  // testId -> className
  const classOf = new Map();
  const unitRe = /<UnitTest [^>]*\bid="([^"]+)"[\s\S]*?<TestMethod [^>]*className="([^"]*)"/g;
  let m;
  while ((m = unitRe.exec(xml))) classOf.set(m[1], unescapeXml(m[2]));

  const times = /<Times [^>]*>/.exec(xml);
  const counters = /<Counters [^>]*>/.exec(xml);
  const t = times ? attrs(times[0]) : {};
  const c = counters ? attrs(counters[0]) : {};

  const tests = [];
  let pos = 0;
  while ((pos = xml.indexOf('<UnitTestResult ', pos)) !== -1) {
    const tagEnd = xml.indexOf('>', pos);
    const tag = xml.slice(pos, tagEnd + 1);
    const a = attrs(tag);
    let body = '';
    if (!tag.endsWith('/>')) {
      const close = xml.indexOf('</UnitTestResult>', tagEnd);
      body = xml.slice(tagEnd + 1, close);
      pos = close;
    } else {
      pos = tagEnd;
    }
    const test = {
      cls: shortClass(classOf.get(a.testId)),
      name: a.testName,
      outcome: a.outcome,
      sec: seconds(a.duration),
      start: a.startTime,
    };
    test.full = test.cls + '.' + test.name;
    if (a.outcome !== 'Passed' && body) {
      const msg = /<Message>([\s\S]*?)<\/Message>/.exec(body);
      const stack = /<StackTrace>([\s\S]*?)<\/StackTrace>/.exec(body);
      const out = /<StdOut>([\s\S]*?)<\/StdOut>/.exec(body);
      if (msg) test.message = unescapeXml(msg[1]).slice(0, 3000);
      if (stack) test.stack = unescapeXml(stack[1]).slice(0, 4000);
      if (out) test.stdout = unescapeXml(out[1]).slice(0, 6000);
    }
    tests.push(test);
  }

  const n = k => Number(c[k] || 0);
  const parsed = {
    start: t.start,
    finish: t.finish,
    total: n('total'),
    passed: n('passed'),
    failed: n('failed') + n('error') + n('timeout') + n('aborted'),
    skipped: Math.max(0, n('total') - n('executed')) + n('notExecuted'),
    tests,
  };
  if (!counters) {
    parsed.total = tests.length;
    parsed.passed = tests.filter(x => x.outcome === 'Passed').length;
    parsed.failed = tests.filter(x => x.outcome === 'Failed').length;
    parsed.skipped = tests.length - parsed.passed - parsed.failed;
  }
  trxCache.set(file, { sig, parsed });
  return parsed;
}

// ---------------------------------------------------------------- runs

function tsFromLegacy(ts) {
  // 20260305_185240 -> 2026-03-05T18:52:40
  return `${ts.slice(0, 4)}-${ts.slice(4, 6)}-${ts.slice(6, 8)}T${ts.slice(9, 11)}:${ts.slice(11, 13)}:${ts.slice(13, 15)}`;
}

function tsFromDir(id) {
  return id.replace('_', 'T').replace(/-(\d{2})-(\d{2})$/, ':$1:$2');
}

// Returns [{id, startedAt, dir, files:[{batch,file}], hasSummary}]
function discoverRuns() {
  let entries = [];
  try {
    entries = fs.readdirSync(RESULTS, { withFileTypes: true });
  } catch {
    return [];
  }
  const runs = [];
  for (const e of entries) {
    if (e.isDirectory() && RUN_DIR.test(e.name)) {
      const dir = path.join(RESULTS, e.name);
      const files = fs
        .readdirSync(dir)
        .filter(f => f.endsWith('.trx'))
        .map(f => ({ batch: f.replace(/^TestResult_\d{8}_\d{6}_/, '').replace(/\.trx$/, ''), file: path.join(dir, f) }));
      runs.push({
        id: e.name,
        startedAt: tsFromDir(e.name),
        dir,
        files,
        extra: fs.readdirSync(dir).filter(f => f === 'run.json' || f.endsWith('.live.log')).map(f => path.join(dir, f)),
        hasSummary: fs.existsSync(path.join(dir, 'summary.txt')),
      });
    }
  }
  const legacy = new Map();
  for (const e of entries) {
    const m = e.isFile() && LEGACY_TRX.exec(e.name);
    if (!m) continue;
    if (!legacy.has(m[1])) legacy.set(m[1], []);
    legacy.get(m[1]).push({ batch: m[2], file: path.join(RESULTS, e.name) });
  }
  for (const [ts, files] of legacy) {
    runs.push({ id: 'legacy-' + ts, startedAt: tsFromLegacy(ts), dir: RESULTS, files, hasSummary: true });
  }
  return runs.sort((a, b) => (a.startedAt < b.startedAt ? 1 : -1)); // newest first
}

function lastActivity(run) {
  let latest = 0;
  for (const f of [...run.files.map(x => x.file), ...(run.extra || [])]) {
    try {
      latest = Math.max(latest, fs.statSync(f).mtimeMs);
    } catch {}
  }
  if (!latest) {
    try {
      latest = fs.statSync(run.dir).mtimeMs;
    } catch {}
  }
  return latest;
}

// Is a run-test-batches.ps1 process alive? Polled in the background; null until the first answer or when the
// check is unavailable, in which case we fall back to judging by file activity.
let runnerAlive = null;
function pollRunner() {
  // A runner that writes run.json is tracked by pid (pollPids). The pattern scan only matters for a newest run
  // that predates run.json and has not finished, so skip spawning PowerShell otherwise.
  const newest = discoverRuns()[0];
  if (!newest || newest.hasSummary || readRunMeta(newest)) {
    runnerAlive = false;
    return;
  }
  const script =
    "(Get-CimInstance Win32_Process | Where-Object { $_.ProcessId -ne $PID -and $_.Name -match '^(powershell|pwsh)' -and $_.CommandLine -like '*run-test-batches.ps1*' }).Count";
  execFile('powershell', ['-NoProfile', '-NonInteractive', '-Command', script], { timeout: 15000, windowsHide: true }, (err, out) => {
    const n = parseInt(String(out || '').trim(), 10);
    runnerAlive = err || Number.isNaN(n) ? null : n > 0;
  });
}
// Runner liveness from run.json: a pid only counts when the live process started when run.json says it did,
// so a recycled pid is not mistaken for the runner.
const pidStart = new Map(); // pid -> ISO start time of the live process
let pidKnown = false;
function pollPids() {
  const pids = [];
  for (const r of discoverRuns().slice(0, 3)) {
    const m = readRunMeta(r);
    if (m && m.status === 'running' && m.pid) pids.push(Number(m.pid));
  }
  if (!pids.length) { pidStart.clear(); pidKnown = true; return; }
  const script = `Get-Process -Id ${pids.join(',')} -ErrorAction SilentlyContinue | ForEach-Object { "$($_.Id)|$($_.StartTime.ToString('o'))" }`;
  execFile('powershell', ['-NoProfile', '-NonInteractive', '-Command', script], { timeout: 15000, windowsHide: true }, (err, out) => {
    if (err && !out) return; // keep the previous answer
    pidStart.clear();
    for (const ln of String(out || '').split(/\r?\n/)) {
      const [id, start] = ln.trim().split('|');
      if (id && start) pidStart.set(Number(id), start);
    }
    pidKnown = true;
  });
}
function metaAlive(meta) {
  const start = pidStart.get(Number(meta.pid));
  return !!start && Math.abs(Date.parse(start) - Date.parse(meta.processStart)) < 3000;
}

if (process.platform === 'win32') {
  pollRunner();
  pollPids();
  setInterval(pollRunner, 10000).unref();
  setInterval(pollPids, 5000).unref();
}

// complete: runner wrote summary.txt. running: a runner is alive and this is the newest run.
// interrupted: no runner is alive but the run never finished (killed, crashed, closed). stalled: runner alive, no output for a long time.
function runState(run, isNewest, meta) {
  if (run.hasSummary) return 'complete';
  if (meta) {
    if (meta.status === 'completed') return 'complete';
    if (meta.status === 'aborted') return 'interrupted';
    if (meta.status === 'running' && pidKnown) {
      if (!metaAlive(meta)) return 'interrupted';
      return Date.now() - lastActivity(run) < 6 * STALL_MS ? 'running' : 'stalled';
    }
  }
  // no run.json (older runs): infer from a live run-test-batches.ps1 process, else from file activity
  if (runnerAlive === null) return Date.now() - lastActivity(run) < STALL_MS ? 'running' : 'stalled';
  if (!runnerAlive || !isNewest) return 'interrupted';
  return Date.now() - lastActivity(run) < 6 * STALL_MS ? 'running' : 'stalled';
}

function describeRun(run, matchers, withTests, isNewest) {
  const batches = [];
  let finish = 0;
  const totals = { total: 0, passed: 0, failed: 0, skipped: 0, newFailed: 0, baselineFailed: 0 };
  for (const f of run.files) {
    let p;
    try {
      p = parseTrx(f.file);
    } catch {
      continue; // being written
    }
    const fin = p.finish ? Date.parse(p.finish) : 0;
    finish = Math.max(finish, fin);
    const failing = p.tests.filter(t => t.outcome === 'Failed' || t.outcome === 'Error' || t.outcome === 'Timeout' || t.outcome === 'Aborted');
    for (const t of failing) {
      const known = matchers.find(mm => mm.re.test(t.full));
      t.baseline = known ? known.reason || true : false;
      t.batch = f.batch;
    }
    const baselineFailed = failing.filter(t => t.baseline).length;
    batches.push({
      name: f.batch,
      status: 'done',
      total: p.total,
      passed: p.passed,
      failed: p.failed,
      skipped: p.skipped,
      newFailed: failing.length - baselineFailed,
      baselineFailed,
      start: p.start,
      finish: p.finish,
      sec: p.start && p.finish ? (Date.parse(p.finish) - Date.parse(p.start)) / 1000 : 0,
      _failing: failing,
      _tests: p.tests,
    });
    for (const k of ['total', 'passed', 'failed', 'skipped']) totals[k] += p[k];
    totals.newFailed += failing.length - baselineFailed;
    totals.baselineFailed += baselineFailed;
  }

  const meta = readRunMeta(run);
  const state = runState(run, isNewest, meta);
  // a run that was started for specific batches expects only those, not everything in the config
  const order = meta && meta.batches && meta.batches.length ? [].concat(meta.batches) : batchConfig();
  const done = new Set(batches.map(b => b.name));
  batches.sort((a, b) => {
    const ia = order.indexOf(a.name), ib = order.indexOf(b.name);
    return (ia < 0 ? 1e9 : ia) - (ib < 0 ? 1e9 : ib);
  });
  const pendingStatus = state === 'interrupted' || state === 'stalled' ? 'notrun' : 'pending';
  const pending = order.filter(n => !done.has(n)).map(n => ({ name: n, status: pendingStatus }));
  if (state === 'running' && pending.length) {
    const current = meta && meta.currentBatch && pending.find(p => p.name === meta.currentBatch);
    (current || pending[0]).status = 'running';
  }

  const summary = {
    id: run.id,
    meta: publicMeta(meta),
    startedAt: run.startedAt,
    state,
    lastActivity: lastActivity(run),
    finishedAt: state === 'complete' && finish ? new Date(finish).toISOString() : null,
    totals,
    batchesDone: batches.length,
    batchesExpected: run.id.startsWith('legacy-') ? batches.length : Math.max(order.length, batches.length),
  };
  if (!withTests) return summary;

  const allTests = batches.flatMap(b => b._tests.map(t => ({ ...t, batch: b.name })));
  const failures = batches.flatMap(b => b._failing);
  const slowest = [...allTests].sort((a, b) => b.sec - a.sec).slice(0, 12).map(t => ({ full: t.full, batch: t.batch, sec: t.sec, outcome: t.outcome }));
  return {
    ...summary,
    live: liveInfo(run, meta, state),
    batches: [...batches.map(({ _failing, _tests, ...b }) => b), ...(run.id.startsWith('legacy-') ? [] : pending)],
    failures: failures.map(t => ({ full: t.full, batch: t.batch, sec: t.sec, baseline: t.baseline, message: t.message, stack: t.stack, stdout: t.stdout })),
    slowest,
  };
}

function compare(runId, baseId, matchers) {
  const runs = discoverRuns();
  const a = runs.find(r => r.id === runId);
  const b = runs.find(r => r.id === baseId);
  if (!a || !b) return null;
  const da = describeRun(a, matchers, true, a === runs[0]);
  const db = describeRun(b, matchers, true, b === runs[0]);
  // only batches both runs have finished are comparable; a running run has not reached the rest yet
  const doneIn = d => new Set(d.batches.filter(x => x.status === 'done').map(x => x.name));
  const comparable = new Set([...doneIn(da)].filter(n => doneIn(db).has(n)));
  const fa = new Map(da.failures.filter(f => comparable.has(f.batch)).map(f => [f.full, f]));
  const fb = new Map(db.failures.filter(f => comparable.has(f.batch)).map(f => [f.full, f]));
  return {
    run: runId,
    base: baseId,
    newlyFailing: [...fa.values()].filter(f => !fb.has(f.full)),
    fixed: [...fb.values()].filter(f => !fa.has(f.full)),
    stillFailing: [...fa.values()].filter(f => fb.has(f.full)).length,
    comparableBatches: [...comparable],
  };
}

// ---------------------------------------------------------------- http

function signature() {
  const runs = discoverRuns();
  return runnerAlive + '|' + [...pidStart.keys()].join(',') + '|' + runs.slice(0, 3).map(r => `${r.id}:${r.files.length}:${r.hasSummary}:${Math.round(lastActivity(r))}`).join('|');
}

const clients = new Set();
let lastSig = '';
setInterval(() => {
  let sig = '';
  try {
    sig = signature();
  } catch {}
  if (sig !== lastSig) {
    lastSig = sig;
    for (const res of clients) res.write('event: changed\ndata: {}\n\n');
  }
}, 2000);
setInterval(() => {
  for (const res of clients) res.write(': keepalive\n\n');
}, 25000);

function sendJson(res, obj, code = 200) {
  const body = JSON.stringify(obj);
  res.writeHead(code, { 'Content-Type': 'application/json', 'Cache-Control': 'no-store' });
  res.end(body);
}

const server = http.createServer((req, res) => {
  const url = new URL(req.url, 'http://localhost');
  try {
    if (url.pathname === '/' || url.pathname === '/index.html') {
      res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8', 'Cache-Control': 'no-store' });
      return res.end(fs.readFileSync(path.join(__dirname, 'index.html')));
    }
    if (url.pathname === '/events') {
      res.writeHead(200, { 'Content-Type': 'text/event-stream', 'Cache-Control': 'no-store', Connection: 'keep-alive' });
      res.write('retry: 2000\n\n');
      clients.add(res);
      req.on('close', () => clients.delete(res));
      return;
    }
    if (url.pathname === '/api/runs') {
      const matchers = baselineMatchers();
      return sendJson(res, { now: Date.now(), runs: discoverRuns().map((r, i) => describeRun(r, matchers, false, i === 0)) });
    }
    if (url.pathname === '/api/run') {
      const matchers = baselineMatchers();
      const all = discoverRuns();
      const run = all.find(r => r.id === url.searchParams.get('id'));
      return run ? sendJson(res, { now: Date.now(), ...describeRun(run, matchers, true, run === all[0]) }) : sendJson(res, { error: 'no such run' }, 404);
    }
    if (url.pathname === '/api/tests') {
      const run = discoverRuns().find(r => r.id === url.searchParams.get('id'));
      const file = run && run.files.find(f => f.batch === url.searchParams.get('batch'));
      if (!file) return sendJson(res, { error: 'no such batch' }, 404);
      const matchers = baselineMatchers();
      const tests = parseTrx(file.file).tests.map(t => {
        const failed = t.outcome === 'Failed' || t.outcome === 'Error' || t.outcome === 'Timeout' || t.outcome === 'Aborted';
        const known = failed && matchers.find(m => m.re.test(t.full));
        return { ...t, failed, baseline: known ? known.reason || true : false };
      });
      return sendJson(res, { batch: file.batch, tests });
    }
    if (url.pathname === '/api/compare') {
      const out = compare(url.searchParams.get('id'), url.searchParams.get('base'), baselineMatchers());
      return out ? sendJson(res, out) : sendJson(res, { error: 'no such run' }, 404);
    }
    res.writeHead(404).end();
  } catch (e) {
    sendJson(res, { error: String(e && e.stack || e) }, 500);
  }
});

server.listen(PORT, '127.0.0.1', () => {
  console.log(`NEsper test dashboard: http://localhost:${PORT}  (results: ${RESULTS})`);
});
