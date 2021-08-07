using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SporeMods.NotifyOnChange;
using SporeMods.Core;

namespace SporeMods.Core
{
	public class AppPathAutoDetectFail : NOCObject, IModalViewModel<string>
	{
		TaskCompletionSource<string> _completionSource = new TaskCompletionSource<string>();

		public TaskCompletionSource<string> GetCompletionSource()
		{
			return _completionSource;
		}

		public AppPathAutoDetectFail()
			: base()
		{
			_selectedGuess = AddProperty<NOCRespondProperty<AppPathGuess>>(new NOCRespondProperty<AppPathGuess>(nameof(SelectedGuess))
			{
				ValueChangeResponse = (x, o, n) =>
				{
					if (n != null)
					{
						_resultPath = n.GuessPath;
					}
				}
			});
		}

		public AppPathAutoDetectFail Guess(AppPath path)
		{
			

			return this;
		}

		ObservableCollection<AppPathGuess> _guesses;
		public ObservableCollection<AppPathGuess> Guesses
		{
			get => _guesses;
		}


		NOCRespondProperty<AppPathGuess> _selectedGuess;
		public AppPathGuess SelectedGuess
		{
			get => _selectedGuess.Value;
		}

		
		string _resultPath = null;
		public string ResultPath
		{
			get => _resultPath;
		}
	}
}
