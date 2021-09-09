/*using SporeMods.Core.ModTransactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Core.Mods
{
	public class InstallError : IInstalledMod, INotifyPropertyChanged
	{
		string _displayName = string.Empty;
		public string DisplayName
		{
			get => _displayName;
			set
			{
				_displayName = value;
				NotifyPropertyChanged(nameof(DisplayName));
			}
		}

		public string Unique => string.Empty;

		public string RealName => string.Empty;

		public bool IsProgressing => false;

		public bool HasConfigsDirectory => false;

		public string Description => _installException.Message;

		public Version ModVersion => ModIdentity.UNKNOWN_MOD_VERSION;

		public List<string> Tags { get; } = new List<string>();

		public Task<ModTransactionCommitException> UninstallModAsync()
		{
			throw new NotImplementedException();
		}

		Exception _installException = null;
		public Exception InstallException
		{
			get => _installException;
			set
			{
				_installException = value;
				NotifyPropertyChanged(nameof(InstallException));
			}
		}

		public InstallError(Exception exception)
		{
			InstallException = exception;
		}

		public InstallError(string modName, Exception exception) : this(exception)
		{
			DisplayName = modName;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
*/