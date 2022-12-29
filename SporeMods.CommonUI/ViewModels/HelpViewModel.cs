using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SporeMods.Core;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;

namespace SporeMods.ViewModels
{
	public class HelpViewModel : NotifyPropertyChangedBase
	{
		public string SMMVersion
		{
			get => Settings.ModManagerVersion.ToString();
		}

		public string SMMBuildChannel
		{
			get => Settings.BuildChannel;
		}

		public string ModAPIDLLsBuild
		{
			get => Settings.CurrentDllsBuildString;
		}

		public string SporeAppVersion
		{
			get => "NYI (PLACEHOLDER) (NOT LOCALIZED)";
		}

		public string DotnetTarget
		{
			get => Settings.TargetFramework;
		}

		public string DotnetRunningUnder
		{
			get => Environment.Version.ToString();
		}

		public string EnvOSVersionWindowsVersion
		{
			get => Environment.OSVersion.Version.ToString();
		}


		string _rtlGetVersionWindowsVersion = string.Empty;
		public string RtlGetVersionWindowsVersion
		{
			get => _rtlGetVersionWindowsVersion;
			private set
			{
				_rtlGetVersionWindowsVersion = value;
				NotifyPropertyChanged();
			}
		}

		string _rtlGetVersionServicePack = string.Empty;
		public string RtlGetVersionServicePack
		{
			get => _rtlGetVersionServicePack;
			private set
			{
				_rtlGetVersionServicePack = value;
				NotifyPropertyChanged();
			}
		}

		string _rtlGetVersionOther = string.Empty;
		public string RtlGetVersionOther
		{
			get => _rtlGetVersionOther;
			private set
			{
				_rtlGetVersionOther = value;
				NotifyPropertyChanged();
			}
		}

		
		string _rtlGetVersionResult = string.Empty;
		public string RtlGetVersionResult
		{
			get => _rtlGetVersionResult;
		}

		public string WINEVersion
		{
			get => Settings.GetWineVersionResult.IsNullOrEmptyOrWhiteSpace() ? LanguageManager.Instance.GetLocalizedText("Help!DiagnosticInfo!WINEVersion!NoneReturned") : Settings.GetWineVersionResult;
		}


		public HelpViewModel()
		{
			NativeMethods.OSVERSIONINFOEXW info = new NativeMethods.OSVERSIONINFOEXW();
			NativeMethods.RtlGetVersion(ref info);
			RtlGetVersionWindowsVersion = $"{info.dwMajorVersion}.{info.dwMinorVersion}.{info.dwBuildNumber}";
			RtlGetVersionServicePack = $"{info.wServicePackMajor}.{info.wServicePackMinor}";
			RtlGetVersionOther = $"size: {info.dwOSVersionInfoSize},\nplatformId: {info.dwPlatformId},\ncsdVersion: {info.szCSDVersion},\nsuiteMask: {info.wSuiteMask},\nproductType: {info.wProductType}";
			/*@$"size: {info.dwOSVersionInfoSize}
            major: {info.dwMajorVersion}
            minor: {info.dwMinorVersion}
            build: {info.dwBuildNumber}
            platformId: {info.dwPlatformId}
            csdVersion: {info.szCSDVersion}
            servicePackMajor: {info.wServicePackMajor}
            servicePackMinor: {info.wServicePackMinor}
            suiteMask: {info.wSuiteMask}
            productType: {info.wProductType}";*/
		}


		public FuncCommand<object> AskQuestionCommand
			= new FuncCommand<object>(_ => WineHelper.OpenUrl(@"https://github.com/Splitwirez/Spore-Mod-Manager/issues/new?assignees=&labels=question&template=question.md&title="));

		public FuncCommand<object> MakeSuggestionCommand
			= new FuncCommand<object>(_ => WineHelper.OpenUrl(@"https://github.com/Splitwirez/Spore-Mod-Manager/issues/new?assignees=&labels=enhancement&template=feature_request.md&title="));

		public FuncCommand<object> ReportBugCommand
			= new FuncCommand<object>(_ => WineHelper.OpenUrl(@"https://github.com/Splitwirez/Spore-Mod-Manager/issues/new?assignees=&labels=bug&template=bug_report.md&title="));

		/*public void OpenUrlCommand(object parameter)
		{
			if (parameter is string url)
				OpenUrl(url);
		}*/

		public static void OpenUrl(string url)
			=> WineHelper.OpenUrl(url);
	}
}
