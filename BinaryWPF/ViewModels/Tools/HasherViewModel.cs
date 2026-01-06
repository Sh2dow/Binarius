using Nikki.Utils;
using CoreExtensions.Native;

namespace BinaryWPF.ViewModels.Tools
{
    public sealed class HasherViewModel : ViewModelBase
    {
        private string _input = string.Empty;
        private string _binHash = string.Empty;
        private string _binFile = string.Empty;
        private string _vltHash = string.Empty;
        private string _vltFile = string.Empty;

        public string Input
        {
            get => _input;
            set
            {
                if (SetField(ref _input, value))
                {
                    UpdateHashes(value);
                }
            }
        }

        public string BinHash { get => _binHash; private set => SetField(ref _binHash, value); }
        public string BinFile { get => _binFile; private set => SetField(ref _binFile, value); }
        public string VltHash { get => _vltHash; private set => SetField(ref _vltHash, value); }
        public string VltFile { get => _vltFile; private set => SetField(ref _vltFile, value); }

        private void UpdateHashes(string value)
        {
            string _0x = "0x";
            bool state = Hashing.PauseHashSave;
            Hashing.PauseHashSave = true;

            uint result = value.BinHash();
            BinHash = $"{_0x}{result:X8}";

            result = result.Reverse();
            BinFile = $"{_0x}{result:X8}";

            result = value.VltHash();
            VltHash = $"{_0x}{result:X8}";

            result = result.Reverse();
            VltFile = $"{_0x}{result:X8}";

            Hashing.PauseHashSave = state;
        }
    }
}
