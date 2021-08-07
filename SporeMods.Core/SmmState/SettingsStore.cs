using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SporeMods.NotifyOnChange;
using static SporeMods.Core.GameInfo;

namespace SporeMods.Core
{
	public static class SettingsStore
	{
        const string SETTINGS_DOC_NAME = "ModManagerSettings.xml";
        static string _settingsDocPath = string.Empty; //Path.Combine(SmmInfo.EnsureInstance().StoragePath, SETTINGS_DOC_NAME);
        static XDocument _settingsDocument;

        
        public static void ReparseSettingsDoc() =>
            ReparseSettingsDoc(SmmInfo.StoragePath);

        public static void ReparseSettingsDoc(string path)
        {
			_settingsDocPath = Path.Combine(path, SETTINGS_DOC_NAME);
			if (!File.Exists(_settingsDocPath))
			{
				WriteSettingsXmlFile();
			}

			try
			{
				_settingsDocument = XDocument.Load(_settingsDocPath);
			}
			catch (XmlException ex)
			{
				WriteSettingsXmlFile();
				_settingsDocument = XDocument.Load(_settingsDocPath);
			}
		}


        static void WriteSettingsXmlFile()
		{
			XDocument document = new XDocument(new XElement("Settings"));
			document.Root.Add(new XElement(SmmInfo._lastMgrVersion, SmmInfo.CurrentVersion));

			/*string xmlStart = @"<Settings>";
			string xmlMiddle = @"
	<" + _lastMgrVersion + ">" + ModManagerVersion.ToString() + "</" + _lastMgrVersion + ">";
			string xmlEnd = @"</Settings>";*/

			SmmInfo.AddToSettingsXmlFile(ref document);
			//xmlMiddle += "\n    <" + _isWineMode + ">True</" + _isWineMode + ">";

			/*File.WriteAllText(_settingsFilePath, xmlStart + xmlMiddle + xmlEnd);
			Permissions.GrantAccessFile(_settingsFilePath);*/
			document.Save(_settingsDocPath);
		}

		static XElement RootElement
		{
			get => (_settingsDocument.Descendants("Settings").ToArray()[0] as XElement);
		}



		public static string GetValue(string elementName, string defaultValue = null)
		{
			XElement element = RootElement.Element(elementName);
			
			if (element != null)
				return element.Value;
			else
				return defaultValue;
		}

		public static void SetValue(string elementName, string value)
		{
			if (value.IsNullOrEmptyOrWhiteSpace())
				RootElement.SetElementValue(elementName, null);
			else
				RootElement.SetElementValue(elementName, value);
			_settingsDocument.Save(_settingsDocPath);
		}
    }
}