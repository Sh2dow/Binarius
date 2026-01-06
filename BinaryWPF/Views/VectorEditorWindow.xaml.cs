using BinaryWPF.ViewModels.Editors;
using BinaryWPF.ViewModels.Editor;
using BinaryWPF.Services;

using System.Windows;

namespace BinaryWPF.Views
{
    public partial class VectorEditorWindow : Window
    {
        private readonly System.Windows.Forms.PropertyGrid _propertyGrid;

        public VectorEditorWindow(Nikki.Support.Shared.Class.VectorVinyl vinyl)
        {
            InitializeComponent();
            Title = $"Vector Editor : {vinyl.CollectionName}";
            DataContext = new VectorEditorViewModel(vinyl);

            _propertyGrid = new System.Windows.Forms.PropertyGrid
            {
                HelpVisible = false
            };
            new ThemeService().ApplyPropertyGridTheme(_propertyGrid);

            PropertyGridHost.Child = _propertyGrid;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is VectorEditorViewModel viewModel)
            {
                viewModel.SelectedNode = e.NewValue as EditorTreeNodeViewModel;
                _propertyGrid.SelectedObject = viewModel.SelectedObject;
            }
        }
    }
}
