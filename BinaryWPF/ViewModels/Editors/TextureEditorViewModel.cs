using BinaryWPF.Services;
using BinaryWPF.Views.UI;

using CoreExtensions.Management;
using CoreExtensions.Text;

using Endscript.Enums;

using ILWrapper.Enums;

using Nikki.Support.Shared.Class;
using Nikki.Utils;
using Nikki.Utils.EA;
using Nikki.Reflection.Enum;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Media.Imaging;

namespace BinaryWPF.ViewModels.Editors
{
    public sealed class TextureEditorViewModel : ViewModelBase
    {
        private readonly TPKBlock _tpk;
        private readonly string _tpkPath;
        private readonly IPromptService _promptService;
        private readonly IWindowService _windowService;

        private TextureItemViewModel? _selectedTexture;
        private BitmapImage? _previewImage;
        private int _lastColumnClicked = -1;

        public ObservableCollection<TextureItemViewModel> Textures { get; } = new();
        public List<string> Commands { get; } = new();

        public TextureItemViewModel? SelectedTexture
        {
            get => _selectedTexture;
            set
            {
                if (SetField(ref _selectedTexture, value))
                {
                    LoadPreview();
                    OnPropertyChanged(nameof(SelectedTexture));
                }
            }
        }

        public BitmapImage? PreviewImage
        {
            get => _previewImage;
            private set => SetField(ref _previewImage, value);
        }

        public RelayCommand AddTextureCommand { get; }
        public RelayCommand RemoveTextureCommand { get; }
        public RelayCommand CopyTextureCommand { get; }
        public RelayCommand ReplaceTextureCommand { get; }
        public RelayCommand ExportTextureCommand { get; }
        public RelayCommand ExportAllCommand { get; }
        public RelayCommand ImportFromCommand { get; }
        public RelayCommand FindReplaceCommand { get; }
        public RelayCommand HasherCommand { get; }
        public RelayCommand RaiderCommand { get; }

        public TextureEditorViewModel(TPKBlock tpk, string path)
        {
            _tpk = tpk;
            _tpkPath = path;
            _promptService = new PromptService();
            _windowService = new WindowService();

            AddTextureCommand = new RelayCommand(AddTexture);
            RemoveTextureCommand = new RelayCommand(RemoveTexture, () => SelectedTexture != null);
            CopyTextureCommand = new RelayCommand(CopyTexture, () => SelectedTexture != null);
            ReplaceTextureCommand = new RelayCommand(ReplaceTexture, () => SelectedTexture != null);
            ExportTextureCommand = new RelayCommand(ExportTexture, () => SelectedTexture != null);
            ExportAllCommand = new RelayCommand(ExportAll);
            ImportFromCommand = new RelayCommand(ImportFrom);
            FindReplaceCommand = new RelayCommand(FindReplace, () => Textures.Count > 0);
            HasherCommand = new RelayCommand(() => _windowService.ShowWindow<BinaryWPF.Views.HasherWindow>());
            RaiderCommand = new RelayCommand(() => _windowService.ShowWindow<BinaryWPF.Views.RaiderWindow>());

            LoadTextures();
        }

        public void LoadTextures(int index = -1)
        {
            Textures.Clear();
            var list = _tpk.GetTextures();

            int count = 0;
            foreach (Texture texture in list)
            {
                string compression = texture.Compression.ToString();
                if (compression.Length > 8) compression = compression.Substring(8);

                bool highlighted = texture.BinKey != texture.CollectionName.BinHash();
                Textures.Add(new TextureItemViewModel(count++, texture, compression, highlighted));
            }

            if (index >= 0 && index < Textures.Count)
            {
                SelectedTexture = Textures[index];
            }
        }

        public void SortByColumn(int columnIndex)
        {
            uint key = SelectedTexture?.Texture.BinKey ?? 0xFFFFFFFF;
            int index = SelectedTexture == null ? -1 : 0;

            switch (columnIndex)
            {
                case 1:
                    _tpk.SortTexturesByType(false);
                    if (_lastColumnClicked == 1)
                    {
                        _tpk.Textures.Reverse();
                        _lastColumnClicked = -1;
                    }
                    else
                    {
                        _lastColumnClicked = 1;
                    }
                    if (index == 0) index = _tpk.GetTextureIndex(key, KeyType.BINKEY);
                    LoadTextures(index);
                    break;
                case 2:
                    _tpk.SortTexturesByType(true);
                    if (_lastColumnClicked == 2)
                    {
                        _tpk.Textures.Reverse();
                        _lastColumnClicked = -1;
                    }
                    else
                    {
                        _lastColumnClicked = 2;
                    }
                    if (index == 0) index = _tpk.GetTextureIndex(key, KeyType.BINKEY);
                    LoadTextures(index);
                    break;
                default:
                    break;
            }
        }

