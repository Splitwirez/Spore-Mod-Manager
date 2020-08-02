using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SporeMods.Core.ModIdentity
{
    public abstract class ModComponent : INotifyPropertyChanged
    {

        string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                NotifyPropertyChanged(nameof(DisplayName));
            }
        }

        string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                NotifyPropertyChanged(nameof(Description));
            }
        }

        string _imagePlacement = "None";
        public string ImagePlacement
        {
            get => _imagePlacement;
            set
            {
                _imagePlacement = value;
                NotifyPropertyChanged(nameof(ImagePlacement));
            }
        }


        ObservableCollection<ModComponent> _subComponents = new ObservableCollection<ModComponent>();
        public ObservableCollection<ModComponent> SubComponents
        {
            get => _subComponents;
            set
            {
                _subComponents = value;
                NotifyPropertyChanged(nameof(SubComponents));
            }
        }

        bool _isGroup = false;
        public bool IsGroup
        {
            get => _isGroup;
            set
            {
                _isGroup = value;
                NotifyPropertyChanged(nameof(IsGroup));
            }
        }

        bool _isInGroup = false;
        public bool IsInGroup
        {
            get => _isInGroup;
            set
            {
                _isInGroup = value;
                NotifyPropertyChanged(nameof(IsInGroup));
            }
        }

        string _groupUnique = string.Empty;
        public string GroupUnique
        {
            get => _groupUnique;
            set
            {
                _groupUnique = value;
                NotifyPropertyChanged(nameof(GroupUnique));
            }
        }

        string _groupDisplayName = string.Empty;
        public string GroupDisplayName
        {
            get => _groupDisplayName;
            set
            {
                _groupDisplayName = value;
                NotifyPropertyChanged(nameof(GroupDisplayName));
            }
        }

        bool _isEnabled = false;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }

        public string Unique = string.Empty;
        public string[] FileNames = new string[0];
        public string[] FileGames = new string[0];

        public string[] CompatFileNames = new string[0];
        public string[] CompatFileGames = new string[0];

        public ModComponent(XElement node)
        {
            var displayAttr = ((XElement)node).Attribute("displayName");
            if (displayAttr != null)
                DisplayName = displayAttr.Value;

            var uniqueAttr = ((XElement)node).Attribute("unique");
            if (uniqueAttr != null)
                Unique = uniqueAttr.Value;


            if (node.Name.LocalName.ToLowerInvariant() == "componentgroup")
            {
                IsGroup = true;
                var groupDisplayAttr = node.Attribute("displayName");
                if (groupDisplayAttr != null)
                    GroupDisplayName = groupDisplayAttr.Value;

                foreach (XElement subNode in ((XElement)node).Elements())
                {
                    ModComponent subComponent = (ModComponent)Activator.CreateInstance(this.GetType(), subNode);
                    subComponent.IsInGroup = true;
                    subComponent.GroupUnique = this.Unique;
                    SubComponents.Add(subComponent);
                }
            }
            else
            {
                if (!(string.IsNullOrWhiteSpace(((XElement)node).Value)))
                    FileNames = ((XElement)node).Value.Split('?');

                var descAttr = ((XElement)node).Attribute("description");
                if (descAttr != null)
                    Description = descAttr.Value;

                var gameAttr = ((XElement)node).Attribute("game");
                if (gameAttr != null)
                    FileGames = gameAttr.Value.Split('?');
                else
                {
                    FileGames = new string[FileNames.Count()];

                    for (int i = 0; i < FileNames.Count(); i++)
                        FileGames[i] = ComponentGameDir.modapi.ToString();
                }

                var defaultCheckedAttr = ((XElement)node).Attribute("defaultChecked");
                if ((defaultCheckedAttr != null) && (bool.TryParse(defaultCheckedAttr.Value, out bool isEnabled)))
                    IsEnabled = isEnabled;
                else
                    IsEnabled = false;
            }
            var imagePlacement = ((XElement)node).Attribute("imagePlacement");
            if (imagePlacement != null)
                ImagePlacement = imagePlacement.Value;
            //ImageName
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ModComponentV1_0_0_0 : ModComponent
    {
        public ModComponentV1_0_0_0(XElement node) : base(node)
        {
            /*string output = string.Empty;
            for (int i = 0; i < FileNames.Length; i++)
                output += ", " + FileGames[i] + "!" + FileNames[i];
            ModInstallation.DebugMessageBoxShow(output);*/
        }
    }
}
