using Nikki.Core;
using CoreExtensions.Native;
using CoreExtensions.Text;

namespace BinaryWPF.ViewModels.Tools
{
    public sealed class RaiderViewModel : ViewModelBase
    {
        private int _searchMode;
        private string _binHash = string.Empty;
        private string _binFile = string.Empty;
        private string _result = string.Empty;

        public int SearchMode
        {
            get => _searchMode;
            set
            {
                if (SetField(ref _searchMode, value))
                {
                    OnPropertyChanged(nameof(IsBinHashMode));
                    OnPropertyChanged(nameof(IsBinFileMode));
                }
            }
        }

        public bool IsBinHashMode => SearchMode == 0;
        public bool IsBinFileMode => SearchMode == 1;

        public string BinHash
        {
            get => _binHash;
            set
            {
                if (SetField(ref _binHash, value))
                {
                    if (IsBinHashMode)
                    {
                        ResolveFromBinHash(value);
                    }
                }
            }
        }

        public string BinFile
        {
            get => _binFile;
            set
            {
                if (SetField(ref _binFile, value))
                {
                    if (IsBinFileMode)
                    {
                        ResolveFromBinFile(value);
                    }
                }
            }
        }

        public string Result
        {
            get => _result;
            private set => SetField(ref _result, value);
        }

        private void ResolveFromBinHash(string input)
        {
            string value = input.StartsWith("0x") ? input : $"0x{input}";
            if (!value.IsHexString() || value.Length > 10)
            {
                Result = "N/A";
                return;
            }

            uint key = System.Convert.ToUInt32(value, 16);
            BinFile = $"0x{key.Reverse():X8}";
            Result = Map.BinKeys.TryGetValue(key, out string? result) ? result ?? "N/A" : "N/A";
        }

        private void ResolveFromBinFile(string input)
        {
            string value = input.StartsWith("0x") ? input : $"0x{input}";
            if (!value.IsHexString() || value.Length > 10)
            {
                Result = "N/A";
                return;
            }

            uint key = System.Convert.ToUInt32(value, 16);
            key = key.Reverse();
            BinHash = $"0x{key:X8}";
            Result = Map.BinKeys.TryGetValue(key, out string? result) ? result ?? "N/A" : "N/A";
        }
    }
}
