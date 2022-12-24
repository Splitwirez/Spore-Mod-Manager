using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SporeMods.Core.Mods
{
    public class ModException : FormatException
    {
        public readonly bool ShouldCheckModAgainstOtherTypes = true;
        /*public ModException()
            : this(null)
        {}
        public ModException(string? message)
            : this(message, null)
        {}
        public ModException(string? message, Exception? innerException)
            : this(message, innerException)
        {

        }*/

        public ModException(bool shouldCheckModAgainstOtherTypes, string? message = null, Exception? innerException = null)
            : base(message, innerException)
        {
            ShouldCheckModAgainstOtherTypes = shouldCheckModAgainstOtherTypes;
        }

        Exceptions _priorExceptions = new Exceptions();
        public Exceptions PriorExceptions
        {
            get => _priorExceptions;
        }
    }

    public sealed class Exceptions : ObservableCollection<Exception>
    {

    }
}
