# README for PowerBrowser Examples
# This file provides an overview of all example scripts and how to run them

Write-Host "📚 PowerBrowser Examples Overview" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Green

Write-Host @"
🎯 Welcome to PowerBrowser Examples!

This folder contains comprehensive examples demonstrating the object-oriented 
pipeline capabilities of the PowerBrowser module. Each example showcases 
different aspects of browser automation using PowerShell.

📂 Available Examples:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

01-Basic-WebScraping.ps1
  🌐 Fundamental web scraping operations
  • Starting browsers and creating pages
  • Finding elements with CSS selectors
  • Extracting text content and attributes
  • Basic pipeline operations

02-Form-Interaction.ps1
  📝 Complete form filling and submission
  • Input field interaction
  • Text entry and clearing
  • Form validation and submission
  • Response handling

03-Multi-Page-Pipeline.ps1
  🌍 Managing multiple browser pages
  • Creating and managing multiple pages
  • Parallel page processing
  • Content comparison across pages
  • Selective page operations

04-Advanced-Element-Interaction.ps1
  ⚡ Advanced element manipulation techniques
  • Complex element finding strategies
  • Detailed element analysis
  • Visibility and positioning checks
  • Element statistics and reporting

05-Pipeline-Chaining-ErrorHandling.ps1
  🔄 Sophisticated pipeline patterns
  • Advanced command chaining
  • Robust error handling strategies
  • Conditional pipeline logic
  • Resource cleanup patterns

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🚀 Quick Start:
1. Ensure PowerBrowser module is built (run 'dotnet build' in project root)
2. Navigate to the Examples folder
3. Run any example script: .\01-Basic-WebScraping.ps1

⚠️ Prerequisites:
• PowerShell 5.1 or later
• Chrome browser installed
• Internet connection for example websites

💡 Tips:
• Run examples from the Examples directory
• Each example includes comprehensive error handling
• Examples demonstrate both successful and error scenarios
• All examples include proper cleanup procedures

🔧 Customization:
• Modify URLs to test with different websites  
• Adjust selectors to target different elements
• Change timeout values for slower connections
• Add your own pipeline operations

🎓 Learning Path:
1. Start with 01-Basic-WebScraping.ps1 for fundamentals
2. Progress through 02-Form-Interaction.ps1 for user input
3. Explore 03-Multi-Page-Pipeline.ps1 for complex scenarios
4. Master 04-Advanced-Element-Interaction.ps1 for detailed control
5. Study 05-Pipeline-Chaining-ErrorHandling.ps1 for production patterns

🎉 Happy Automating!
"@ -ForegroundColor White

Write-Host "📋 Running Example Menu:" -ForegroundColor Yellow
Write-Host "1. Basic Web Scraping" -ForegroundColor Cyan
Write-Host "2. Form Interaction" -ForegroundColor Cyan  
Write-Host "3. Multi-Page Pipeline" -ForegroundColor Cyan
Write-Host "4. Advanced Element Interaction" -ForegroundColor Cyan
Write-Host "5. Pipeline Chaining & Error Handling" -ForegroundColor Cyan
Write-Host "A. Run All Examples" -ForegroundColor Magenta
Write-Host "Q. Quit" -ForegroundColor Red

$choice = Read-Host "Select an example to run (1-5, A, or Q)"

switch ($choice.ToUpper()) {
    "1" { 
        Write-Host "🚀 Running Basic Web Scraping Example..." -ForegroundColor Green
        .\01-Basic-WebScraping.ps1 
    }
    "2" { 
        Write-Host "🚀 Running Form Interaction Example..." -ForegroundColor Green
        .\02-Form-Interaction.ps1 
    }
    "3" { 
        Write-Host "🚀 Running Multi-Page Pipeline Example..." -ForegroundColor Green
        .\03-Multi-Page-Pipeline.ps1 
    }
    "4" { 
        Write-Host "🚀 Running Advanced Element Interaction Example..." -ForegroundColor Green
        .\04-Advanced-Element-Interaction.ps1 
    }
    "5" { 
        Write-Host "🚀 Running Pipeline Chaining & Error Handling Example..." -ForegroundColor Green
        .\05-Pipeline-Chaining-ErrorHandling.ps1 
    }
    "A" {
        Write-Host "🎬 Running All Examples..." -ForegroundColor Magenta
        Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
        
        $examples = @(
            "01-Basic-WebScraping.ps1",
            "02-Form-Interaction.ps1", 
            "03-Multi-Page-Pipeline.ps1",
            "04-Advanced-Element-Interaction.ps1",
            "05-Pipeline-Chaining-ErrorHandling.ps1"
        )
        
        foreach ($example in $examples) {
            Write-Host "📂 Starting: $example" -ForegroundColor Yellow
            try {
                & ".\$example"
                Write-Host "✅ Completed: $example" -ForegroundColor Green
            }
            catch {
                Write-Host "❌ Error in $example : $($_.Exception.Message)" -ForegroundColor Red
            }
            Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
            Start-Sleep -Seconds 2
        }
        
        Write-Host "🎉 All examples completed!" -ForegroundColor Magenta
    }
    "Q" { 
        Write-Host "👋 Goodbye!" -ForegroundColor Yellow
        exit 0
    }
    default { 
        Write-Host "❌ Invalid choice. Please select 1-5, A, or Q." -ForegroundColor Red 
    }
}