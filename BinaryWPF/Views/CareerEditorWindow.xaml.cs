using BinaryWPF.ViewModels.Editors;
using BinaryWPF.ViewModels.Editor;

using System.Windows;

namespace BinaryWPF.Views
{
    public partial class CareerEditorWindow : Window
    {
        private readonly System.Windows.Forms.PropertyGrid _propertyGrid;

        public System.Collections.Generic.IReadOnlyList<string> Commands =>
            DataContext is CareerEditorViewModel viewModel ? viewModel.Commands : System.Array.Empty<string>();

        public CareerEditorWindow(Nikki.Support.Shared.Class.GCareer career, string path)
        {
            InitializeComponent();
            Title = $"Career Editor : {career.CollectionName}";
            DataContext = new CareerEditorViewModel(career, path);

            _propertyGrid = new System.Windows.Forms.PropertyGrid
            {
                HelpVisible = false
            };

            _propertyGrid.PropertyValueChanged += PropertyGrid_PropertyValueChanged;
            PropertyGridHost.Child = _propertyGrid;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is CareerEditorViewModel viewModel)
            {
                viewModel.SelectedNode = e.NewValue as EditorTreeNodeViewModel;
                _propertyGrid.SelectedObject = viewModel.SelectedObject;
            }
        }

        private void PropertyGrid_PropertyValueChanged(object? sender, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            if (DataContext is CareerEditorViewModel viewModel)
            {
                var label = e.ChangedItem?.Label;
                if (string.IsNullOrEmpty(label))
                {
                    return;
                }

                string value = e.ChangedItem?.Value?.ToString() ?? string.Empty;
                viewModel.OnPropertyValueChanged(label, value);
                _propertyGrid.SelectedObject = viewModel.SelectedObject;
            }
        }
    }
}
