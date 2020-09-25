using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace SporeMods.CommonUI
{
    public static class MessageDisplay
    {
        static bool exceptionShown = false;
        public static void ShowException(Exception exception)
        {
            if (!exceptionShown)
            {
                exceptionShown = true;
                Exception current = exception;
                int count = 0;
                string errorText = "\n\nPlease send the contents this MessageBox and all which follow it to rob55rod\\Splitwirez, along with a description of what you were doing at the time.\n\nThe Spore Mod Manager will exit after the last Inner exception has been reported.";
                string errorTitle = "Something is very wrong here. Layer ";
                while (current != null)
                {
                    MessageBox.Show(current.GetType() + ": " + current.Message + "\n" + current.Source + "\n" + current.StackTrace + errorText, errorTitle + count);
                    count++;
                    current = current.InnerException;
                    if (count > 4)
                        break;
                }
                if (current != null)
                {
                    MessageBox.Show(current.GetType() + ": " + current.Message + "\n" + current.Source + "\n" + current.StackTrace + errorText, errorTitle + count);
                }
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
