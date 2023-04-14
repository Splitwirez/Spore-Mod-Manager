using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core
{
    public class InlineDisposable : IDisposable
    {
        readonly Action _action;
        
        public InlineDisposable(Action action)
        {
            _action = action;
        }
        
        
        public void Dispose()
            => _action();
    }
}
