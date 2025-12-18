// File: ui/DotNetDllInvoker.UI/ViewModels/JsonParameterViewModel.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// ViewModel for complex object parameters using JSON input.
// Deserializes JSON string to target type using System.Text.Json.
//
// Depends on:
// - System.Reflection
// - System.Text.Json
//
// Execution Risk:
// Low. JSON deserialization of user input.

using System;
using System.Reflection;
using System.Text.Json;

namespace DotNetDllInvoker.UI.ViewModels;

public class JsonParameterViewModel : ParameterViewModel
{
    private string _jsonText = "{}";
    private string? _validationError;

    public JsonParameterViewModel(ParameterInfo info) : base(info)
    {
        TargetType = info.ParameterType;
        
        // Generate sample JSON structure
        _jsonText = GenerateSampleJson(TargetType);
    }

    public Type TargetType { get; }

    public string JsonText
    {
        get => _jsonText;
        set
        {
            if (SetProperty(ref _jsonText, value))
            {
                ValidateJson();
            }
        }
    }

    public string? ValidationError
    {
        get => _validationError;
        private set => SetProperty(ref _validationError, value);
    }

    public bool IsValid => string.IsNullOrEmpty(_validationError);

    public override object? GetValue()
    {
        if (string.IsNullOrWhiteSpace(_jsonText))
            return null;

        try
        {
            return JsonSerializer.Deserialize(_jsonText, TargetType, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    private void ValidateJson()
    {
        if (string.IsNullOrWhiteSpace(_jsonText))
        {
            ValidationError = null;
            return;
        }

        try
        {
            JsonSerializer.Deserialize(_jsonText, TargetType, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            ValidationError = null;
        }
        catch (JsonException ex)
        {
            ValidationError = $"Invalid JSON: {ex.Message}";
        }
        catch (Exception ex)
        {
            ValidationError = $"Error: {ex.Message}";
        }
    }

    private static string GenerateSampleJson(Type type)
    {
        try
        {
            // Try to create a default instance and serialize it
            var instance = Activator.CreateInstance(type);
            if (instance != null)
            {
                return JsonSerializer.Serialize(instance, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
        }
        catch
        {
            // Fall through to manual generation
        }

        // Manual generation based on properties
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("{");
        
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .Take(10); // Limit to avoid overwhelming

        var first = true;
        foreach (var prop in props)
        {
            if (!first) sb.AppendLine(",");
            first = false;
            
            var sampleValue = GetSampleValue(prop.PropertyType);
            sb.Append($"  \"{prop.Name}\": {sampleValue}");
        }
        
        sb.AppendLine();
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GetSampleValue(Type type)
    {
        if (type == typeof(string)) return "\"\"";
        if (type == typeof(int) || type == typeof(long)) return "0";
        if (type == typeof(double) || type == typeof(float) || type == typeof(decimal)) return "0.0";
        if (type == typeof(bool)) return "false";
        if (type == typeof(DateTime)) return $"\"{DateTime.Now:yyyy-MM-dd}\"";
        if (type.IsArray || IsGenericList(type)) return "[]";
        if (type.IsClass) return "null";
        return "null";
    }

    private static bool IsGenericList(Type type)
    {
        return type.IsGenericType && 
               type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>);
    }
}
