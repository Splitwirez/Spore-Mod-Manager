﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SporeMods.Core.Mods
{
	public class ModConfigurator1_0_x_xViewModel : ModalViewModel<bool>
	{
		ModIdentity _identity = null;
		public ModIdentity Identity
		{
			get => _identity;
			set
			{
				_identity = value;
				NotifyPropertyChanged();
			}
		}


		BaseModComponent _viewingComponent = null;
		public BaseModComponent ViewingComponent
		{
			get => _viewingComponent;
			set
			{
				Console.WriteLine("a");
				_viewingComponent = value;
				NotifyPropertyChanged();

				
				
				ViewingComponentContent.Clear();
				bool hasValue = value != null;
				
				
				ViewingComponentContent.Add(new StringCollection()
				{
					hasValue ? value.DisplayName : Identity.DisplayName,
					string.Empty
				});

				string desc = hasValue ? value.Description : Identity.Description;
				/*if (!hasValue)
				{
					ViewingComponentContent.Add(desc);
				}
				else if (
						(value is ModComponent modCmp) &&
						(modCmp.ImagePlacement != ImagePlacementType.None)
					)
				{
					object image = modCmp.Image;
					if (image != null)
					{
						if (modCmp.ImagePlacement != ImagePlacementType.After)
							ViewingComponentContent.Add(image);
						
						if (modCmp.ImagePlacement != ImagePlacementType.InsteadOf)
							ViewingComponentContent.Add(desc);
						
						if (modCmp.ImagePlacement == ImagePlacementType.After)
							ViewingComponentContent.Add(image);
					}
				}*/
				ViewingComponentContent.Add(new StringCollection()
				{
					desc
				});
				
				
				Console.WriteLine(hasValue ? $"type: {value.GetType().FullName}" : "null");
				Console.WriteLine($"count: {ViewingComponentContent.Count}");
			}
		}


		bool _configuring = false;
		public bool Configuring
		{
			get => _configuring;
			set
			{
				_configuring = value;
				NotifyPropertyChanged();
			}
		}


		ObservableCollection<object> _viewingComponentContent = new ObservableCollection<object>();
		public ObservableCollection<object> ViewingComponentContent
		{
			get => _viewingComponentContent;
			set
			{
				_viewingComponentContent = value;
				NotifyPropertyChanged();
			}
		}

		public ModConfigurator1_0_x_xViewModel(ModIdentity identity, bool configuring)
		{
			Identity = identity;
			ViewingComponent = identity;

			AcceptCommand = Externals.CreateCommand<object>(_ => CompletionSource.TrySetResult(true));
			ViewComponentCommand = Externals.CreateCommand<BaseModComponent>((p => ViewingComponent = p));
			
			Configuring = configuring;
			if (configuring)
				DismissCommand = Externals.CreateCommand<object>(_ => CompletionSource.TrySetResult(false));
			
			if (!identity.ParentMod.HasLogo)
				Title = identity.ParentMod.DisplayName;
		}

		object _acceptCommand = null;
		public object AcceptCommand
		{
			get => _acceptCommand;
			set
			{
				_acceptCommand = value;
				NotifyPropertyChanged();
			}
		}

		object _viewComponentCommand = null;
		public object ViewComponentCommand
		{
			get => _viewComponentCommand;
			set
			{
				_viewComponentCommand = value;
				NotifyPropertyChanged();
			}
		}

        public override string GetViewTypeName()
            => this.GetType().FullName.Replace("SporeMods.Core.Mods", "SporeMods.ViewModels").Replace("ViewModel", "View");

        public override string ToString()
			=> $"Configurator({Identity.DisplayName}, {Identity.Unique}) 1.0.x.x";
    }
}