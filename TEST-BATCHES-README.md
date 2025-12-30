# NEsper Test Batch Configuration System

This directory contains a configurable test batch system for running NEsper regression tests efficiently and managing test organization.

## Files

- **test-batches.json** - Configuration file defining test batches, timeouts, and known issues
- **run-test-batches.ps1** - PowerShell script to execute tests according to configuration
- **TEST-BATCHES-README.md** - This documentation file

## Quick Start

### View Configuration Summary
```powershell
.\run-test-batches.ps1 -Summary
```

### Run All Enabled Batches
```powershell
.\run-test-batches.ps1
```

### Run a Specific Batch
```powershell
.\run-test-batches.ps1 -BatchName "EPL-Database"
```

### Run with Verbose Output
```powershell
.\run-test-batches.ps1 -BatchName "Event" -Verbose
```

### Run Isolated Tests Only
```powershell
.\run-test-batches.ps1 -IsolatedOnly
```

### Run on Different Framework
```powershell
.\run-test-batches.ps1 -Framework "net8.0"
```

## Configuration File Structure

### Main Sections

#### 1. Configuration
Global settings for test execution:
```json
{
  "configuration": {
    "defaultTimeout": 600000,
    "defaultFramework": "net9.0",
    "runSettingsPath": "NEsper.runsettings",
    "testProjectPath": "tst/NEsper.Regression.Runner",
    "parallelBatches": false,
    "stopOnFirstFailure": false
  }
}
```

#### 2. Batches
Array of test batch definitions:
```json
{
  "batches": [
    {
      "name": "EPL-Database",
      "filter": "TestEPLDatabase",
      "enabled": true,
      "timeout": 300000,
      "description": "Database integration tests"
    }
  ]
}
```

**Batch Properties:**
- `name` - Unique identifier for the batch
- `filter` - NUnit test filter (e.g., "TestEPLDatabase" or "FullyQualifiedName~WithMethod")
- `enabled` - Boolean to enable/disable batch
- `timeout` - Timeout in milliseconds
- `description` - Human-readable description

#### 3. Isolated Tests
Tests that should run separately due to state pollution:
```json
{
  "isolatedTests": {
    "tests": [
      {
        "name": "WithParameterInjectionCallback-Isolated",
        "filter": "FullyQualifiedName~WithParameterInjectionCallback",
        "enabled": false,
        "timeout": 120000,
        "reason": "Fails when run with other dataflow tests"
      }
    ]
  }
}
```

#### 4. Known Failures
Documentation of known test failures:
```json
{
  "knownFailures": {
    "failures": [
      {
        "test": "TestEventXMLSchemaEventObservationDOM.WithCreateSchema",
        "reason": "XPath property 'mytags[1].ID' assertion failure",
        "issue": "XML-001"
      }
    ]
  }
}
```

## Modifying Test Batches

### Adding a New Batch

1. Edit `test-batches.json`
2. Add a new entry to the `batches` array:

```json
{
  "name": "MyNewBatch",
  "filter": "TestMyNewFeature",
  "enabled": true,
  "timeout": 300000,
  "description": "Tests for my new feature"
}
```

3. Run: `.\run-test-batches.ps1 -Summary` to verify
4. Execute: `.\run-test-batches.ps1 -BatchName "MyNewBatch"`

### Splitting a Large Batch

If a batch is too large or times out:

1. Identify sub-groups using test names (e.g., `TestEPLOther` has many sub-tests)
2. Create multiple smaller batches with more specific filters:

```json
{
  "name": "EPL-Other-Part1",
  "filter": "TestEPLOtherIStreamRStream",
  "enabled": true,
  "timeout": 300000,
  "description": "EPL Other - IStream/RStream tests"
},
{
  "name": "EPL-Other-Part2",
  "filter": "TestEPLOtherStaticFunc",
  "enabled": true,
  "timeout": 300000,
  "description": "EPL Other - Static function tests"
}
```

### Adjusting Timeouts

Based on test execution results, adjust timeouts:

```json
{
  "name": "EPL-Join",
  "timeout": 600000,  // Increased from 300000 due to slow execution
  "description": "Join tests - complex queries"
}
```

### Disabling Problematic Tests

Temporarily disable a batch while investigating:

```json
{
  "name": "Event",
  "enabled": false,  // Changed from true
  "description": "Disabled due to XML XPath failures under investigation"
}
```

## Common Use Cases

### Testing After Code Changes

1. Run fast batches first to catch obvious issues:
```powershell
.\run-test-batches.ps1 -BatchName "Client"
```

2. Run related batches:
```powershell
.\run-test-batches.ps1 -BatchName "EPL-Database"
```

3. Run full suite:
```powershell
.\run-test-batches.ps1
```

### Investigating Intermittent Failures

1. Move test to isolated tests section in config
2. Run multiple times:
```powershell
for ($i=1; $i -le 10; $i++) {
    Write-Host "Run $i"
    .\run-test-batches.ps1 -IsolatedOnly
}
```

### Creating Custom Test Groups

For focused testing, create temporary batch configurations:

```powershell
# Copy config
Copy-Item test-batches.json test-batches-custom.json

# Edit custom config to enable only needed batches

# Run with custom config
.\run-test-batches.ps1 -ConfigFile test-batches-custom.json
```

## NUnit Filter Syntax

The `filter` property uses NUnit's test filter syntax:

### Examples

```
"TestEPLDatabase"              # All tests in TestEPLDatabase class
"FullyQualifiedName~WithParam" # Tests with "WithParam" in fully qualified name
"TestCategory=Integration"      # Tests with Integration category
"TestName=TestFoo|TestName=TestBar"  # TestFoo OR TestBar
```

### Finding Test Names

To see available test names:
```powershell
cd tst/NEsper.Regression.Runner
dotnet test --list-tests --framework net9.0
```

## Troubleshooting

### Batch Times Out

1. Increase timeout in configuration:
```json
"timeout": 900000  // 15 minutes
```

2. Or split into smaller batches

### Test Isolation Issues

If tests pass in isolation but fail in batch:

1. Add to `isolatedTests` section
2. Enable isolated test
3. Disable from main batch or use more specific filter to exclude

### Configuration Not Found

Ensure you're running from the repository root:
```powershell
cd D:\src\Espertech\NEsper-8.9.x
.\run-test-batches.ps1
```

## Current Test Statistics

Based on latest run (2025-12-30):

- **Total Regression Tests**: 3,801
- **Overall Pass Rate**: 99.87%
- **Intermittent Failures**: 4 (pass when run in isolation)
- **Known Consistent Failures**: 4 (XML XPath issues)

## Integration with CI/CD

### GitHub Actions Example

```yaml
- name: Run NEsper Test Batches
  run: |
    pwsh ./run-test-batches.ps1
  working-directory: ${{ github.workspace }}
```

### Azure DevOps Example

```yaml
- task: PowerShell@2
  inputs:
    filePath: 'run-test-batches.ps1'
    workingDirectory: '$(Build.SourcesDirectory)'
  displayName: 'Run NEsper Test Batches'
```

## Best Practices

1. **Keep batches focused**: Group related tests together
2. **Reasonable timeouts**: Start with 5-10 minutes, adjust based on results
3. **Document known issues**: Use `knownFailures` section
4. **Regular updates**: Adjust configuration based on test results
5. **Version control**: Commit configuration changes with code changes
6. **Test locally first**: Run batches locally before CI/CD

## Support

For questions or issues with the batch configuration system, see:
- CLAUDE.md - General NEsper development guide
- NEsper.runsettings - Test runner configuration
- This file - Batch system documentation
