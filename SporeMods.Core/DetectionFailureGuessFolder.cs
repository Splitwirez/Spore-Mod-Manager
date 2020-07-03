using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SporeMods.Core
{
    public class DetectionFailureGuessFolder : INotifyPropertyChanged
    {
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

        public DetectionFailureGuessFolder(string path, GameInfo.GameExecutableType type)
        {
            GuessPath = path;
            GuessType = type;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
