using BinaryWPF.ViewModels.Editors;
using BinaryWPF.ViewModels.Editor;
using BinaryWPF.Services;

using System.Windows;

namespace BinaryWPF.Views
{
    public partial class CarPartsEditorWindow : Window
    {
        private readonly System.Windows.Forms.PropertyGrid _propertyGrid;

        public CarPartsEditorWindow(Nikki.Support.Shared.Class.DBModelPart model)
        {
            InitializeComponent();
            Title = $"Car Parts Editor : {model.CollectionName}";
            DataContext = new CarPartsEditorViewModel(model);

            _propertyGrid = new System.Windows.Forms.PropertyGrid
            {
                HelpVisible = false
            };
            new ThemeService().ApplyPropertyGridTheme(_propertyGrid);

            _propertyGrid.PropertyValueChanged += PropertyGrid_PropertyValueChanged;
            PropertyGridHost.Child = _propertyGrid;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is CarPartsEditorViewModel viewModel)
            {
                viewModel.SelectedNode = e.NewValue as EditorTreeNodeViewModel;
                _propertyGrid.SelectedObject = viewModel.SelectedObject;
            }
        }

        private void PropertyGrid_PropertyValueChanged(object? sender, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            if (DataContext is CarPartsEditorViewModel viewModel)
            {
                viewModel.OnPropertyValueChanged();
                _propertyGrid.SelectedObject = viewModel.SelectedObject;
            }
        }
    }
}
