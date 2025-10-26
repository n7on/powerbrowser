# PowerBrowser Project Structure

This document outlines the organized project structure following .NET best practices.

## 📁 Directory Structure

```
PowerBrowser/
├── 📦 Models/                          # Domain objects and data models
│   ├── PowerBrowserInstance.cs        # Browser wrapper with metadata
│   ├── PowerBrowserPage.cs            # Page wrapper with metadata  
│   └── PowerBrowserElement.cs         # Element wrapper with metadata
├── 🔧 Cmdlets/                        # PowerShell cmdlets
│   ├── BrowserCmdletBase.cs           # Base class for all cmdlets
│   ├── StartBrowserCommand.cs         # Start-Browser cmdlet
│   ├── StopBrowserCommand.cs          # Stop-Browser cmdlet
│   ├── GetBrowserCommand.cs           # Get-Browser cmdlet
│   ├── NewBrowserPageCommand.cs       # New-BrowserPage cmdlet
│   ├── GetBrowserPageCommand.cs       # Get-BrowserPage cmdlet
│   ├── RemoveBrowserPageCommand.cs    # Remove-BrowserPage cmdlet
│   ├── FindBrowserElementCommand.cs   # Find-BrowserElement cmdlet
│   ├── ClickBrowserElementCommand.cs  # Invoke-BrowserElementClick cmdlet
│   ├── SetBrowserElementTextCommand.cs # Set-BrowserElementText cmdlet
│   ├── GetBrowserElementAttributeCommand.cs # Get-BrowserElementAttribute cmdlet
│   ├── InstallBrowserCommand.cs       # Install-Browser cmdlet
│   └── UninstallBrowserCommand.cs     # Uninstall-Browser cmdlet
├── 🧪 Tests/                          # Test files
│   ├── PowerBrowser.Tests.ps1         # Pester test suite
│   └── README.md                      # Testing documentation
├── 📖 Examples/                       # Usage examples
│   ├── 01-Basic-WebScraping.ps1
│   ├── 02-Form-Interaction.ps1
│   ├── 03-Multi-Page-Pipeline.ps1
│   ├── 04-Advanced-Element-Interaction.ps1
│   ├── 05-Pipeline-Chaining-ErrorHandling.ps1
│   └── README.ps1                     # Examples documentation
├── 📋 Project Files/
│   ├── PowerBrowser.csproj            # Project configuration
│   ├── PowerBrowser.psd1              # PowerShell module manifest
│   ├── PowerBrowser.sln               # Solution file
│   ├── TODO.md                        # Feature roadmap
│   └── PIPELINE-EXAMPLES.md           # Pipeline usage guide
└── 🔨 Build Output/
    ├── bin/                           # Compiled binaries
    └── obj/                           # Build artifacts
```

## 🏗️ Architecture Principles

### **Separation of Concerns**
- **Models**: Pure data objects and domain logic
- **Cmdlets**: PowerShell integration and user interface
- **Tests**: Comprehensive test coverage
- **Examples**: User documentation and tutorials

### **One Class Per File**
- Each class has its own file with matching filename
- `PowerBrowserInstance` → `PowerBrowserInstance.cs`
- `PowerBrowserPage` → `PowerBrowserPage.cs` 
- `PowerBrowserElement` → `PowerBrowserElement.cs`
- Improves code navigation and maintainability

### **Namespace Organization**
- `PowerBrowser.Models` - Domain objects (PowerBrowserInstance, PowerBrowserPage, PowerBrowserElement)
- `PowerBrowser.Cmdlets` - PowerShell cmdlets and base classes

### **Dependencies**
```
Cmdlets → Models  ✅ (Cmdlets depend on Models)
Models → Cmdlets  ❌ (Models are independent)
Tests → Both      ✅ (Tests verify both layers)
```

## 📊 Code Metrics (as of refactoring)

| Directory | Files | Lines | Purpose |
|-----------|-------|-------|---------|
| **Models** | 3 | 216 | Domain objects |
| **Cmdlets** | 13 | 1,841 | PowerShell integration |
| **Tests** | 2 | 382 | Quality assurance |
| **Examples** | 6 | 834 | Documentation |
| **Total** | 22 | **3,273** | Source code only |

## 🎯 Benefits of This Structure

### **🔍 Maintainability**
- Clear separation between business logic and PowerShell integration
- Easy to locate and modify specific functionality
- Consistent file naming conventions

### **🧪 Testability**
- Models can be unit tested independently
- Cmdlets can be integration tested
- Clear dependencies make mocking easier

### **📈 Scalability**
- Easy to add new cmdlets without affecting models
- Models can be extended without changing cmdlets
- New features fit naturally into the structure

### **👥 Team Development**
- Different developers can work on models vs cmdlets
- Clear ownership boundaries
- Easier code reviews

## 🚀 Future Enhancements

As we add features from TODO.md, they should follow this structure:

### **New Cmdlets**
```
Cmdlets/
├── NavigateBrowserPageCommand.cs      # Navigate-BrowserPage
├── WaitBrowserElementCommand.cs       # Wait-BrowserElement  
├── GetBrowserPageScreenshotCommand.cs # Get-BrowserPageScreenshot
└── SendBrowserKeysCommand.cs          # Send-BrowserKeys
```

### **Extended Models**
```csharp
// Models/PowerBrowserObjects.cs
public class PowerBrowserCookie { ... }      // Cookie management
public class PowerBrowserStorage { ... }     // Local/session storage
public class PowerBrowserNetwork { ... }     # Network monitoring
```

### **Additional Namespaces** (if needed)
```
PowerBrowser.Models.Authentication   # Authentication objects
PowerBrowser.Models.Network         # Network monitoring objects  
PowerBrowser.Utilities              # Helper functions
PowerBrowser.Extensions             # Extension methods
```

## 📝 Migration Notes

### **What Changed**
1. **Moved**: `Cmdlets/PowerBrowserObjects.cs` → `Models/` (split into separate files)
2. **Split**: One file with 3 classes → 3 files with 1 class each
3. **Updated**: Namespace from `PowerBrowser.Cmdlets` → `PowerBrowser.Models`
4. **Added**: `using PowerBrowser.Models;` to all cmdlet files
5. **Maintained**: All functionality and APIs remain unchanged

### **Breaking Changes**
- **None** - This is purely internal refactoring
- All PowerShell cmdlets work exactly the same
- All tests pass without modification
- All examples continue to work

---

*This structure follows .NET best practices and enables sustainable growth of the PowerBrowser module.*