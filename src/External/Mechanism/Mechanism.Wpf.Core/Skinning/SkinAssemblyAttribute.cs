using System;
using System.Collections.Generic;
using System.Text;

namespace Mechanism.Wpf.Core.Skinning
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class SkinAssemblyAttribute : Attribute
    {
        public Type InterfaceImplType;

        public SkinAssemblyAttribute(Type interfaceImplType)
        {
            InterfaceImplType = interfaceImplType;
            bool containsSkinInfo = interfaceImplType.IsAssignableFrom(typeof(SkinAssemblyInfo)); /*false;
            foreach (Type intr in InterfaceImplType.GetInterfaces())
            {
                if (intr == typeof(SkinAssemblyInfo))
                {
                    containsSkinInfo = true;
                    break;
                }
            }*/
            /*if (!containsSkinInfo)
                throw new Exception("Type must implement \"Mechanism.Wpf.Core.Skinning.ISkinInfo\"!");*/
        }
    }
}
