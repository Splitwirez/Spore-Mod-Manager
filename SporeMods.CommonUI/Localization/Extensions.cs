using SporeMods.NotifyOnChange;
using SporeMods.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Reactive;
using Avalonia.ReactiveUI;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
//using Avalonia.Controls.ResourceNodeExtensions;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using System.Collections;


namespace SporeMods.CommonUI.Localization
{
    internal static class Extensions
    {
        public static T GetService<T>(this IServiceProvider sp) => (T)sp?.GetService(typeof(T));
        
        
        public static Uri GetContextBaseUri(this IServiceProvider ctx) => ctx.GetService<IUriContext>().BaseUri;

        public static T GetFirstParent<T>(this IServiceProvider ctx) where T : class 
            => ctx.GetService<IAvaloniaXamlIlParentStackProvider>().Parents.OfType<T>().FirstOrDefault();

        public static T GetLastParent<T>(this IServiceProvider ctx) where T : class 
            => ctx.GetService<IAvaloniaXamlIlParentStackProvider>().Parents.OfType<T>().LastOrDefault();

        public static IEnumerable<T> GetParents<T>(this IServiceProvider sp)
        {
            return sp.GetService<IAvaloniaXamlIlParentStackProvider>().Parents.OfType<T>();
            
            
        }

        public static Type ResolveType(this IServiceProvider ctx, string namespacePrefix, string type)
        {
            var tr = ctx.GetService<IXamlTypeResolver>();
            string name = string.IsNullOrEmpty(namespacePrefix) ? type : $"{namespacePrefix}:{type}";
            return tr?.Resolve(name);
        }
    }
}