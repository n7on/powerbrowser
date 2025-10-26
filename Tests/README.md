# PowerBrowser Test Configuration

## Prerequisites

1. **Install Pester** (if not already installed):
   ```powershell
   Install-Module -Name Pester -Force -SkipPublisherCheck
   ```

2. **Ensure PowerBrowser is built**:
   ```bash
   dotnet build
   ```

## Running Tests

### Run All Tests
```powershell
# From the project root
Invoke-Pester -Path .\Tests\

# With detailed output
Invoke-Pester -Path .\Tests\ -Output Detailed

# Generate test results
Invoke-Pester -Path .\Tests\ -OutputFile TestResults.xml -OutputFormat NUnitXml
```

### Run Specific Test Categories
```powershell
# Run only browser management tests
Invoke-Pester -Path .\Tests\ -Tag "BrowserManagement"

# Run only pipeline tests
Invoke-Pester -Path .\Tests\ -Tag "Pipeline"

# Run only regex tests
Invoke-Pester -Path .\Tests\ -Tag "Regex"
```

### Run with Code Coverage
```powershell
Invoke-Pester -Path .\Tests\ -CodeCoverage .\Cmdlets\*.cs -CodeCoverageOutputFile coverage.xml
```

## Test Structure

### Test Categories
- **Browser Management** - Start/Stop browser operations
- **Page Management** - Page creation, naming, navigation
- **Pipeline Integration** - Object-oriented pipeline testing
- **Element Interaction** - Find, click, type, attribute operations
- **Error Handling** - Graceful failure scenarios
- **Regex Implementation** - Page name generation with regex

### Test Data
- Uses `https://httpbin.org` for reliable test endpoints
- Tests with Chrome browser (ensure Chrome is installed)
- Includes cleanup in `BeforeEach`/`AfterEach` blocks

## CI/CD Integration

### GitHub Actions Example
```yaml
name: PowerBrowser Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '6.0.x'
    
    - name: Build
      run: dotnet build
    
    - name: Install Chrome
      uses: browser-actions/setup-chrome@latest
    
    - name: Install Pester
      shell: powershell
      run: Install-Module -Name Pester -Force -SkipPublisherCheck
    
    - name: Run Tests
      shell: powershell
      run: |
        Invoke-Pester -Path .\Tests\ -OutputFile TestResults.xml -OutputFormat NUnitXml -CI
    
    - name: Publish Test Results
      uses: dorny/test-reporter@v1
      if: always()
      with:
        name: Pester Tests
        path: TestResults.xml
        reporter: dotnet-trx
```

## Test Best Practices

### 1. Isolation
- Each test should be independent
- Use `BeforeEach`/`AfterEach` for cleanup
- Don't rely on test execution order

### 2. Reliability
- Use proper wait times for async operations
- Handle browser startup/shutdown gracefully
- Use reliable test URLs (httpbin.org)

### 3. Maintainability
- Use descriptive test names
- Group related tests in contexts
- Keep tests focused and atomic

### 4. Performance
- Reuse browser instances where possible
- Clean up resources promptly
- Use parallel execution for independent tests

## Debugging Tests

### Run Single Test
```powershell
Invoke-Pester -Path .\Tests\PowerBrowser.Tests.ps1 -TestName "Should create pages with sequential names"
```

### Debug Mode
```powershell
# Enable verbose output
$VerbosePreference = "Continue"
Invoke-Pester -Path .\Tests\ -Output Detailed
```

### Manual Debugging
```powershell
# Start interactive session with test setup
Import-Module .\bin\Debug\netstandard2.0\PowerBrowser.dll -Force
$browser = Start-Browser -Name 'Chrome'
$page = New-BrowserPage -BrowserName 'Chrome' -Url 'https://httpbin.org/html'
# ... interact manually
Stop-Browser -Name 'Chrome'
```