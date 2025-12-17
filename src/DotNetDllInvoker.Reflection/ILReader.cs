// File: src/DotNetDllInvoker.Reflection/ILReader.cs
// Project: DotNet DLL Invoker
//
// Responsibility:
// Parses raw byte arrays from method bodies into readable IL instructions.
// Provides the "IL" tab content.
//
// Depends on:
// - System.Reflection.Emit.OpCodes
//
// Execution Risk:
// Low. Passive parsing only.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DotNetDllInvoker.Reflection;

public class ILInstruction
{
    public int Offset { get; set; }
    public OpCode OpCode { get; set; }
    public object? Operand { get; set; }
    
    public override string ToString()
    {
        return $"IL_{Offset:x4}: {OpCode.Name} {Operand}";
    }
}

public class ILReader
{
    private static readonly Dictionary<short, OpCode> _opCodeCache = new();

    static ILReader()
    {
        foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType == typeof(OpCode))
            {
                var opCode = (OpCode)field.GetValue(null)!;
                _opCodeCache[opCode.Value] = opCode;
            }
        }
    }

    public static List<ILInstruction> Read(MethodBase method)
    {
        var body = method.GetMethodBody();
        if (body == null) return new List<ILInstruction>();

        var il = body.GetILAsByteArray();
        if (il == null) return new List<ILInstruction>();

        var instructions = new List<ILInstruction>();
        int position = 0;
        
        // Resolve module for token resolution
        var module = method.Module;

        while (position < il.Length)
        {
            int offset = position;
            short opCodeValue = il[position++];
            
            // Multi-byte opcode? (0xFE prefix)
            if (opCodeValue == 0xFE && position < il.Length)
            {
                opCodeValue = (short)(0xFE00 | il[position++]);
            }

            if (!_opCodeCache.TryGetValue(opCodeValue, out OpCode opCode))
            {
                // Unknown opcode, shouldn't happen often in standard IL
                instructions.Add(new ILInstruction { Offset = offset, OpCode = OpCodes.Nop, Operand = $"<Unknown: {opCodeValue:x}>" });
                continue;
            }

            object? operand = null;

            switch (opCode.OperandType)
            {
                case OperandType.InlineNone: break;
                case OperandType.ShortInlineBrTarget:
                    operand = (sbyte)il[position++] + position; // Target offset
                    break;
                case OperandType.InlineBrTarget:
                    operand = BitConverter.ToInt32(il, position) + position + 4;
                    position += 4;
                    break;
                case OperandType.ShortInlineI:
                    operand = (sbyte)il[position++];
                    break;
                case OperandType.InlineI:
                    operand = BitConverter.ToInt32(il, position);
                    position += 4;
                    break;
                case OperandType.InlineI8:
                    operand = BitConverter.ToInt64(il, position);
                    position += 8;
                    break;
                case OperandType.ShortInlineR:
                    operand = BitConverter.ToSingle(il, position);
                    position += 4;
                    break;
                case OperandType.InlineR:
                    operand = BitConverter.ToDouble(il, position);
                    position += 8;
                    break;
                case OperandType.ShortInlineVar:
                    operand = il[position++];
                    break;
                case OperandType.InlineVar:
                    operand = BitConverter.ToInt16(il, position);
                    position += 2;
                    break;
                case OperandType.InlineString:
                    var metadataToken = BitConverter.ToInt32(il, position);
                    position += 4;
                    try { operand = $"\"{module.ResolveString(metadataToken)}\""; } catch { operand = $"Token:{metadataToken:x}"; }
                    break;
                case OperandType.InlineSig:
                    position += 4; // Signature blob (handled as raw token usually)
                    operand = "SIG"; 
                    break;
                case OperandType.InlineMethod:
                case OperandType.InlineField:
                case OperandType.InlineType:
                case OperandType.InlineTok: // ldtoken
                    int token = BitConverter.ToInt32(il, position);
                    position += 4;
                    try 
                    { 
                        // Try resolving member
                        var member = module.ResolveMember(token);
                        operand = member?.Name ?? $"Token:{token:x}";
                        if (member is Type t) operand = t.Name;
                    } 
                    catch 
                    { 
                        operand = $"Token:{token:x}"; 
                    }
                    break;
                case OperandType.InlineSwitch:
                    int count = BitConverter.ToInt32(il, position);
                    position += 4;
                    position += count * 4; // Jump over targets
                    operand = "SWITCH";
                    break;
            }

            instructions.Add(new ILInstruction { Offset = offset, OpCode = opCode, Operand = operand });
        }

        return instructions;
    }
}
