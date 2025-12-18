using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DotNetDllInvoker.UI.Services;

/// <summary>
/// Manages a list of recently opened DLL files.
/// Stores in a JSON file in the app directory.
/// </summary>
public class RecentFilesService
{
    private const int MaxRecentFiles = 10;
    private const string FileName = "recent_files.json";
    
    private readonly string _filePath;
    private List<string> _recentFiles;

    public RecentFilesService()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, FileName);
        _recentFiles = Load();
    }

    public IReadOnlyList<string> RecentFiles => _recentFiles.AsReadOnly();

    public void AddRecent(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        
        // Remove if already exists (to move it to top)
        _recentFiles.Remove(path);
        
        // Insert at beginning
        _recentFiles.Insert(0, path);
        
        // Trim to max
        if (_recentFiles.Count > MaxRecentFiles)
        {
            _recentFiles = _recentFiles.Take(MaxRecentFiles).ToList();
        }
        
        Save();
    }

    private List<string> Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
        }
        catch
        {
            // Ignore errors, return empty list
        }
        return new List<string>();
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_recentFiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }
}
