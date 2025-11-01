using System;
using PowerBrowser.Common;

public static class BrowserTypeValidator
{
    public static bool IsValid(string browserType)
    {
        return Enum.TryParse<SupportedPBrowser>(browserType, true, out var _);
    }
    public static void Validate(string browserType)
    {
        if (!IsValid(browserType))
            throw new ArgumentException($"Invalid browser type: {browserType}");
    }
}