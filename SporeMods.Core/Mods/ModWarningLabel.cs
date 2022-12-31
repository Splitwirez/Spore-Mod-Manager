using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SporeMods.Core.Mods
{
    public class ModWarningLabel : NotifyPropertyChangedBase
    {
        public enum WarningLabelSeverity
        {
            Positive,
            Neutral,
            Caution,
            Warning,
            Danger,
        }
        public enum WarningLabelType
        {
            /// <summary>
            /// Whether or not this is an experimental mod release (may have unforeseen pitfalls, bugs, etc.)
            /// </summary>
            IsExperimental,
            /// <summary>
            /// Whether or not playing with this mod will cause save data corruption or other undesirable effects if it is subsequently uninstalled
            /// </summary>
            CausesSaveDataDependency,
            /// <summary>
            /// Whether or not this mod requires a Galaxy reset in order to work correctly
            /// </summary>
            RequiresGalaxyReset,
            /// <summary>
            /// Whether or not this mod requires custom code injection in order to work correctly
            /// </summary>
            UsesCodeInjection,
            /// <summary>
            /// True only if use of this mod will NEVER result in player creations which require the mod to be installed in order to work correctly
            /// </summary>
            GuaranteedVanillaCompatible,
            /// <summary>
            /// Whether or not this mod is known to frequently cause problems, yet fails to disclose them adequately via standard warnings. The value of this property is dictated not by the mod, but by the SMM itself, to warn users of problems that weren't known until it was too late.
            /// It is not our place to unconditionally forbid the user from installing these mods. No matter how heavily the SMM ends up having to discourage them from doing so, the user must always be able to override our judgement if they truly see fit.
            /// The SMM is not a babysitter to the user, but an advisor and assistant - that is the baseline of respect we, as developers, owe our users.
            /// </summary>
            KnownHazardousMod,
        }
        WarningLabelType _labelType = WarningLabelType.UsesCodeInjection;
        public WarningLabelType LabelType
        {
            get => _labelType;
            internal set
            {
                _labelType = value;
                NotifyPropertyChanged();
            }
        }

        WarningLabelSeverity _severity = WarningLabelSeverity.Danger;
        public WarningLabelSeverity Severity
        {
            get => _severity;
            internal set
            {
                _severity = value;
                NotifyPropertyChanged();
            }
        }
        public string Header
        {
            get => Externals.GetLocalizedText($"Mods!Warning!Label!{LabelType}!Header");
        }
        private ModWarningLabel(WarningLabelType labelType)
            : this()
        {
            LabelType = labelType;
            Severity = _TEMP_SEVERITY_MAP[labelType];
        }
        private ModWarningLabel()
            : base()
        {
            Externals.LanguageChanged += (s, e) =>
            {
                NotifyPropertyChanged(nameof(Header));
            };
        }

        public static readonly IReadOnlyDictionary<WarningLabelType, ModWarningLabel> Labels;
        static readonly IReadOnlyDictionary<WarningLabelType, WarningLabelSeverity> _TEMP_SEVERITY_MAP = new ReadOnlyDictionary<WarningLabelType, WarningLabelSeverity>(
            new Dictionary<WarningLabelType, WarningLabelSeverity>()
            {
                { WarningLabelType.IsExperimental, WarningLabelSeverity.Neutral },
                { WarningLabelType.RequiresGalaxyReset, WarningLabelSeverity.Caution },
                { WarningLabelType.CausesSaveDataDependency, WarningLabelSeverity.Warning },
                { WarningLabelType.UsesCodeInjection, WarningLabelSeverity.Neutral },
                { WarningLabelType.GuaranteedVanillaCompatible, WarningLabelSeverity.Positive },
                { WarningLabelType.KnownHazardousMod, WarningLabelSeverity.Danger },
            }
        );
        /*new Func<IReadOnlyDictionary<WarningLabelType, WarningLabelSeverity>(() =>
        {
        });*/
        static ModWarningLabel()
        {
            Dictionary<WarningLabelType, ModWarningLabel> labels = new Dictionary<WarningLabelType, ModWarningLabel>();
            var labelTypes = Enum.GetValues(typeof(WarningLabelType))
                .Cast<WarningLabelType>()
            ;
            foreach (WarningLabelType labelType in labelTypes)
            {
                labels.Add(labelType, new ModWarningLabel(labelType));
            }

            Labels = new ReadOnlyDictionary<WarningLabelType, ModWarningLabel>(labels);
            
            if (labelTypes.Count() != _TEMP_SEVERITY_MAP.Keys.Count()) //labelTypes.Any(x => !_TEMP_SEVERITY_MAP.ContainsKey(x)))
                throw new KeyNotFoundException("_TEMP_SEVERITY_MAP");
        }
    }

    public class ModWarningLabels : NotifyPropertyChangedBase
    {

        bool _isExperimental = false;
        public bool IsExperimental
        {
            get => _isExperimental;
            internal set
            {
                _isExperimental = value;
                NotifyPropertyChanged();
                WhenWarningPropertyChanged(value);
            }
        }

        bool _causesSaveDataDependency = false;
        public bool CausesSaveDataDependency
        {
            get => _causesSaveDataDependency;
            internal set
            {
                _causesSaveDataDependency = value;
                NotifyPropertyChanged();
                WhenWarningPropertyChanged(value);
            }
        }

        bool _requiresGalaxyReset = false;
        public bool RequiresGalaxyReset
        {
            get => _requiresGalaxyReset;
            internal set
            {
                _requiresGalaxyReset = value;
                NotifyPropertyChanged();
                WhenWarningPropertyChanged(value);
            }
        }


        bool _usesCodeInjection = false;
        public bool UsesCodeInjection
        {
            get => _usesCodeInjection;
            internal set
            {
                _usesCodeInjection = value;
                NotifyPropertyChanged();
                WhenWarningPropertyChanged(value);
            }
        }

        bool _guaranteedVanillaCompatible = false;
        public bool GuaranteedVanillaCompatible
        {
            get => _guaranteedVanillaCompatible;
            internal set
            {
                _guaranteedVanillaCompatible = value;
                NotifyPropertyChanged();
                WhenWarningPropertyChanged(value);
            }
        }

        bool _knownHazardousMod = false;
        public bool KnownHazardousMod
        {
            get => _knownHazardousMod;
            internal set
            {
                _knownHazardousMod = value;
                NotifyPropertyChanged();
                WhenWarningPropertyChanged(value);
            }
        }



        ObservableCollection<ModWarningLabel> _labels = new ObservableCollection<ModWarningLabel>();
        public ObservableCollection<ModWarningLabel> Labels
        {
            get => _labels;
        }

        void WhenWarningPropertyChanged(bool value, [CallerMemberName] string propertyName = "")
        {
            Cmd.WriteLine($"WhenWarningPropertyChanged: {propertyName}={value}");
            if (!Enum.TryParse<ModWarningLabel.WarningLabelType>(propertyName, out ModWarningLabel.WarningLabelType labelType))
                return;
            Cmd.WriteLine("Parsed!");

            ModWarningLabel label = ModWarningLabel.Labels[labelType];
            bool containsLabel = Labels.Contains(label);
            if (value != containsLabel)
            {
                if (value)
                    Labels.Add(label);
                else
                    Labels.Remove(label);
            }
        }
    }
}
