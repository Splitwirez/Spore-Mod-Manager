using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SporeMods.Core
{
    public static class RegistryHelper
    {

        public static string[] RegistryValues = { "InstallLoc", "Install Dir" };

        public static string[] GalacticAdventuresRegistryKeys = {
                                             "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Electronic Arts\\SPORE_EP1",
                                             "HKEY_LOCAL_MACHINE\\SOFTWARE\\Electronic Arts\\SPORE_EP1"
                                         };

        public static string[] SporeRegistryKeys = {
                                             "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Electronic Arts\\SPORE",
                                             "HKEY_LOCAL_MACHINE\\SOFTWARE\\Electronic Arts\\SPORE"
                                                    };

        public static string[] CCRegistryKeys = {
                                             "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Electronic Arts\\SPORE(TM) Creepy & Cute Parts Pack",
                                             "HKEY_LOCAL_MACHINE\\SOFTWARE\\Electronic Arts\\SPORE(TM) Creepy & Cute Parts Pack"
                                         };


        public static string RegistryDataDir = "DataDir";  // Steam/GoG users don't have InstallLoc nor Install Dir

        public static string[] SporeRegistryValueNames =
        {
            "InstallLoc",
            "DataDir",
            "Install Dir"
        };

        public static string Wow6432Node = @"SOFTWARE\Wow6432Node";

        public static string GetFromRegistry(string key)
        {
            string result = null;
            foreach (string value in RegistryValues)
            {
                result = (string)Registry.GetValue(key, value, null);
                if (result != null)
                {

                    return result;
                }
            }
            return null;
        }

        public static string GetFromRegistry(string[] keys)
        {
            string result = null;

            foreach (string key in keys)
            {
                foreach (string value in RegistryValues)
                {
                    result = (string)Registry.GetValue(key, value, null);
                    if (result != null)
                    {

                        return result;
                    }
                }
            }

            // not found? try with DataDir; some users only have that one
            foreach (string key in GalacticAdventuresRegistryKeys)
            {
                result = (string)Registry.GetValue(key, RegistryDataDir, null);
                if (result != null)
                {

                    return result;
                }
            }

            return null;
        }


        public static string GetFromRegistry(string[] keys, string[] values)
        {
            string result = null;

            foreach (string key in keys)
            {
                foreach (string value in values)
                {
                    result = (string)Registry.GetValue(key, value, null);
                    if (result != null)
                    {

                        return result;
                    }
                }
            }

            return null;
        }
    }
}
