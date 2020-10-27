using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace SporeMods.CommonUI
{
    public static class WineHelper
    {
        public static void SetClipboardContent(string newContent)
        {
            try
            {
                Clipboard.SetText(newContent);
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowClipboardFallback(Settings.GetLanguageString("CopypasteToTechSupport"), newContent);
            }
        }

        public static void OpenUrl(string url, Process dragServant)
        {
            if ((dragServant != null) && (!dragServant.HasExited))
                File.WriteAllText(Path.Combine(Settings.TempFolderPath, "OpenUrl"), url);
            else
            {
                bool showFallback = false;
                if (Settings.NonEssentialIsRunningUnderWine || (!Permissions.IsAdministrator()))
                {
                    try
                    {
                        Process process = Process.Start(url);

                        if (process == null)
                            showFallback = true;
                        else if (process.HasExited)
                            showFallback = true;
                    }
                    catch (Exception ex)
                    {
                        showFallback = true;
                    }
                }

                if (showFallback)
                    MessageDisplay.ShowClipboardFallback(Settings.GetLanguageString("CopyUrlIntoBrowser"), url);
            }
        }
    }
}
