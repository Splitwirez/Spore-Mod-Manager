using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core
{
    public interface IViewLocatable
    {
        string GetViewTypeName();
    }

    public class ViewLocatableBase : NotifyPropertyChangedBase, IViewLocatable
    {
        readonly string _viewTypeName = null;
        public virtual string GetViewTypeName()
            => _viewTypeName;

        public ViewLocatableBase()
        {
            string viewTypeNameNoNS = GetType().Name.Replace("ViewModel", "View");
            _viewTypeName = $"{nameof(SporeMods)}.Views.{viewTypeNameNoNS}";
        }
    }
}
