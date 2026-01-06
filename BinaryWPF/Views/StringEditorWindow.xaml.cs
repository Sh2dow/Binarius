using BinaryWPF.ViewModels.Editors;

using System.Windows;
using System.Windows.Controls;

namespace BinaryWPF.Views
{
    public partial class StringEditorWindow : Window
    {
        public System.Collections.Generic.IReadOnlyList<string> Commands =>
            DataContext is StringEditorViewModel viewModel ? viewModel.Commands : System.Array.Empty<string>();

        public StringEditorWindow(Nikki.Support.Shared.Class.STRBlock str, string path)
        {
            InitializeComponent();
            DataContext = new StringEditorViewModel(str, path);
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (DataContext is StringEditorViewModel viewModel)
            {
                viewModel.SortByColumn(e.Column.DisplayIndex);
                e.Handled = true;
            }
        }
    }
}
