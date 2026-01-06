using BinaryWPF.ViewModels.Editor;
using BinaryWPF.Services;

using System.ComponentModel;
using System.Windows;
using System.Windows.Forms.Integration;

namespace BinaryWPF.Views
{
    public partial class EditorWindow : Window
    {
        private readonly System.Windows.Forms.PropertyGrid _propertyGrid;

        public EditorWindow()
        {
            InitializeComponent();

            _propertyGrid = new System.Windows.Forms.PropertyGrid
            {
                HelpVisible = false
            };

            PropertyGridHost.Child = _propertyGrid;
            _propertyGrid.PropertyValueChanged += PropertyGrid_PropertyValueChanged;

            if (DataContext is EditorViewModel viewModel)
            {
                viewModel.Initialize();
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is EditorViewModel viewModel)
            {
                viewModel.SelectedNode = e.NewValue as EditorTreeNodeViewModel;
                _propertyGrid.SelectedObject = viewModel.SelectedObject;
            }
        }

        private void PropertyGrid_PropertyValueChanged(object? sender, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            if (DataContext is EditorViewModel viewModel)
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

        private void TreeView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is EditorViewModel viewModel)
            {
                viewModel.OpenEditorCommand.Execute(null);
            }
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.TreeViewItem item)
            {
                item.IsSelected = true;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DataContext is EditorViewModel viewModel && !viewModel.CanClose())
            {
                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }
    }
}
