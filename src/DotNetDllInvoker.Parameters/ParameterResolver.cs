// File: src/DotNetDllInvoker.Parameters/ParameterResolver.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// orchestrates the decision between using user input (Manual) and automatic generation.
// Performs type conversion for string inputs.
//
// Depends on:
// - System.Reflection
// - DotNetDllInvoker.Contracts
// - DotNetDllInvoker.Shared
//
// Execution Risk:
// Low. Type conversion logic.

using System;
using System.Reflection;
using DotNetDllInvoker.Contracts;
using DotNetDllInvoker.Shared;

namespace DotNetDllInvoker.Parameters;

public class ParameterResolver : IParameterResolver
{
    private readonly AutoParameterGenerator _autoGenerator;

    public ParameterResolver()
    {
        _autoGenerator = new AutoParameterGenerator();
    }

    public object? Resolve(ParameterInfo parameter, string? userInput)
    {
        Guard.NotNull(parameter, nameof(parameter));

        // If user input is provided, attempt conversion
        if (userInput != null)
        {
            try
            {
                Type targetType = parameter.ParameterType;
                
                // Handle basic string mapping first
                if (targetType == typeof(string)) return userInput;

                // Handle Enums manually to support names
                if (targetType.IsEnum)
                {
                    return Enum.Parse(targetType, userInput, ignoreCase: true);
                }

                // Handle GUIDs
                if (targetType == typeof(Guid))
                {
                    return Guid.Parse(userInput);
                }

                // Use ChangeType for primitives
                return Convert.ChangeType(userInput, targetType);
            }
            catch (Exception)
            {
                // Fallback? Or throw? Interface implies we return a value. 
                // If manual conversion fails, we should probably throw so UI knows.
                throw new ArgumentException($"Failed to convert input '{userInput}' to type '{parameter.ParameterType.Name}'.");
            }
        }

        // Check if optional
        if (parameter.IsOptional && parameter.HasDefaultValue)
        {
            return parameter.DefaultValue;
        }

        return GenerateDefault(parameter); // Fallback to auto
    }

    public object? GenerateDefault(ParameterInfo parameter)
    {
        Guard.NotNull(parameter, nameof(parameter));
        return _autoGenerator.Generate(parameter.ParameterType);
    }
    
    public object? GenerateDefault(Type type)
    {
        Guard.NotNull(type, nameof(type));
        return _autoGenerator.Generate(type);
    }
}
