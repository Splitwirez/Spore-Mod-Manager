using Mechanism.Wpf.Core;
using Mechanism.Wpf.Core.Behaviors;
using Mechanism.Wpf.Core.Windows;
using Mechanism.Wpf.Styles.Shale;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using MessageBox = Mechanism.Wpf.Core.Windows.MessageBox;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Mechanism.Wpf.Showcase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DecoratableWindow
    {
        enum TestEnum
        {
            OK,
            No_u,
            Cancel
        }

        //ShaleAccent _accent = ShaleAccents.Sky; //new ShaleAccent(183, 28);
        //ResourceDictionary _dictionary = null;

        public ObservableCollection<BreadcrumbDemo> SampleCrumbs { get; set; } = new ObservableCollection<BreadcrumbDemo>();

        public void AddSampleCrumbs()
        {
            SampleCrumbs.Add(new BreadcrumbDemo("Organism")
            {
                SubItems = new ObservableCollection<BreadcrumbDemo>()
                {
                    new BreadcrumbDemo("Animal")
                    {
                        SubItems = new ObservableCollection<BreadcrumbDemo>()
                        {
                            new BreadcrumbDemo("Mammal")
                            {
                                SubItems = new ObservableCollection<BreadcrumbDemo>()
                                {
                                    new BreadcrumbDemo("Moose")
                                }
                            },
                            new BreadcrumbDemo("Bird")
                            {
                                SubItems = new ObservableCollection<BreadcrumbDemo>()
                                {
                                    new BreadcrumbDemo("Goose")
                                }
                            }
                        }
                    },
                    new BreadcrumbDemo("Plant")
                    {
                        SubItems = new ObservableCollection<BreadcrumbDemo>()
                        {
                            new BreadcrumbDemo("Tree")
                            {
                                SubItems = new ObservableCollection<BreadcrumbDemo>()
                                {
                                    new BreadcrumbDemo("Maple")
                                }
                            },
                            new BreadcrumbDemo("Flowering Plant")
                            {
                                SubItems = new ObservableCollection<BreadcrumbDemo>()
                                {
                                    new BreadcrumbDemo("Poison Ivy")
                                }
                            }
                        }
                    },
                }
            });
        }

        public MainWindow()
        {
            AddSampleCrumbs();
            InitializeComponent();
            Timer timer = new Timer(5000);
            timer.Elapsed += (sneder, args) =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (IsVisible)
                        Hide();
                    else
                        Show();
                }));
            };


            TimeDurationTextBox.Text = ScrollAnimationBehavior.GetTimeDuration(SmoothScrollTestListView).ToString();

            //ShaleHueSlider.Value = _accent.Hue;
            //ShaleSaturationSlider.Value = _accent.Saturation;

            //Application.Current.Resources.MergedDictionaries.Add(_accent.Dictionary);

            ShaleHueSlider.ValueChanged += ShaleSliders_ValueChanged;
            ShaleSaturationSlider.ValueChanged += ShaleSliders_ValueChanged;
        }

        private void Sliders_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Mouse up");
            CalculateColor();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TimeDurationTextBox.TextChanged += TimeDurationTextBox_TextChanged;
            AnimationTypeComboBox.SelectionChanged += ScrollAnimationComboBox_SelectionChanged;
            EasingModeComboBox.SelectionChanged += ScrollAnimationComboBox_SelectionChanged;
            EnableSmoothScrollingCheckBox.Checked += EnableSmoothScrollingCheckBox_Checked;
            EnableSmoothScrollingCheckBox.Unchecked += EnableSmoothScrollingCheckBox_Checked;
            LightsToggleSwitch.Checked += LightsToggleSwitch_Checked;
            LightsToggleSwitch.Unchecked += LightsToggleSwitch_Unchecked;

            //ColouresPresetsGrid.Children.Add(GetColoureButton(ShaleAccents.Sky));
            //ColouresPresetsGrid.Children.Add(GetColoureButton(ShaleAccents.Sand));

            SkinsComboBox.SelectionChanged += SkinsComboBox_SelectionChanged;
        }

        private Button GetColoureButton(/*ShaleAccent*/object accent)
        {
            //Debug.WriteLine("PRESET COLOUR: " + accent.Brush.Color.ToString());

            Button button = new Button()
            {
                //Background = accent.Brush//new SolidColorBrush(accent.Color)
            };
            /*

            button.SetBinding(Button.HeightProperty, new Binding()
            {
                Source = button,
                Path = new PropertyPath("ActualWidth"),
                FallbackValue = 10.0
            });

            button.Click += (sneder, args) =>
            {
                ShaleHueSlider.Value = accent.Hue;
                ShaleSaturationSlider.Value = accent.Saturation;
            };
            */
            return button;
        }

        public void CalculateColor()
        {
            /*_accent.Hue = ShaleHueSlider.Value;
            _accent.Saturation = ShaleSaturationSlider.Value;*/
        }

        private void CycleCompositionStateButton_Click(object sender, RoutedEventArgs e)
        {
            if (CompositionState == WindowCompositionState.Alpha)
                CompositionState = WindowCompositionState.Glass;
            else if (CompositionState == WindowCompositionState.Glass)
                CompositionState = WindowCompositionState.Accent;
            else if (CompositionState == WindowCompositionState.Accent)
                CompositionState = WindowCompositionState.Acrylic;
            else
                CompositionState = WindowCompositionState.Alpha;

            //CurrentCompositionStateTextBlock.Text = CompositionState.ToString();
        }

        private void ScrollAnimationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EasingFunctionBase ease = null;
            if (AnimationTypeComboBox.SelectedIndex != 0)
            {
                Type easeType = /*(*/AnimationTypeComboBox.SelectedItem/* as ComboBoxItem).Content*/.GetType();
                ease = (EasingFunctionBase)Activator.CreateInstance(easeType);
                ease.EasingMode = (EasingMode)Enum.Parse(typeof(EasingMode), ((ComboBoxItem)EasingModeComboBox.SelectedItem).Content.ToString());
            }

            ScrollAnimationBehavior.SetEasingFunction(SmoothScrollTestListView, ease);

            Debug.WriteLine("ScrollAnimationComboBox_SelectionChanged");
        }

        private void TimeDurationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TimeSpan.TryParse(TimeDurationTextBox.Text, out TimeSpan resultTime))
            {
                ScrollAnimationBehavior.SetTimeDuration(SmoothScrollTestListView, resultTime);
                TimeDurationBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
                Debug.WriteLine("TimeDurationTextBox_TextChanged: " + true);
            }
            else
            {
                TimeDurationBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                Debug.WriteLine("TimeDurationTextBox_TextChanged: " + false);
            }
        }

        private void EnableSmoothScrollingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool enable = EnableSmoothScrollingCheckBox.IsChecked == true;
            ScrollAnimationBehavior.SetIsEnabled(SmoothScrollTestListView, enable);
            Debug.WriteLine("EnableSmoothScrollingCheckBox_Checked: " + enable);
        }


        private void ShaleSliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SkinsComboBox.SelectedIndex == 0)
                CalculateColor();
        }

        private void LightsToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (SkinsComboBox.SelectedIndex == 0)
                Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Mechanism.Wpf.Styles.Shale;component/Themes/Colors/BaseLight.xaml", UriKind.Relative);
            else if (SkinsComboBox.SelectedIndex == 1)
                Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Mechanism.Wpf.Styles.Plex;component/Themes/Colors/LightBlue.xaml", UriKind.Relative);
        }

        private void LightsToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            if (SkinsComboBox.SelectedIndex == 0)
                Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Mechanism.Wpf.Styles.Shale;component/Themes/Colors/BaseDark.xaml", UriKind.Relative);
            else if (SkinsComboBox.SelectedIndex == 1)
                Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Mechanism.Wpf.Styles.Plex;component/Themes/Colors/DarkBlue.xaml", UriKind.Relative);
        }

        private void SkinsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearValue(TitlebarHeightProperty);
            MainTabControl.Margin = new Thickness(0);
            MainTabControl.ClearValue(TabControl.BackgroundProperty);

            if (SkinsComboBox.SelectedIndex == 0)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri("/Mechanism.Wpf.Styles.Shale;component/Themes/Shale.xaml", UriKind.Relative);
                TitlebarHeight = 61;
            }
            if (SkinsComboBox.SelectedIndex == 1)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri("/Mechanism.Wpf.Styles.Plex;component/Themes/Plex.xaml", UriKind.Relative);
            }
            else if (SkinsComboBox.SelectedIndex == 2)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri("/Mechanism.Wpf.Styles.EDNA;component/Themes/EDNA.xaml", UriKind.Relative);
                MainTabControl.Margin = new Thickness(15);
            }
            else if (SkinsComboBox.SelectedIndex == 4)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri("/PresentationFramework.Classic, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL;component/themes/classic.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Mechanism.Wpf.Core;component/Themes/Generic.xaml", UriKind.Relative);
                MainTabControl.Background = SystemColors.ActiveBorderBrush;
            }
            else if (SkinsComboBox.SelectedIndex == 5)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri("/PresentationFramework.Luna, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL;component/themes/luna.normalcolor.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Mechanism.Wpf.Core;component/Themes/Luna.NormalColor.xaml", UriKind.Relative);
            }
            else if (SkinsComboBox.SelectedIndex == 6)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri("/PresentationFramework.Luna, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL;component/themes/luna.homestead.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Mechanism.Wpf.Core;component/Themes/Luna.Homestead.xaml", UriKind.Relative);
            }
            else if (SkinsComboBox.SelectedIndex == 7)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri("/PresentationFramework.Luna, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL;component/themes/luna.metallic.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Mechanism.Wpf.Core;component/Themes/Luna.Metallic.xaml", UriKind.Relative);
            }
            else if (SkinsComboBox.SelectedIndex == 7)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri("/PresentationFramework.Luna, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL;component/themes/luna.metallic.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Mechanism.Wpf.Core;component/Themes/Luna.Metallic.xaml", UriKind.Relative);
            }
            else if (SkinsComboBox.SelectedIndex == 8)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri("/PresentationFramework.Royale, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL;component/themes/royale.normalcolor.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Mechanism.Wpf.Core;component/Themes/Royale.NormalColor.xaml", UriKind.Relative);
            }
            else if (SkinsComboBox.SelectedIndex == 9)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri("/PresentationFramework.Aero, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL;component/themes/aero.normalcolor.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Mechanism.Wpf.Core;component/Themes/Aero.NormalColor.xaml", UriKind.Relative);
            }
            else if (SkinsComboBox.SelectedIndex == 10)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri("/PresentationFramework.Aero2, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL;component/themes/aero2.normalcolor.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Mechanism.Wpf.Core;component/Themes/Aero2.NormalColor.xaml", UriKind.Relative);
            }
            else if (SkinsComboBox.SelectedIndex == 11)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri("/PresentationFramework.AeroLite, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL;component/themes/aerolite.normalcolor.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries[1].Source = new Uri("/Mechanism.Wpf.Core;component/Themes/Generic.xaml", UriKind.Relative);
            }

            if ((SkinsComboBox.SelectedIndex >= 13) && (SkinsComboBox.SelectedItem is ComboBoxItem item) && (item.Tag is string uri))
            {
                string[] uris = uri.Split("?".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < uris.Length; i++)
                {
                    if (i < Application.Current.Resources.MergedDictionaries.Count)
                        Application.Current.Resources.MergedDictionaries[i].Source = new Uri(uris[i], UriKind.RelativeOrAbsolute);
                    else
                        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                        {
                            Source = new Uri(uris[i], UriKind.RelativeOrAbsolute)
                        });
                }
            }

            //Enable/disable Reveal test tab as needed
            if (SkinsComboBox.SelectedIndex == 1)
                RevealTestTabItem.IsEnabled = true;
            else
                RevealTestTabItem.IsEnabled = false;

            if (SkinsComboBox.SelectedIndex != 2)
                MainTabControl.ClearValue(TabControl.MarginProperty);

            if (LightsToggleSwitch.IsChecked.Value)
                LightsToggleSwitch_Checked(LightsToggleSwitch, null);
            else
                LightsToggleSwitch_Unchecked(LightsToggleSwitch, null);
        }

        private void MessageBoxTestButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxButtons.IgnoreRetryAbortButtons b = (MessageBoxButtons.IgnoreRetryAbortButtons)MessageBox<IgnoreRetryAbortActionSet>.Show("Here, have some actions to choose from.", "This is a MessageBox", (Rectangle)Resources["SampleIcon"]);

            MessageBox.Show("Result of previous MessageBox: " + b.ToString(), "haha yes");

            MessageBox<SampleActionSet>.Show("*laughs in custom MessageBox actions*", "I CAN DO ANYTHING", (Rectangle)Resources["SampleIcon"]);
        }

        private void LoadExternalThemeButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog browse = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = ".NET Assembly (*.dll, *.exe)|*.dll;*.exe",
                CheckFileExists = true
            };
            if ((browse.ShowDialog() == true) && (!string.IsNullOrWhiteSpace(browse.FileName)))
            {
                Assembly skin = Assembly.LoadFile(browse.FileName);
                string tagValue = string.Empty;
                foreach (string s in ExternalThemeUriTextBox.Text.Split("?".ToCharArray()))
                    tagValue += @"pack://application:,,,/" + System.IO.Path.GetFileNameWithoutExtension(browse.FileName) + @";component/" + s + "?";

                SkinsComboBox.Items.Add(new ComboBoxItem()
                {
                    Content = System.IO.Path.GetFileName(browse.FileName),
                    Tag = tagValue
                });
            }
        }

        private void BreadcrumbsBar_PathUpdated(object sender, Mechanism.Wpf.Core.Breadcrumbs.PathChangedEventArgs e)
        {
            BreadcrumbsPathTextBlock.Text = e.NewPath;
        }

        private void BreadcrumbsBar_PathItemAdded(object sender, Mechanism.Wpf.Core.Breadcrumbs.PathItemAddedEventArgs e)
        {
            SampleCrumbs.Insert(e.Index, e.NewValue as BreadcrumbDemo);
        }
    }

    public enum SampleButtons
    {
        Yeet,
        NoU
    }

    public class SampleActionSet : IMessageBoxActionSet
    {
        public string GetDisplayName(object value)
        {
            switch ((SampleButtons)value)
            {
                case SampleButtons.Yeet:
                    return "Yeet";
                case SampleButtons.NoU:
                    return "no u";
                default:
                    return string.Empty;
            }
            /*if ((SampleButtons)value == SampleButtons.Yeet)
                return "Yeet";
            else if ((SampleButtons)value == SampleButtons.Yeet)
                return value.ToString();*/
        }

        public IEnumerable<object> Actions
        {
            get
            {
                object[] objects = new object[Enum.GetNames(typeof(SampleButtons)).Count()];
                Enum.GetValues(typeof(SampleButtons)).CopyTo(objects, 0);
                return objects.ToList();
            }
        }
    }

    public class BreadcrumbDemo : System.ComponentModel.INotifyPropertyChanged
    {
        string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        ObservableCollection<BreadcrumbDemo> _subItems = new ObservableCollection<BreadcrumbDemo>();
        public ObservableCollection<BreadcrumbDemo> SubItems
        {
            get => _subItems;
            set
            {
                _subItems = value;
                NotifyPropertyChanged(nameof(SubItems));
            }
        }

        public BreadcrumbDemo(string name)
        {
            Name = name;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return Name;
        }
    }
}
