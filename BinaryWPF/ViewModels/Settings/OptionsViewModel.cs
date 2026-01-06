using BinaryWPF.Properties;

namespace BinaryWPF.ViewModels.Settings
{
    public sealed class OptionsViewModel : ViewModelBase
    {
        private string _watermark = Configurations.Default.Watermark;
        private bool _autoBackups = Configurations.Default.AutoBackups;
        private bool _startMaximized = Configurations.Default.StartMaximized;
        private bool _soonFeature = Configurations.Default.SoonFeature;
        private bool _disableAdminWarning = Configurations.Default.DisableAdminWarning;
        private bool _hideEmptyManagers = Configurations.Default.HideEmptyManagers;

        public string Watermark
        {
            get => _watermark;
            set => SetField(ref _watermark, value);
        }

        public bool AutoBackups
        {
            get => _autoBackups;
            set => SetField(ref _autoBackups, value);
        }

        public bool StartMaximized
        {
            get => _startMaximized;
            set => SetField(ref _startMaximized, value);
        }

        public bool SoonFeature
        {
            get => _soonFeature;
            set => SetField(ref _soonFeature, value);
        }

        public bool DisableAdminWarning
        {
            get => _disableAdminWarning;
            set => SetField(ref _disableAdminWarning, value);
        }

        public bool HideEmptyManagers
        {
            get => _hideEmptyManagers;
            set => SetField(ref _hideEmptyManagers, value);
        }

        public void Save()
        {
            Configurations.Default.Watermark = Watermark;
            Configurations.Default.AutoBackups = AutoBackups;
            Configurations.Default.StartMaximized = StartMaximized;
            Configurations.Default.SoonFeature = SoonFeature;
            Configurations.Default.DisableAdminWarning = DisableAdminWarning;
            Configurations.Default.HideEmptyManagers = HideEmptyManagers;
            Configurations.Default.Save();
        }
    }
}
