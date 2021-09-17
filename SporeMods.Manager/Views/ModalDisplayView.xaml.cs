using System;
using System.Windows;
using System.Windows.Controls;
using SporeMods.CommonUI;

namespace SporeMods.Views
{
    /// <summary>
    /// Interaction logic for ModalDisplayView.xaml
    /// </summary>
    public partial class ModalDisplayView : AnimatableContentControl
    {
        static ModalDisplayView()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(ModalDisplayView), new FrameworkPropertyMetadata(typeof(AnimatableContentControl)));
            /*FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
            {
                DefaultValue = Application.Current.FindResource(typeof(Window))
            });*/
        }

		public ModalDisplayView()
        {
            InitializeComponent();
        }
	}
}
