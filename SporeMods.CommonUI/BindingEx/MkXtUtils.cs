using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace SporeMods.CommonUI
{
    public static class MkXtUtils
    {
        static readonly Type _PV_TARGET = typeof(IProvideValueTarget);
        public static bool TryGetPVTarget(in IServiceProvider provider, out IProvideValueTarget pvTarget)
        {
            if (provider == null)
                goto fail;

            var service = provider.GetService(_PV_TARGET);
            if (service == null)
                goto fail;



            if (service is IProvideValueTarget target)
            {
                pvTarget = target;
                return pvTarget != null;
            }



            fail:
            pvTarget = null;
            return false;
        }

        public static bool TryGetPvtObject<T>(in IProvideValueTarget pvTarget, out T target)
        {
            if (pvTarget.TargetObject is T trgObj)
            {
                target = trgObj;
                return true;
            }

            target = default;
            return false;
        }

        public static bool TryGetPvtStuff<T>(in IProvideValueTarget pvTarget, out T target, out DependencyProperty property)
            where T : DependencyObject
        {
            property = (pvTarget.TargetProperty is DependencyProperty dp)
                ? dp
                : null
            ;

            if (pvTarget.TargetObject is T trgObj)
            {
                target = trgObj;
                return true;
            }

            target = default;
            return false;
        }
    }
}
