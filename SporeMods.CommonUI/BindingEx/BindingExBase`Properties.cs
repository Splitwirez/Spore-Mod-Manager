using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;


namespace SporeMods.CommonUI
{
    partial class BindingExBase
    {
        //check documentation of the Binding class for property information

        /// <summary>
        /// The decorated binding class.
        /// </summary>
        [Browsable(false)]
        public Binding Binding
        {
            get => ActualBinding;
            set => ActualBinding = value;
        }


        [DefaultValue(null)]
        public object AsyncState
        {
            get => ActualBinding.AsyncState;
            set => ActualBinding.AsyncState = value;
        }

        [DefaultValue(false)]
        public bool BindsDirectlyToSource
        {
            get => ActualBinding.BindsDirectlyToSource;
            set => ActualBinding.BindsDirectlyToSource = value;
        }

        [DefaultValue(null)]
        public IValueConverter Converter
        {
            get => ActualBinding.Converter;
            set => ActualBinding.Converter = value;
        }

        [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter)), DefaultValue(null)]
        public CultureInfo ConverterCulture
        {
            get => ActualBinding.ConverterCulture;
            set => ActualBinding.ConverterCulture = value;
        }

        [DefaultValue(null)]
        public object ConverterParameter
        {
            get => ActualBinding.ConverterParameter;
            set => ActualBinding.ConverterParameter = value;
        }

        [DefaultValue(null)]
        public string ElementName
        {
            get => ActualBinding.ElementName;
            set => ActualBinding.ElementName = value;
        }

        [DefaultValue(null)]
        public object FallbackValue
        {
            get => ActualBinding.FallbackValue;
            set => ActualBinding.FallbackValue = value;
        }

        [DefaultValue(false)]
        public bool IsAsync
        {
            get => ActualBinding.IsAsync;
            set => ActualBinding.IsAsync = value;
        }

        [DefaultValue(BindingMode.Default)]
        public BindingMode Mode
        {
            get => ActualBinding.Mode;
            set => ActualBinding.Mode = value;
        }

        [DefaultValue(false)]
        public bool NotifyOnSourceUpdated
        {
            get => ActualBinding.NotifyOnSourceUpdated;
            set => ActualBinding.NotifyOnSourceUpdated = value;
        }

        [DefaultValue(false)]
        public bool NotifyOnTargetUpdated
        {
            get => ActualBinding.NotifyOnTargetUpdated;
            set => ActualBinding.NotifyOnTargetUpdated = value;
        }

        [DefaultValue(false)]
        public bool NotifyOnValidationError
        {
            get => ActualBinding.NotifyOnValidationError;
            set => ActualBinding.NotifyOnValidationError = value;
        }

        [DefaultValue(null)]
        public PropertyPath Path
        {
            get => ActualBinding.Path;
            set => ActualBinding.Path = value;
        }

        [DefaultValue(null)]
        public RelativeSource RelativeSource
        {
            get => ActualBinding.RelativeSource;
            set => ActualBinding.RelativeSource = value;
        }

        [DefaultValue(null)]
        public object Source
        {
            get => ActualBinding.Source;
            set => ActualBinding.Source = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public UpdateSourceExceptionFilterCallback UpdateSourceExceptionFilter
        {
            get => ActualBinding.UpdateSourceExceptionFilter;
            set => ActualBinding.UpdateSourceExceptionFilter = value;
        }

        [DefaultValue(UpdateSourceTrigger.Default)]
        public UpdateSourceTrigger UpdateSourceTrigger
        {
            get => ActualBinding.UpdateSourceTrigger;
            set => ActualBinding.UpdateSourceTrigger = value;
        }

        [DefaultValue(false)]
        public bool ValidatesOnDataErrors
        {
            get => ActualBinding.ValidatesOnDataErrors;
            set => ActualBinding.ValidatesOnDataErrors = value;
        }

        [DefaultValue(false)]
        public bool ValidatesOnExceptions
        {
            get => ActualBinding.ValidatesOnExceptions;
            set => ActualBinding.ValidatesOnExceptions = value;
        }

        [DefaultValue(null)]
        public string XPath
        {
            get => ActualBinding.XPath;
            set => ActualBinding.XPath = value;
        }

        [DefaultValue(null)]
        public Collection<ValidationRule> ValidationRules
        {
            get => ActualBinding.ValidationRules;
        }
    }
}