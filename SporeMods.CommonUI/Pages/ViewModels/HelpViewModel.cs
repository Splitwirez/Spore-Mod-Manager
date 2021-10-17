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
		/*ObservableCollection<CreditsItem> _credits = new ObservableCollection<CreditsItem>()
		{
			new CreditsItem("Splitwirez (formerly rob55rod)", "Designed and (mostly) built the Spore Mod Manager.", @"https://github.com/Splitwirez/"),
			new CreditsItem("emd4600", "Started the Spore ModAPI Project. Created the Spore ModAPI Launcher Kit, which laid the foundations for the Spore Mod Manager. Helped build the Spore Mod Manager to be as robust as possible. Oh, and Spanish and Catalan translations.", @"https://github.com/emd4600/"),
			new CreditsItem("reflectronic", "Provided invaluable guidance and assistance with code architecture, asynchronous behaviour, and working the inner machinations of .NET Core in the Spore Mod Manager's favor.", @"https://github.com/reflectronic/"),
			//new CreditsItem("DotNetZip (formerly Ionic.Zip)", "Zip archive library used throughout the Spore Mod Manager.", @"https://www.nuget.org/packages/DotNetZip/"),
			new CreditsItem("Newtonsoft", "Made the library to read JSON data.", @"https://www.newtonsoft.com/json"),
			new CreditsItem("cederenescio", "Indirectly provided substantial creative influence."),
			new CreditsItem("PricklySaguaro/ThePixelMouse", "Found a way to run the Spore ModAPI Launcher Kit under WINE. Assisted with figuring out how to make WINE cooperate for the Spore Mod Manager.", @"https://github.com/PricklySaguaro"),
			new CreditsItem("Huskky", "Assisted substantially with figuring out how to make WINE cooperate."),
			new CreditsItem("Darhagonable", "Provided an additional perspective on usability. Helped confirm the feasibility of supporting WINE setups on Linux.", @"https://www.youtube.com/user/darhagonable"),
			new CreditsItem("Zakhar Afonin", "WINE testing [TODO: probably Russian translation]", @"https://github.com/AfoninZ"),
			new CreditsItem("Auntie Owl", "Testing, Polish translation.", @"https://github.com/plencka"),
			new CreditsItem("bandithedoge", "WINE regression testing", @"http://bandithedoge.com/"),
			new CreditsItem("TheRublixCube", "Testing, additional perspective on usability."),
			new CreditsItem("Magic Gonads", "Testing", @"https://github.com/MagicGonads"),
			new CreditsItem("KloxEdge", "Testing"),
			new CreditsItem("Liskomato", "Testing"),
			new CreditsItem("ChocIce75", "Testing"),
			new CreditsItem("Deoxys_0", "Testing"),
			new CreditsItem("Psi", "Testing"),
			new CreditsItem("Ivy", "Testing"),
			new CreditsItem("Masaochism", "Testing"),
		};

		public ObservableCollection<CreditsItem> Credits
		{
			get => _credits;
			private set
			{
				_credits = value;
				NotifyPropertyChanged();
			}
		}*/
		
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
