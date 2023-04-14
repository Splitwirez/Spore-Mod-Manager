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

		/*
		public string EnvOSVersionWindowsVersion
		{
			get => Environment.OSVersion.Version.ToString();
		}

		public string EnvIs64BitOS
		{
			get => Environment.Is64BitOperatingSystem.ToString();
		}

		public string EnvIs64BitProcess
		{
			get => Environment.Is64BitProcess.ToString();
		}*/

		const string _DIAGINFO_KEY_PREFIX = "Help!DiagnosticInfo!";
		static readonly string _DOTNET_OSINFO_KEY_PREFIX = $"{_DIAGINFO_KEY_PREFIX}DotnetOSInfo!";
		readonly Dictionary<string, string> _dotnetOSInfo = new Dictionary<string, string>()
		{
			{
				$"{_DOTNET_OSINFO_KEY_PREFIX}OSVersion",
				Environment.OSVersion.Version.ToString()
			},
			{
				$"{_DOTNET_OSINFO_KEY_PREFIX}Is64BitOperatingSystem",
				Environment.Is64BitOperatingSystem.ToString()
			},
			{
				$"{_DOTNET_OSINFO_KEY_PREFIX}Is64BitProcess",
				Environment.Is64BitProcess.ToString()
			},
		};
		public Dictionary<string, string> DotnetOSInfo
		{
			get => _dotnetOSInfo;
		}


		/*
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
		}*/

		static readonly string _RTLGETVERSION_KEY_PREFIX = $"{_DIAGINFO_KEY_PREFIX}RtlGetVersionOSInfo!";
		readonly Dictionary<string, string> _rtlGetVersionDetails;
		public Dictionary<string, string> RtlGetVersionDetails
		{
			get => _rtlGetVersionDetails;
		}

		public string WINEVersion
		{
			get => Settings.GetWineVersionResult.IsNullOrEmptyOrWhiteSpace() ? LanguageManager.Instance.GetLocalizedText("Help!DiagnosticInfo!WINEVersion!NoneReturned") : Settings.GetWineVersionResult;
		}


		public HelpViewModel()
		{
			NativeMethods.OSVERSIONINFOEXW info = new NativeMethods.OSVERSIONINFOEXW();
			NativeMethods.RtlGetVersion(ref info);
			//RtlGetVersionWindowsVersion = $"{info.dwMajorVersion}.{info.dwMinorVersion}.{info.dwBuildNumber}";
			//RtlGetVersionServicePack = $"{info.wServicePackMajor}.{info.wServicePackMinor}";
			//RtlGetVersionOther = $"size: {info.dwOSVersionInfoSize},\nplatformId: {info.dwPlatformId},\ncsdVersion: {info.szCSDVersion},\nsuiteMask: {info.wSuiteMask},\nproductType: {info.wProductType}";
			_rtlGetVersionDetails = new Dictionary<string, string>()
			{
				{
					$"{_RTLGETVERSION_KEY_PREFIX}OSVersion",
					$"{info.dwMajorVersion}.{info.dwMinorVersion}.{info.dwBuildNumber}"
				},
			/*};
			if (!spName.IsNullOrEmptyOrWhiteSpace())
			{
				_rtlGetVersionDetails.Add(*/
				{
					$"{_RTLGETVERSION_KEY_PREFIX}SPName",
					info.szCSDVersion.IsNullOrEmptyOrWhiteSpace()
						? info.szCSDVersion
						: string.Empty
				},
				/*);
			}
			Dictionary<string, string> rtlGetVersionDetailsExtras = new()
			{*/
				{
					$"{_RTLGETVERSION_KEY_PREFIX}SPVersion",
					$"{info.wServicePackMajor}.{info.wServicePackMinor}"
				},
				{
					$"{_RTLGETVERSION_KEY_PREFIX}{nameof(NativeMethods.OSVERSIONINFOEXW.dwOSVersionInfoSize)}",
					$"{info.dwOSVersionInfoSize}"
				},
				{
					$"{_RTLGETVERSION_KEY_PREFIX}{nameof(NativeMethods.OSVERSIONINFOEXW.dwPlatformId)}",
					$"{info.dwPlatformId}"
				},
				{
					$"{_RTLGETVERSION_KEY_PREFIX}{nameof(NativeMethods.OSVERSIONINFOEXW.wSuiteMask)}",
					//$"{info.wSuiteMask}"
					/*new Func<string>(() =>
					{
						List<NativeMethods.SuiteMask> suiteMask = new List<NativeMethods.SuiteMask>();
						var suiteMaskRaw = info.wSuiteMask;
						var enumVals = Enum.GetValues<NativeMethods.SuiteMask>();
						foreach (NativeMethods.SuiteMask val in enumVals)
						{
							suiteMask
						}

						return ret;
					})()*/
					GetAllFlagsString(info.wSuiteMask)
				},
				{
					$"{_RTLGETVERSION_KEY_PREFIX}{nameof(NativeMethods.OSVERSIONINFOEXW.wProductType)}",
					//$"{info.wProductType}"
					GetAllFlagsString((NativeMethods.ProductType)0x0000001) //(info.wProductType)
				},
			};
			
			/*foreach (var entry in rtlGetVersionDetailsExtras)
				_rtlGetVersionDetails.Add(entry.Key, entry.Value);*/
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

		static IEnumerable<TEnum> GetAllFlags<TEnum>(TEnum rawValue)
			where TEnum : Enum
		{
			List<TEnum> allFlags = new List<TEnum>();
			var enumVals = Enum.GetValues(typeof(TEnum));
			foreach (TEnum val in enumVals)
			{
				if (rawValue.HasFlag(val))
					allFlags.Add(val);
			}

			return allFlags;
		}

		static string GetAllFlagsString<T>(T rawValue)
			where T : Enum
		{
			var allFlags = GetAllFlags(rawValue);
			if (allFlags.Count() <= 0)
				return string.Empty;
			
			string ret = allFlags.First().ToString();
			if (allFlags.Count() == 1)
				return ret;
			
			allFlags = allFlags.Skip(1);
			foreach (T val in allFlags)
			{
				ret += $", {val}";
			}
			
			return (ret.Length > 2)
				? ret.Substring(2)
				: ret
			;
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