        public void OnPropertyValueChanged(string property, string value)
        {
            if (SelectedTexture == null) return;

            if (property == nameof(Texture.ClassName) ||
                property == nameof(Texture.ClassKey) ||
                property == nameof(Texture.MipmapBiasInt) ||
                property == nameof(Texture.MipmapBiasType))
            {
                return;
            }

            if (property == nameof(Texture.CollectionName))
            {
                SelectedTexture.UpdateCollectionName(value);
            }

            GenerateUpdateTextureCommand(SelectedTexture.BinKey, property, value);
        }

        private void AddTexture()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Direct Draw Surface Files|*.dds"
            };

            if (dialog.ShowDialog() != true) return;

            var initial = Path.GetFileNameWithoutExtension(dialog.FileName);
            var input = new TextInputDialog("Enter name of the new texture", initial) { Owner = Application.Current.MainWindow };

            if (input.ShowDialog() != true)
            {
                return;
            }

            try
            {
                _tpk.AddTexture(input.Value, dialog.FileName);
                GenerateAddTextureCommand(input.Value, dialog.FileName);
                LoadTextures();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveTexture()
        {
            if (SelectedTexture == null) return;

            try
            {
                uint key = SelectedTexture.Texture.BinKey;
                int index = SelectedTexture.Index;

                _tpk.RemoveTexture(key, KeyType.BINKEY);
                GenerateRemoveTextureCommand(SelectedTexture.BinKey);

                if (_tpk.TextureCount == 0)
                {
                    LoadTextures();
                    PreviewImage = null;
                    return;
                }

                LoadTextures(index == 0 ? 0 : index - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyTexture()
        {
            if (SelectedTexture == null) return;

            var input = new TextInputDialog("Enter name of the new texture") { Owner = Application.Current.MainWindow };
            if (input.ShowDialog() != true) return;

            try
            {
                uint key = SelectedTexture.Texture.BinKey;
                _tpk.CloneTexture(input.Value, key, KeyType.BINKEY);
                GenerateCopyTextureCommand(SelectedTexture.BinKey, input.Value);
                LoadTextures(SelectedTexture.Index);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReplaceTexture()
        {
            if (SelectedTexture == null) return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Direct Draw Surface Files|*.dds"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                if (!Comp.IsDDSTexture(dialog.FileName, out string error))
                {
                    throw new Exception(error);
                }

                uint key = SelectedTexture.Texture.BinKey;
                var texture = _tpk.FindTexture(key, KeyType.BINKEY);
                texture.Reload(dialog.FileName);
                GenerateReplaceTextureCommand(SelectedTexture.BinKey, dialog.FileName);
                LoadTextures(SelectedTexture.Index);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportTexture()
        {
            if (SelectedTexture == null) return;

            string filter = "Direct Draw Surface files|*.dds|" +
                            "Portable Network Graphics files|*.png|" +
                            "Joint Photographic Group files|*.jpg|" +
                            "Bitmap Pixel Format files|*.bmp";

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = filter,
                FileName = SelectedTexture.CollectionName
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                string path = dialog.FileName;
                string last = Path.GetExtension(path).ToUpperInvariant()[1..];
                var ext = (ImageType)Enum.Parse(typeof(ImageType), last);

                if (ext == ImageType.DDS)
                {
                    using var bw = new BinaryWriter(File.Open(path, FileMode.Create));
                    bw.Write(SelectedTexture.Texture.GetDDSArray(false));
                }
                else
                {
                    var data = SelectedTexture.Texture.GetDDSArray(true);
                    var image = new ILWrapper.Image(data);
                    image.Save(path, ext);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportAll()
        {
            using var browser = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select directory where all textures should be exported.",
                ShowNewFolderButton = true
            };

            if (browser.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            foreach (Texture texture in _tpk.GetTextures())
            {
                var path = Path.Combine(browser.SelectedPath, texture.CollectionName) + ".dds";
                var data = texture.GetDDSArray(false);
                using var bw = new BinaryWriter(File.Open(path, FileMode.Create));
                bw.Write(data);
            }

            MessageBox.Show("All textures have been exported", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ImportFrom()
        {
            var dialog = new ImportModeDialog { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() != true) return;

            using var browser = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select directory to import textures from.",
                ShowNewFolderButton = false
            };

            if (browser.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            try
            {
                var type = dialog.Mode;

                foreach (var file in Directory.GetFiles(browser.SelectedPath))
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var key = name.BinHash();
                    var texture = _tpk.FindTexture(key, KeyType.BINKEY);

                    if (texture is null)
                    {
                        _tpk.AddTexture(name, file);
                    }
                    else if (type == SerializeType.Synchronize)
                    {
                        texture.Reload(file);
                    }
                    else if (type == SerializeType.Override)
                    {
                        _tpk.RemoveTexture(key, KeyType.BINKEY);
                        _tpk.AddTexture(name, file);
                    }
                }

                MessageBox.Show("All textures have been imported", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                GenerateBindTexturesCommand(type, browser.SelectedPath);
                LoadTextures();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FindReplace()
        {
            if (Textures.Count == 0) return;

            var input = new TextInputDialog("Enter string to search for") { Owner = Application.Current.MainWindow };
            if (input.ShowDialog() != true) return;

            var replace = new TextInputDialog("Enter string to replace with") { Owner = Application.Current.MainWindow };
            if (replace.ShowDialog() != true) return;

            bool isCaseSensitive = _promptService.ShowCheckbox("Make case-sensitive replace?", false);
            var options = isCaseSensitive
                ? RegexOptions.Multiline | RegexOptions.CultureInvariant
                : RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

            for (int i = 0; i < _tpk.TextureCount; ++i)
            {
                var texture = _tpk.Textures[i];
                if (texture.BinKey != texture.CollectionName.BinHash()) continue;

                var cname = Regex.Replace(texture.CollectionName, input.Value, replace.Value, options);
                if (cname == texture.CollectionName) continue;

                texture.CollectionName = cname;
                GenerateUpdateTextureCommand($"0x{texture.BinKey:X8}", "CollectionName", cname);
            }

            LoadTextures();
        }

        private void LoadPreview()
        {
            if (SelectedTexture == null)
            {
                PreviewImage = null;
                return;
            }

            try
            {
                var data = SelectedTexture.Texture.GetDDSArray(true);
                var image = new ILWrapper.Image(data);

                using var ms = new MemoryStream();
                image.Save(ms, ImageType.PNG);
                ms.Position = 0;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();

                PreviewImage = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to preview texture: {ex.GetLowestMessage()}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateUpdateTextureCommand(string key, string property, string value)
        {
            if (property.Contains(' ')) property = $"\"{property}\"";
            if (value.Contains(' ')) value = $"\"{value}\"";
            var command = $"{eCommandType.update_texture} {_tpkPath} {key} {property} {value}";
            Commands.Add(command);
        }

        private void GenerateAddTextureCommand(string name, string file)
        {
            if (name.Contains(' ')) name = $"\"{name}\"";
            if (file.Contains(' ')) file = $"\"{file}\"";
            var command = $"{eCommandType.add_texture} {_tpkPath} {name} {file}";
            Commands.Add(command);
        }

        private void GenerateRemoveTextureCommand(string key)
        {
            var command = $"{eCommandType.remove_texture} {_tpkPath} {key}";
            Commands.Add(command);
        }

        private void GenerateCopyTextureCommand(string key, string name)
        {
            var command = $"{eCommandType.copy_texture} {_tpkPath} {key} {name}";
            Commands.Add(command);
        }

        private void GenerateReplaceTextureCommand(string key, string file)
        {
            if (file.Contains(' ')) file = $"\"{file}\"";
            var command = $"{eCommandType.replace_texture} {_tpkPath} {key} {file}";
            Commands.Add(command);
        }

        private void GenerateBindTexturesCommand(SerializeType type, string directory)
        {
            string import = type.ToString().ToLowerInvariant();
            if (directory.Contains(' ')) directory = $"\"{directory}\"";
            var command = $"{eCommandType.bind_textures} {import} {_tpkPath} {directory}";
            Commands.Add(command);
        }
    }
}
