using BinaryWPF.Services;
using BinaryWPF.Views.UI;

using CoreExtensions.Management;

using Endscript.Enums;

using Nikki.Support.Shared.Class;
using Nikki.Utils;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace BinaryWPF.ViewModels.Editors
{
    public sealed class StringEditorViewModel : ViewModelBase
    {
        private const string KeyProperty = "Key";
        private const string LabelProperty = "Label";
        private const string TextProperty = "Text";

        private readonly STRBlock _str;
        private readonly string _strPath;
        private readonly HashSet<string> _modified;
        private readonly IWindowService _windowService;
        private readonly IPromptService _promptService;

        private StringRecordItemViewModel? _selectedRecord;
        private string _searchKey = string.Empty;
        private string _searchLabel = string.Empty;
        private string _searchText = string.Empty;
        private string _editorText = string.Empty;

        public ObservableCollection<StringRecordItemViewModel> Records { get; } = new();
        public List<string> Commands { get; } = new();

        public StringRecordItemViewModel? SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                if (SetField(ref _selectedRecord, value))
                {
                    EditorText = value?.Text ?? string.Empty;
                }
            }
        }

        public string SearchKey
        {
            get => _searchKey;
            set
            {
                if (SetField(ref _searchKey, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _searchLabel = string.Empty;
                        _searchText = string.Empty;
                        OnPropertyChanged(nameof(SearchLabel));
                        OnPropertyChanged(nameof(SearchText));
                    }
                    ApplySearch();
                }
            }
        }

        public string SearchLabel
        {
            get => _searchLabel;
            set
            {
                if (SetField(ref _searchLabel, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _searchKey = string.Empty;
                        _searchText = string.Empty;
                        OnPropertyChanged(nameof(SearchKey));
                        OnPropertyChanged(nameof(SearchText));
                    }
                    ApplySearch();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _searchKey = string.Empty;
                        _searchLabel = string.Empty;
                        OnPropertyChanged(nameof(SearchKey));
                        OnPropertyChanged(nameof(SearchLabel));
                    }
                    ApplySearch();
                }
            }
        }

        public string EditorText
        {
            get => _editorText;
            set
            {
                if (SetField(ref _editorText, value))
                {
                    UpdateSelectedText(value);
                }
            }
        }

        public RelayCommand AddStringCommand { get; }
        public RelayCommand RemoveStringCommand { get; }
        public RelayCommand EditStringCommand { get; }
        public RelayCommand ReplaceCommand { get; }
        public RelayCommand FindPreviousCommand { get; }
        public RelayCommand FindNextCommand { get; }
        public RelayCommand HasherCommand { get; }
        public RelayCommand RaiderCommand { get; }

        public StringEditorViewModel(STRBlock str, string path)
        {
            _str = str;
            _strPath = path;
            _modified = new HashSet<string>();
            _windowService = new WindowService();
            _promptService = new PromptService();

            AddStringCommand = new RelayCommand(AddString);
            RemoveStringCommand = new RelayCommand(RemoveString, () => SelectedRecord != null);
            EditStringCommand = new RelayCommand(EditString, () => SelectedRecord != null);
            ReplaceCommand = new RelayCommand(ReplaceAll, () => Records.Count > 0);
            FindPreviousCommand = new RelayCommand(FindPrevious, () => Records.Count > 0);
            FindNextCommand = new RelayCommand(FindNext, () => Records.Count > 0);
            HasherCommand = new RelayCommand(() => _windowService.ShowWindow<BinaryWPF.Views.HasherWindow>());
            RaiderCommand = new RelayCommand(() => _windowService.ShowWindow<BinaryWPF.Views.RaiderWindow>());

            LoadRecords();
        }

        public void LoadRecords(int index = -1)
        {
            Records.Clear();
            int count = 0;

            foreach (var record in _str.GetRecords())
            {
                var item = new StringRecordItemViewModel(count++, record);
                if (_modified.Contains(item.Key))
                {
                    item.IsModified = true;
                }
                Records.Add(item);
            }

            if (index >= 0 && index < Records.Count)
            {
                SelectedRecord = Records[index];
            }
        }

        public void SortByColumn(int columnIndex)
        {
            uint key = SelectedRecord?.Record.Key ?? 0xFFFFFFFF;
            int index = SelectedRecord == null ? -1 : 0;

            switch (columnIndex)
            {
                case 1:
                    _str.SortRecordsByKey();
                    if (index == 0) index = FindIndex(key);
                    LoadRecords(index);
                    break;
                case 2:
                    _str.SortRecordsByLabel();
                    if (index == 0) index = FindIndex(key);
                    LoadRecords(index);
                    break;
                case 3:
                    _str.SortRecordsByText();
                    if (index == 0) index = FindIndex(key);
                    LoadRecords(index);
                    break;
                default:
                    break;
            }

            ApplySearch();
        }

        private void AddString()
        {
            var dialog = new StringRecordDialog { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() != true) return;

            try
            {
                _str.AddRecord(dialog.Key, dialog.Label, dialog.TextValue);
                GenerateAddStringCommand(dialog.Key, dialog.Label, dialog.TextValue);
                _modified.Add(dialog.Key);
                LoadRecords();
                int index = FindIndex(Convert.ToUInt32(dialog.Key, 16));
                if (index >= 0) SelectedRecord = Records[index];
                ApplySearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveString()
        {
            if (SelectedRecord == null) return;

            try
            {
                int index = SelectedRecord.Index;
                uint key = SelectedRecord.Record.Key;
                _str.RemoveRecord(key);
                GenerateRemoveStringCommand(SelectedRecord.Key);

                if (_str.StringRecordCount == 0)
                {
                    LoadRecords();
                    EditorText = string.Empty;
                    return;
                }

                LoadRecords(index == 0 ? 0 : index - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditString()
        {
            if (SelectedRecord == null) return;

            var dialog = new StringRecordDialog(SelectedRecord.Record) { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() != true) return;

            try
            {
                var record = SelectedRecord.Record;
                record.SetValue("Key", dialog.Key);
                record.SetValue("Label", dialog.Label);
                record.SetValue("Text", dialog.TextValue);

                var stringKey = Hashing.BinString(Convert.ToUInt32(SelectedRecord.Key, 16), LookupReturn.NULLREF);
                GenerateUpdateStringCommand(stringKey ?? SelectedRecord.Key, TextProperty, dialog.TextValue);
                GenerateUpdateStringCommand(stringKey ?? SelectedRecord.Key, LabelProperty, dialog.Label);
                GenerateUpdateStringCommand(stringKey ?? SelectedRecord.Key, KeyProperty, dialog.Key);

                _modified.Add(dialog.Key);
                SelectedRecord.Refresh();
                SelectedRecord.IsModified = true;
                EditorText = dialog.TextValue;
                ApplySearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReplaceAll()
        {
            if (Records.Count == 0) return;

            var input = new TextInputDialog("Enter string to search for") { Owner = Application.Current.MainWindow };
            if (input.ShowDialog() != true) return;

            var replace = new TextInputDialog("Enter string to replace with") { Owner = Application.Current.MainWindow };
            if (replace.ShowDialog() != true) return;

            bool isCaseSensitive = _promptService.ShowCheckbox("Make case-sensitive replace?", false);
            var options = isCaseSensitive
                ? RegexOptions.Multiline | RegexOptions.CultureInvariant
                : RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

            foreach (var item in Records)
            {
                var record = item.Record;
                record.Text = Regex.Replace(record.Text, input.Value, replace.Value, options);
                GenerateUpdateStringCommand(item.Key, TextProperty, record.Text);

                if (record.Text != item.Text)
                {
                    _modified.Add(item.Key);
                    item.IsModified = true;
                    item.Refresh();
                }
            }

            ApplySearch();
        }

        private void FindPrevious()
        {
            var match = Records.Where(r => r.IsHighlighted).ToList();
            if (match.Count == 0) return;

            int start = SelectedRecord == null ? Records.Count - 1 : SelectedRecord.Index - 1;
            for (int i = start; i >= 0; --i)
            {
                if (Records[i].IsHighlighted)
                {
                    SelectedRecord = Records[i];
                    return;
                }
            }

            for (int i = Records.Count - 1; i > start; --i)
            {
                if (Records[i].IsHighlighted)
                {
                    SelectedRecord = Records[i];
                    return;
                }
            }
        }

        private void FindNext()
        {
            var match = Records.Where(r => r.IsHighlighted).ToList();
            if (match.Count == 0) return;

            int start = SelectedRecord == null ? 0 : SelectedRecord.Index + 1;
            for (int i = start; i < Records.Count; ++i)
            {
                if (Records[i].IsHighlighted)
                {
                    SelectedRecord = Records[i];
                    return;
                }
            }

            for (int i = 0; i < start; ++i)
            {
                if (Records[i].IsHighlighted)
                {
                    SelectedRecord = Records[i];
                    return;
                }
            }
        }

        private void ApplySearch()
        {

            bool hasFilter = !string.IsNullOrEmpty(SearchKey) || !string.IsNullOrEmpty(SearchLabel) || !string.IsNullOrEmpty(SearchText);

            foreach (var item in Records)
            {
                item.IsHighlighted = false;

                if (hasFilter)
                {
                    string compare = SearchKey.Length > 0 ? item.Key :
                                     SearchLabel.Length > 0 ? item.Label :
                                     item.Text;

                    string target = SearchKey.Length > 0 ? SearchKey :
                                    SearchLabel.Length > 0 ? SearchLabel :
                                    SearchText;

                    if (compare.Contains(target, StringComparison.OrdinalIgnoreCase))
                    {
                        item.IsHighlighted = true;
                    }
                }
            }
        }

        private void UpdateSelectedText(string value)
        {
            if (SelectedRecord == null) return;

            var record = SelectedRecord.Record;
            if (record.Text == value) return;

            record.Text = value;
            SelectedRecord.Refresh();
            _modified.Add(SelectedRecord.Key);
            SelectedRecord.IsModified = true;

            if (!string.IsNullOrEmpty(SearchText))
            {
                SelectedRecord.IsHighlighted = record.Text.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            }
        }

        private int FindIndex(uint key)
        {
            int count = 0;
            foreach (var record in _str.GetRecords())
            {
                if (record.Key == key) return count;
                ++count;
            }

            return -1;
        }

        private void GenerateUpdateStringCommand(string key, string property, string value)
        {
            if (value.Contains(' ')) value = $"\"{value}\"";
            var command = $"{eCommandType.update_string} {_strPath} {key} {property} {value}";
            Commands.Add(command);
        }

        private void GenerateAddStringCommand(string key, string label, string text)
        {
            if (label.Contains(' ')) label = $"\"{label}\"";
            if (text.Contains(' ')) text = $"\"{text}\"";
            var command = $"{eCommandType.add_string} {_strPath} {key} {label} {text}";
            Commands.Add(command);
        }

        private void GenerateRemoveStringCommand(string key)
        {
            var command = $"{eCommandType.remove_string} {_strPath} {key}";
            Commands.Add(command);
        }
    }
}
