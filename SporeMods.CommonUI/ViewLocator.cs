using System;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SporeMods.Core;
using SporeMods.CommonUI.Views;
using Avalonia;

namespace SporeMods.CommonUI
{
    public class ViewLocator : AvaloniaObject, IDataTemplate
    {
        public static readonly DirectProperty<ViewLocator, Func<object, IControl>> ManualOverrideViewProviderProperty =
            AvaloniaProperty.RegisterDirect<ViewLocator, Func<object, IControl>>(nameof(ManualOverrideViewProvider),
                vl => vl.ManualOverrideViewProvider, (vl, prov) => vl.ManualOverrideViewProvider = prov);

        Func<object, IControl> _manualOverrideViewProvider = null;
        public Func<object, IControl> ManualOverrideViewProvider
        {
            get => _manualOverrideViewProvider;
            set => SetAndRaise(ManualOverrideViewProviderProperty, ref _manualOverrideViewProvider, value);
        }
        
        protected bool DirectGetView(Func<object, IControl> manualProvider, object viewModel, out IControl view)
        {
            if (manualProvider != null)
            {
                var ovView = manualProvider(viewModel);
                if (ovView != null)
                {
                    view = ovView;
                    return true;
                }
            }

            if (viewModel is AppPath path)
            {
                view = new AppPathView();
                return true;
            }
            else if (viewModel is AppPathAutoDetectFail fail)
            {
                view = new AppPathAutoDetectFailView();
                return true;
            }
            else if (viewModel is BadPathEventArgs badPathArgs)
            {
                view = new TempFolderNotFoundView();
                return true;
            }

            view = null;
            return false;
        }

        public bool SupportsRecycling => false;

        public IControl Build(object data)
        {
            /*if (_manualOverrideViewProvider != null)
            {
                var manualRet = _manualOverrideViewProvider(data);
                if (manualRet != null)
                    return manualRet;
            }*/
            if (DirectGetView(ManualOverrideViewProvider, data, out IControl ovView))
            {
                ovView.DataContext = data;
                return ovView;
            }

            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            
            
            var type = Type.GetType(name);
            
            var current = Assembly.GetEntryAssembly();
            /*var assemblies = current.GetReferencedAssemblies().ToList(); //AppDomain.CurrentDomain.GetAssemblies();
            assemblies.Add(current.GetName());*/

            
            var modules = current.GetLoadedModules();
            //MessageDisplay.ShowMessageBox($"There are {modules.Length} modules loaded");
            
            foreach (Module module in modules)
            {
                var mType = module.Assembly.GetType(name, false, true);
                if (mType != null)
                {
                    type = mType;
                    break;
                }
            }

            /*asmName =>
            {
                foreach (AssemblyName name in assemblies)
                {
                    MessageDisplay.ShowMessageBox(name != null ? name.ToString() : "name == null");
                    
                    if (name.FullName.Equals(asmName.FullName, StringComparison.OrdinalIgnoreCase))
                        return Assembly.Load(name);
                }
                return null;
            },
            
            (asm, typeName, ignoreCase) => 
            {
                string content = asm != null ? "Assembly matched!" : "No match found :(";
                MessageDisplay.ShowMessageBox(content, "assembly matched? (DEBUG)");
                return asm.GetType(typeName, false, ignoreCase);
            }, false, true);*/


            object instance = null;

            if (type != null)
            {
                instance = Activator.CreateInstance(type);

                if (instance is IControl view)
                {
                    view.DataContext = data;
                    return view;
                }
            }
            

            string start = type == null ? $"View type not found: {name}.\n" : string.Empty;
            
            string ending = instance == null ? "null" : "not null";
            
            return new TextBlock()
            {
                Text = $"{start}Instance was {ending}.",
                DataContext = data
            };
        }

        public bool Match(object data)
        {
            return true;
        }
    }
}