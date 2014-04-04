using System;
using System.Windows;
using AddMaterials.ViewModel;
using AddMaterials.ViewModel.Events;

namespace AddMaterials.View
{
    /// <summary>
    /// Interaction logic for MaterialsView.xaml
    /// </summary>
    public partial class MaterialsView
    {
        public MaterialsView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var materialBrowser = e.NewValue as MaterialBrowserViewModel;
            if (materialBrowser != null)
                materialBrowser.RequestClose += OnRequestClose;
        }

        private void OnRequestClose(object sender, DialogCloseEventArgs e)
        {
            try
            {
                DialogResult = e.Result;
            }
            catch (InvalidOperationException)
            {
                Close();
            }
        }
    }
}
