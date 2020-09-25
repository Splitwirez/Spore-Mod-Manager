using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SporeMods.Core
{
    public static class MessageDisplay
    {
        public static string ErrorSeparator = "?\n?\n?\n?\n";
        public static string ErrorsSubdirectory = "Exceptions";

        public static event EventHandler<MessageBoxEventArgs> DebugMessageSent;
        public static event EventHandler<ErrorEventArgs> ErrorOccurred;
        public static event EventHandler<MessageBoxEventArgs> MessageBoxShown;

        public static void DebugShowMessageBox(string body)
        {
            DebugShowMessageBox(body, Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));
        }

        public static void DebugShowMessageBox(string body, string title)
        {
            Debug.WriteLine(title + ":\n" + body);
            if (Settings.DebugMode)
                DebugMessageSent?.Invoke(null, new MessageBoxEventArgs(title, body));
        }

        internal static void RaiseError(ErrorEventArgs args)
        {
            RaiseError(args, string.Empty);
        }

        internal static void RaiseError(ErrorEventArgs args, string modName)
        {
            string errorsSubDirectory = Path.Combine(Settings.ProgramDataPath, ErrorsSubdirectory);
            if (!string.IsNullOrWhiteSpace(modName))
                errorsSubDirectory = Path.Combine(Settings.ModConfigsPath, modName, ErrorsSubdirectory);

            if (!Directory.Exists(errorsSubDirectory))
                Directory.CreateDirectory(errorsSubDirectory);

            string errorPath = Path.Combine(errorsSubDirectory, GetExceptionFileName());
            File.WriteAllText(errorPath, args.Title + ErrorSeparator + args.Content);
            Permissions.GrantAccessFile(errorPath);
            ErrorOccurred?.Invoke(null, args);
        }

        internal static void ShowMessageBox(string content)
        {
            ShowMessageBox(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location), content);
        }

        internal static void ShowMessageBox(string title, string content)
        {
            MessageBoxShown?.Invoke(null, new MessageBoxEventArgs(title, content));
        }



        static string GetExceptionFileName()
        {
            string now = DateTime.Now.ToLongTimeString();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (now.Contains(c))
                    now = now.Replace(c.ToString(), string.Empty);
            }

            return Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location) + "!!" + now + ".info";
        }
    }
}
