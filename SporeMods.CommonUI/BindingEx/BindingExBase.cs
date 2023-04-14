using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SporeMods.CommonUI
{
    //massive thanks to http://www.hardcodet.net/2008/04/wpf-custom-binding-class
    public abstract partial class BindingExBase : MarkupExtension
    {
        Binding _binding = new Binding();
        /// <summary>
        /// The decorated binding class.
        /// </summary>
        protected Binding ActualBinding
        {
            get => _binding;
            private set => _binding = value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (!MkXtUtils.TryGetPVTarget(serviceProvider, out IProvideValueTarget pvt))
                return null;


            FrameworkElement target = null;
            DependencyProperty prop = null;
            if (!MkXtUtils.TryGetPvtStuff(pvt, out FrameworkElement trg, out DependencyProperty dp))
            {
                target = trg;
                prop = dp;
            }

            var binding = ActualBinding;
            bool shouldSetBinding = PrepareBinding(pvt, ref binding, in target, in prop);
            ActualBinding = binding;
            if (shouldSetBinding)
                target?.SetBinding(prop, binding);

            return binding.ProvideValue(serviceProvider);
        }

        protected virtual bool PrepareBinding(in IProvideValueTarget pvt, ref Binding binding, in FrameworkElement target, in DependencyProperty prop)
            => true;
    }
}
