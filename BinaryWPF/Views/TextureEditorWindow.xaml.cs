using BinaryWPF.ViewModels.Editors;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BinaryWPF.Views
{
    public partial class TextureEditorWindow : Window
    {
        private readonly System.Windows.Forms.PropertyGrid _propertyGrid;

        public System.Collections.Generic.IReadOnlyList<string> Commands =>
            DataContext is TextureEditorViewModel viewModel ? viewModel.Commands : System.Array.Empty<string>();

        public TextureEditorWindow(Nikki.Support.Shared.Class.TPKBlock tpk, string path)
        {
            InitializeComponent();

            DataContext = new TextureEditorViewModel(tpk, path);

            _propertyGrid = new System.Windows.Forms.PropertyGrid
            {
                HelpVisible = false
            };

            _propertyGrid.PropertyValueChanged += PropertyGrid_PropertyValueChanged;
            PropertyGridHost.Child = _propertyGrid;
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (DataContext is TextureEditorViewModel viewModel)
            {
                viewModel.SortByColumn(e.Column.DisplayIndex);
                e.Handled = true;
            }
        }

        private void PropertyGrid_PropertyValueChanged(object? sender, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            if (DataContext is TextureEditorViewModel viewModel)
            {
                var label = e.ChangedItem?.Label;
                if (string.IsNullOrEmpty(label))
                {
                    return;
                }

                string value = e.ChangedItem?.Value?.ToString() ?? string.Empty;
                viewModel.OnPropertyValueChanged(label, value);
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is TextureEditorViewModel viewModel)
            {
                _propertyGrid.SelectedObject = viewModel.SelectedTexture?.Texture;
            }
        }
    }
}
