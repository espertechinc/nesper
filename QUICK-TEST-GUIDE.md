# NEsper Test Quick Reference

## Quick Commands

### View all batches
```powershell
.\run-test-batches.ps1 -Summary
```

### Run all tests
```powershell
.\run-test-batches.ps1
```

### Run specific batch
```powershell
.\run-test-batches.ps1 -BatchName "Client"
.\run-test-batches.ps1 -BatchName "EPL-Database"
.\run-test-batches.ps1 -BatchName "Event"
```

### Run with verbose output
```powershell
.\run-test-batches.ps1 -BatchName "Client" -Verbose
```

### Run isolated tests
```powershell
.\run-test-batches.ps1 -IsolatedOnly
```

## Available Batches (in order of speed)

| Batch | Tests | Est. Time | Notes |
|-------|-------|-----------|-------|
| Client | 253 | ~2.5 min | Quick validation tests |
| EPL-Database | 55 | ~20 sec | Database integration |
| Multithread | 1 | ~3 sec | Timing-sensitive |
| EPL-FromClause | 56 | ~20 sec | Method tests |
| EPL-Dataflow | 64 | ~40 sec | Has 1 intermittent failure |
| Context | ~200 | ~3 min | Context tests |
| RowRecognition | 27 | ~30 sec | Has 1 intermittent failure |
| EPL-Join | 86 | ~1 min | Complex queries |
| Pattern | 143 | ~2.5 min | Has 2 intermittent failures |
| EPL-Subselect | 164 | ~1 min | Subquery tests |
| Event | 244 | ~4 min | **Has 4 known XML failures** |
| ResultSet | ~200 | ~5 min | Result set tests |
| View | ~200 | ~5 min | View tests |
| EPL-Other | 203 | ~2 min | Miscellaneous EPL |
| Expression | 909 | ~10 min | Large expression batch |
| Infrastructure | 412 | ~9 min | Tables, named windows |

## Common Workflows

### After making changes
1. **Quick sanity check**: `.\run-test-batches.ps1 -BatchName "Client"`
2. **Test related area**: `.\run-test-batches.ps1 -BatchName "<YourArea>"`
3. **Full regression**: `.\run-test-batches.ps1`

### Investigating failures
1. **Run single batch**: `.\run-test-batches.ps1 -BatchName "Event" -Verbose`
2. **Check if intermittent**: Run the same batch 3-5 times
3. **Isolate test**: Add to `isolatedTests` in config, run with `-IsolatedOnly`

### Before committing
```powershell
# Run all tests
.\run-test-batches.ps1
```

## Modifying Test Batches

### Enable/disable a batch
Edit `test-batches.json`:
```json
{
  "name": "Event",
  "enabled": false,  // Change to true/false
  ...
}
```

### Adjust timeout
```json
{
  "name": "EPL-Join",
  "timeout": 900000,  // 15 minutes in milliseconds
  ...
}
```

### Add new batch
```json
{
  "name": "MyNewTests",
  "filter": "TestMyFeature",
  "enabled": true,
  "timeout": 300000,
  "description": "Tests for my new feature"
}
```

### Split large batch
```json
{
  "name": "Event-Part1",
  "filter": "TestEventAvro",
  "enabled": true,
  "timeout": 300000,
  "description": "Event tests - Avro only"
},
{
  "name": "Event-Part2",
  "filter": "TestEventBean",
  "enabled": true,
  "timeout": 300000,
  "description": "Event tests - Bean only"
}
```

## Known Issues

### 4 Consistent Failures (99.89% pass rate)
All related to XML XPath property access:
- `TestEventXMLSchemaEventObservationDOM.WithCreateSchema`
- `TestEventXMLSchemaEventObservationDOM.WithPreconfig`
- `TestEventXMLSchemaEventObservationXPath.WithCreateSchema`
- `TestEventXMLSchemaEventObservationXPath.WithPreconfig`

Error: `failed assertion for property 'mytags[1].ID'`

### 4 Intermittent Failures (pass when run in isolation)
- `WithParameterInjectionCallback` - Static state pollution
- `WithSingleMaxSimple` - Deployment name mismatch
- `WithSinglePermFalseAndQuit` - Deployment name mismatch
- `TestRowRecogMaxStatesEngineWideNoPreventStart` - Deployment name mismatch

## Direct dotnet test Commands

If you need to run tests without the batch system:

### Run all regression tests
```powershell
cd tst\NEsper.Regression.Runner
dotnet test --framework net9.0 --settings ..\..\NEsper.runsettings
```

### Run specific test filter
```powershell
dotnet test --filter "TestEPLDatabase" --framework net9.0
```

### Run single test
```powershell
dotnet test --filter "FullyQualifiedName~WithSingleMaxSimple" --framework net9.0
```

### List all tests
```powershell
dotnet test --list-tests --framework net9.0
```

## Files

- `test-batches.json` - Batch configuration
- `run-test-batches.ps1` - Execution script
- `NEsper.runsettings` - Test runner settings
- `TEST-BATCHES-README.md` - Detailed documentation
- `QUICK-TEST-GUIDE.md` - This file

## Test Statistics

- **Total Regression Tests**: 3,801
- **Unit Tests**: 1,890
- **Combined**: 5,691 tests
- **Pass Rate**: 99.87% (excluding intermittent failures)
- **Frameworks**: net8.0, net9.0

## Troubleshooting

### "Batch not found" error
```powershell
# List available batches
.\run-test-batches.ps1 -Summary
```

### Test timeout
Increase timeout in `test-batches.json` for the specific batch

### Script execution policy error
```powershell
powershell.exe -ExecutionPolicy Bypass -File run-test-batches.ps1 -Summary
```

### All tests fail
1. Check database is running: `docker ps`
2. Restore packages: `dotnet restore NEsperAll.sln`
3. Build solution: `dotnet build NEsperAll.sln`
