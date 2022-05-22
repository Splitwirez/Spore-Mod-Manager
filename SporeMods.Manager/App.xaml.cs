using SporeMods.Core;
using SporeMods.CommonUI;
using SporeMods.CommonUI.Localization;
using CUIMsg = SporeMods.CommonUI.MessageDisplay;
using CoreMsg = SporeMods.Core.MessageDisplay;
using SporeMods.Views;
using SporeMods.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace SporeMods.Manager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : SmmApp
	{
		protected override bool ShouldRerunAsAdministrator()
			=> true;
		protected override bool ShouldEnsureUACPartner()
			=> true;


		protected override void FinishStartup(bool isAdmin)
		{
			if (isAdmin)
			{
				//TODO: Implement this stuff correctly
#if MOD_IMPL_RESTORE_LATER
				Core.ModTransactions.Operations.ValidateModOp.InstallingExperimentalMod += s => true;
				Core.ModTransactions.Operations.ValidateModOp.InstallingRequiresGalaxyResetMod += s => true;
				Core.ModTransactions.Operations.ValidateModOp.InstallingSaveDataDependencyMod += s => true;
				Core.ModTransactions.ModTransactionManager.UninstallingSaveDataDependencyMod += m => true;
#endif
				MainWindow = new MainView();
				MainWindow.Show();
				var vm = new MainViewModel();
				MainWindow.DataContext = vm;
			}
			else
				UACPartnerCommands.WatchForPartnerSignals = true;
		}
	}
}
