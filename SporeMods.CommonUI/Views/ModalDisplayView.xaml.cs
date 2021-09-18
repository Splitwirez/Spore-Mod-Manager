using System;
using System.Windows;
using System.Windows.Controls;
using SporeMods.CommonUI;
using SporeMods.Core;

namespace SporeMods.Views
{
    /// <summary>
    /// Interaction logic for ModalDisplayView.xaml
    /// </summary>
    public partial class ModalDisplayView : AnimatableContentControl
    {
		public ModalDisplayView()
        {
            InitializeComponent();
        }

        void ModalDisplayView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool newIsVisible)
            {
                if (newIsVisible)
                    Modal.PreventProceed();
                else
                    Modal.PermitProceed();
            }
        }
	}
}
