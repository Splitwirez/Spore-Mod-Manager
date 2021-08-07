using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SporeMods.Core
{
	public class AppPathGuess : INotifyPropertyChanged
	{
		public AppPathGuess(string path, GameInfo.GameExecutableType type)
		{
			GuessPath = path;
			GuessType = type;
		}




		string _guessPath;
		public string GuessPath
		{
			get => _guessPath;
			set
			{
				_guessPath = value;
				NotifyPropertyChanged(nameof(GuessPath));
			}
		}


		GameInfo.GameExecutableType _guessType;
		public GameInfo.GameExecutableType GuessType
		{
			get => _guessType;
			set
			{
				_guessType = value;
				NotifyPropertyChanged(nameof(GuessType));
			}
		}

		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
