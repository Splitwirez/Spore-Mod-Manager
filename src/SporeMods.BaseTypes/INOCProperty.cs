using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace SporeMods.BaseTypes
{
    public interface INOCProperty
    {
        void Refresh();

        string Name
        {
            get;
        }
    }
}