# Security Policy

## ⚠️ Critical Warning
**DotNet DLL Invoker executes arbitrary code.**

By design, this tool uses `System.Reflection` to load assemblies and invoke methods. It is **NOT** a sandbox.

## 1. Static Constructor Risk
When an assembly is loaded (even for inspection), the .NET Runtime **MAY** execute:
*   Module Initializers
*   Static Constructors (`.cctor`) of accessed types

**Mitigation:**
*   This tool cannot prevent the CLR from running these initializers.
*   **ALWAYS** analyze unknown DLLs in an isolated Virtual Machine.

## 2. No Sandbox
This tool runs with the full privileges of the current user.
*   **Do not** run this tool as Administrator unless absolutely necessary.
*   **Do not** load malware on your host machine.

## 3. Reporting Vulnerabilities
If you find an escape from the *logic* boundaries (e.g., UI crashing due to invocation), please open an issue.
However, "The tool executed malicious code I told it to invoke" is **expected behavior**, not a vulnerability.

## 4. Intended Use
*   Malware Analysis (in VM)
*   Debugging
*   Security Research

**Rule of Thumb:** If you wouldn't double-click the DLL, don't load it in this tool on your host.
