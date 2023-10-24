using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LinuxProcessEnvVarsPOC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Open Spore, then close it once it opens.");
            //Console.WriteLine(args.Length);
            List<string> exeNames = (args.Length == 0)
                ? new List<string>()
                {
                    "SporeApp.exe",
                    "SporeApp_ModAPIFix.exe",
                }
                : args.ToList();
            
            
            List<Process> allProcesses = new List<Process>();
            
            while (allProcesses.Count <= 0)
            {
                //List<Process> allProcesses2 = new List<Process>();
                foreach (string name in exeNames)
                {
                    string exeName = name;
                    if (!exeName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        exeName = $"{name}.exe";
                    //Console.WriteLine(exeName);
                    var processes = Process.GetProcessesByName(exeName);
                    
                    //).Append(Process.GetProcessesByName("SporeApp_ModAPIFix.exe")).ToList()).ToArray()
                    foreach (Process process in processes)
                    {
                        allProcesses.Add(process);
                        Console.WriteLine("A process was found!");
                    }
                }
                //allProcesses = allProcesses2;
            }
            //Console.WriteLine($"Processes matched: {allProcesses.Count}");
            //TryGetWINEInfo(pid);

            List<(Process, string, string, int)> candidates = new List<(Process, string, string, int)>();
            foreach (Process process in allProcesses)
            {
                if (TryGetWINEInfo(process, out string winePrefix, out string wineExecutable, out int wineEsync))
                {
                    candidates.Add((process, winePrefix, wineExecutable, wineEsync));
                    Console.WriteLine($"PROCESS \"{process.ProcessName}\" (ID {process.Id})");
                    Console.WriteLine($"\tLAUNCHED BY \"{wineExecutable}\"");
                    Console.WriteLine($"\tIN PREFIX \"{winePrefix}\"");
                    Console.WriteLine("");
                }
                else
                {
                    Console.WriteLine("Hold up what");
                    Console.WriteLine($"name: {process.ProcessName}");
                }
            }

            while (candidates.Any(x => !x.Item1.HasExited))
            { }

            if (candidates.Count == 1)
            {
                Console.WriteLine("Initiating Spore Mod Manager installation...");
                var spore = candidates[0];
                string winePrefix = spore.Item2;
                string wineExecutable = spore.Item3;
                
                ProcessStartInfo smmWineInfo = new ProcessStartInfo(wineExecutable)
                {
                    UseShellExecute = false,
                    Arguments = "SporeModManagerSetup.exe"
                };
                smmWineInfo.EnvironmentVariables.Add("WINEPREFIX", winePrefix);
                if (spore.Item4 != -129)
                    smmWineInfo.EnvironmentVariables.Add("WINEESYNC", spore.Item4.ToString());
                var smmWine = new Process()
                {
                    StartInfo = smmWineInfo
                };
                smmWine.Start();
                smmWine.WaitForExit();
                string sh = "#!/bin/sh";
                string execute = $"WINEPREFIX=`realpath .` \"{wineExecutable}\" \"./drive_c/Program Files (x86)/Spore Mod Manager/";
                File.WriteAllLines(Path.Combine(winePrefix, "SMM.sh"), new string[]
                {
                    sh,
                    execute + "Spore Mod Manager.exe\""
                });
                File.WriteAllLines(Path.Combine(winePrefix, "SPORE.sh"), new string[]
                {
                    sh,
                    execute + "Launch Spore.exe\""
                });
                //Process.Start
            }
            else
                throw new NotImplementedException();
        }

        const string WINE_EXEC = "_=";
        const string PREFIX = "WINEPREFIX=";
        const string ESYNC = "WINEESYNC=";
        //wineEsync
        static bool TryGetWINEInfo(Process process, out string winePrefix, out string wineExecutable, out int wineEsync)
        {
            winePrefix = null;
            wineExecutable = null;
            wineEsync = -129;
            if (process == null)
                return false;
            else if (process.HasExited)
                return false;

            var lines = GetOutputForSProc(process.Id, "environ").Split('\0');
            //File.WriteAllLines(@"/home/splitwirez/Documents/Spore Modding/Proton/steam-spore-env-vars.txt", lines);
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                    continue;
                
                if (line.StartsWith(PREFIX) && (winePrefix == null))
                    winePrefix = line.Substring(PREFIX.Length);
                else if (line.StartsWith(WINE_EXEC) && (wineExecutable == null))
                    wineExecutable = line.Substring(WINE_EXEC.Length);
                else if (line.StartsWith(ESYNC) && (wineEsync == -129) && int.TryParse(line.Substring(ESYNC.Length), out int esync))
                    wineEsync = esync;
                /*else if ((winePrefix != null) && (wineExecutable != null))
                    break;*/
            }
            wineExecutable = Path.Combine(Path.GetDirectoryName(ReadLink(SProcFor(process.Id, "exe"))), "wine");
            //Console.WriteLine($"[wineExecutable dir: {Path.GetDirectoryName(wineExecutable)}]");
            //Console.WriteLine($"[wineExecutable: \"{wineExecutable}\"]");
            //Console.WriteLine($"[winePrefix \"{winePrefix}\"]");
            //lines = GetOutputForSmaps(process.Id).Split('\0');
            return (winePrefix != null) && (wineExecutable != null);
        }

        
        static string GetOutputForSmaps(int pid)
            => GetOutputForSProc(pid, "smaps");
        static string GetOutputForSProc(int pid, string subCmd)
        {
            var get = new Process()
            {
                StartInfo = GetProcCmd(pid, subCmd)
            };
            get.Start();
            string output = get.StandardOutput.ReadToEnd();
            get.WaitForExit();
            return output;
        }
        static ProcessStartInfo GetProcCmd(int pid, string subCmd)
            => new ProcessStartInfo()
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                //https://serverfault.com/questions/66363/environment-variables-of-a-running-process-on-unix/66366#66366
                FileName = "cat",
                Arguments = SProcFor(pid, subCmd)
            };
        static string SProcFor(int pid, string subCmd)
            => $"/proc/{pid}/{subCmd}";
        

        //https://stackoverflow.com/questions/58326739/how-can-i-find-the-target-of-a-linux-symlink-in-c-sharp
        //https://github.com/dotnet/coreclr/blob/master/src/System.Private.CoreLib/shared/Interop/Unix/System.Native/Interop.ReadLink.cs
        /// <summary>
        /// Takes a path to a symbolic link and attempts to place the link target path into the buffer. If the buffer is too
        /// small, the path will be truncated. No matter what, the buffer will not be null terminated.
        /// </summary>
        /// <param name="path">The path to the symlink</param>
        /// <param name="buffer">The buffer to hold the output path</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <returns>
        /// Returns the number of bytes placed into the buffer on success; bufferSize if the buffer is too small; and -1 on error.
        /// </returns>
        [DllImport("System.Native", EntryPoint = "SystemNative_ReadLink", SetLastError = true)]
        private static extern unsafe int ReadLink(string path, byte[] buffer, int bufferSize);

        /// <summary>
        /// Takes a path to a symbolic link and returns the link target path.
        /// </summary>
        /// <param name="path">The path to the symlink</param>
        /// <returns>
        /// Returns the link to the target path on success; and null otherwise.
        /// </returns>
        public static string? ReadLink(string path)
        {
            int bufferSize = 256;
            while (true)
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                try
                {
                    int resultLength = ReadLink(path, buffer, buffer.Length);
                    if (resultLength < 0)
                    {
                        // error
                        return null;
                    }
                    else if (resultLength < buffer.Length)
                    {
                        // success
                        return Encoding.UTF8.GetString(buffer, 0, resultLength);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }

                // buffer was too small, loop around again and try with a larger buffer.
                bufferSize *= 2;
            }
        }
    }
}
