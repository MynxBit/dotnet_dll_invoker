// File: src/DotNetDllInvoker.Reflection/SignatureBuilder.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Reconstructs a readable C# signature string from MethodInfo.
// Useful for UI display since we don't have full decompilation.
//
// Depends on:
// - System.Reflection
// - System.Text
//
// Execution Risk:
// None. String formatting.

using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNetDllInvoker.Reflection;

public static class SignatureBuilder
{
    public static string BuildSignature(MethodBase method)
    {
        var sb = new StringBuilder();

        // 1. Access Modifiers (Approximate)
        if (method.IsPublic) sb.Append("public ");
        else if (method.IsPrivate) sb.Append("private ");
        else if (method.IsFamily) sb.Append("protected ");
        else if (method.IsAssembly) sb.Append("internal ");
        
        if (method.IsStatic) sb.Append("static ");
        if (method.IsAbstract) sb.Append("abstract ");
        if (method.IsVirtual && !method.IsAbstract) sb.Append("virtual ");

        // 2. Return Type
        if (method is MethodInfo mi)
        {
            sb.Append(FormatTypeName(mi.ReturnType)).Append(" ");
        }
        else if (method is ConstructorInfo)
        {
            // No return type for ctor
        }

        // 3. Name
        sb.Append(method.Name);

        // 4. Generics
        if (method.IsGenericMethod)
        {
            sb.Append("<");
            var genericArgs = method.GetGenericArguments();
            sb.Append(string.Join(", ", genericArgs.Select(t => t.Name)));
            sb.Append(">");
        }

        // 5. Parameters
        sb.Append("(");
        var parameters = method.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            if (p.IsOut) sb.Append("out ");
            else if (p.ParameterType.IsByRef) sb.Append("ref ");
            
            // Handle params
            if (p.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
            {
                sb.Append("params ");
            }

            sb.Append(FormatTypeName(p.ParameterType)).Append(" ");
            sb.Append(p.Name);
            
            if (i < parameters.Length - 1) sb.Append(", ");
        }
        sb.Append(")");

        return sb.ToString();
    }

    private static string FormatTypeName(Type type)
    {
        // Handle common primitives for readability
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "int";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(void)) return "void";
        if (type == typeof(object)) return "object";
        if (type == typeof(double)) return "double";
        
        // Handle Generics (e.g. List<string>)
        if (type.IsGenericType)
        {
            var name = type.Name;
            int backtick = name.IndexOf('`');
            if (backtick > 0) name = name.Substring(0, backtick);
            
            var args = type.GetGenericArguments();
            return $"{name}<{string.Join(", ", args.Select(FormatTypeName))}>";
        }

        return type.Name;
    }
}
