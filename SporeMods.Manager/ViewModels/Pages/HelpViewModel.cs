using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SporeMods.Core;
using SporeMods.CommonUI;
using App = SporeMods.Manager.App;

namespace SporeMods.ViewModels
{
	public class HelpViewModel : NotifyPropertyChangedBase
	{
		ObservableCollection<CreditsItem> _credits = new ObservableCollection<CreditsItem>()
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
		}


		public void AskQuestionCommand(object parameter)
			=> OpenUrl(@"https://github.com/Splitwirez/Spore-Mod-Manager/issues/new?assignees=&labels=question&template=question.md&title=");

		public void MakeSuggestionCommand(object parameter)
			=> OpenUrl(@"https://github.com/Splitwirez/Spore-Mod-Manager/issues/new?assignees=&labels=enhancement&template=feature_request.md&title=");

		public void ReportBugCommand(object parameter)
			=> OpenUrl(@"https://github.com/Splitwirez/Spore-Mod-Manager/issues/new?assignees=&labels=bug&template=bug_report.md&title=");

		public void OpenUrlCommand(object parameter)
		{
			if (parameter is string url)
				OpenUrl(url);
		}

		public static void OpenUrl(string url)
			=> WineHelper.OpenUrl(url, App.DragServantProcess);
	}
}
