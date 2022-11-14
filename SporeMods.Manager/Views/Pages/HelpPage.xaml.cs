using SporeMods.CommonUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SporeMods.Manager
{
    /// <summary>
    /// Interaction logic for HelpPage.xaml
    /// </summary>
    public partial class HelpPage : UserControl
    {
		public ObservableCollection<CreditsItem> Credits
		{
			get => (ObservableCollection<CreditsItem>)GetValue(CreditsProperty);
			set => SetValue(CreditsProperty, value);
		}

		public static readonly DependencyProperty CreditsProperty =
		DependencyProperty.Register(nameof(Credits), typeof(ObservableCollection<CreditsItem>), typeof(ManagerContent), new PropertyMetadata(new ObservableCollection<CreditsItem>()
		{
			new CreditsItem("Splitwirez (formerly rob55rod)", "Designed - and lead the effort to build - the Spore Mod Manager. (I couldn't have done it alone though!)", @"https://github.com/Splitwirez"),
			new CreditsItem("emd4600", "Started the Spore ModAPI Project. Created the Spore ModAPI Launcher Kit, which laid the foundations for the Spore Mod Manager. Helped build the Spore Mod Manager to be as robust as possible. Oh, and Spanish and Catalan translations.", @"https://github.com/emd4600"),
			new CreditsItem("reflectronic", "Provided invaluable guidance and assistance with code architecture, asynchronous behaviour, and working the inner machinations of .NET Core in the Spore Mod Manager's favor.", @"https://github.com/reflectronic"),
			//new CreditsItem("DotNetZip (formerly Ionic.Zip)", "Zip archive library used throughout the Spore Mod Manager.", @"https://www.nuget.org/packages/DotNetZip/"),
			new CreditsItem("Newtonsoft", "Created the Json.NET library, which is used for update retrieval.", @"https://www.newtonsoft.com/json"),
			new CreditsItem("cederenescio", "Indirectly provided substantial creative influence.", @"http://rso.bg/"),
			new CreditsItem("PricklySaguaro", "Found a way to run the Spore ModAPI Launcher Kit under WINE. Assisted with figuring out how to make WINE cooperate for the Spore Mod Manager.", @"https://github.com/PricklySaguaro"),
			new CreditsItem("Huskky", "Assisted substantially with figuring out how to make WINE cooperate."),
			new CreditsItem("Darhagonable", "Provided an additional perspective on usability. Helped confirm the feasibility of supporting WINE setups on Linux.", @"https://www.youtube.com/user/darhagonable"),
			new CreditsItem("Zakhar Afonin", "Assisted with figuring out how to make WINE cooperate.", @"https://github.com/AfoninZ"),
			new CreditsItem("Auntie Owl", "Polish translation, testing.", @"https://github.com/plencka"),
			new CreditsItem("bandithedoge", "WINE regression testing", @"http://bandithedoge.com/"),
			new CreditsItem("TheRublixCube", "Additional perspective on usability, testing."),
			new CreditsItem("Magic Gonads", "Testing", @"https://github.com/MagicGonads"),
			new CreditsItem("KloxEdge", "Testing"),
			new CreditsItem("Liskomato", "Testing"),
			new CreditsItem("ChocIce75", "Testing"),
			new CreditsItem("Deoxys_0", "Testing"),
			new CreditsItem("Psi", "Testing"),
			new CreditsItem("Ivy", "Testing"),
			new CreditsItem("Masaochism", "Testing"),
		}));

		public HelpPage()
        {
            InitializeComponent();
        }

		private void AskQuestionButton_Click(object sender, RoutedEventArgs e) =>
			OpenUrl(@"https://github.com/Splitwirez/Spore-Mod-Manager/issues/new?assignees=&labels=question&template=question.md&title=");

		private void SuggestFeatureButton_Click(object sender, RoutedEventArgs e) =>
			OpenUrl(@"https://github.com/Splitwirez/Spore-Mod-Manager/issues/new?assignees=&labels=enhancement&template=feature_request.md&title=");

		private void ReportBugButton_Click(object sender, RoutedEventArgs e) =>
			OpenUrl(@"https://github.com/Splitwirez/Spore-Mod-Manager/issues/new?assignees=&labels=bug&template=bug_report.md&title=");

		private void OpenUrl(string url)
		{
			WineHelper.OpenUrl(url, App.DragServantProcess);
		}
	}
}
