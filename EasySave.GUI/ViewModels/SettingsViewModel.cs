using System.Collections.ObjectModel;
using EasyLog;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.Core.Repositories;

namespace EasySave.GUI.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private LogFormat _selectedLogFormat = LogFormat.Json;
        private string _businessSoftware = string.Empty;
        private string _newExtensionInput = string.Empty;
        private string _successMessage = string.Empty;
        private string _errorMessage = string.Empty;
        private string _selectedLanguage = "fr";
        private readonly BackupManager _backupManager;

        public SettingsViewModel()
        {
            _backupManager = new BackupManager(
                new JsonJobRepository(),
                new JsonSettingsRepository());

            AvailableFormats = new ObservableCollection<LogFormat>
            {
                LogFormat.Json,
                LogFormat.Xml
            };

                AvailableLanguages = new ObservableCollection<string>
                {
                    "fr",
                    "en"
                };
            ExtensionsToEncrypt = new ObservableCollection<string>();

            SaveCommand = new RelayCommand(_ => SaveSettings());
            AddExtensionCommand = new RelayCommand(_ => AddExtension(), _ => !string.IsNullOrWhiteSpace(NewExtensionInput));
            RemoveExtensionCommand = new RelayCommand(extension => RemoveExtension(extension as string));

            LoadSettings();
        }

        public ObservableCollection<LogFormat> AvailableFormats { get; }

        public ObservableCollection<string> ExtensionsToEncrypt { get; }

        public ObservableCollection<string> AvailableLanguages { get; }

        public LogFormat SelectedLogFormat
        {
            get => _selectedLogFormat;
            set
            {
                _selectedLogFormat = value;
                OnPropertyChanged();
            }
        }

        public string BusinessSoftware
        {
            get => _businessSoftware;
            set
            {
                _businessSoftware = value;
                OnPropertyChanged();
            }
        }

        public string NewExtensionInput
        {
            get => _newExtensionInput;
            set
            {
                _newExtensionInput = value;
                OnPropertyChanged();
                AddExtensionCommand.RaiseCanExecuteChanged();
            }
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                _successMessage = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SaveCommand { get; }

        public RelayCommand AddExtensionCommand { get; }

        public RelayCommand RemoveExtensionCommand { get; }

        private void SaveSettings()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var settings = new AppSettings
            {
                LogFormat = SelectedLogFormat,
                Language = SelectedLanguage,
                BusinessSoftware = BusinessSoftware,
                ExtensionsToEncrypt = new System.Collections.Generic.List<string>(ExtensionsToEncrypt)
            };

            _backupManager.SaveSettings(settings);

            SuccessMessage = "Settings saved successfully.";
        }

        private void AddExtension()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            string extension = NewExtensionInput.Trim();

            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            if (ExtensionsToEncrypt.Contains(extension))
            {
                ErrorMessage = "This extension already exists.";
                return;
            }

            ExtensionsToEncrypt.Add(extension);
            NewExtensionInput = string.Empty;
            SuccessMessage = "Extension added.";
        }

        private void RemoveExtension(string? extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return;
            }

            ExtensionsToEncrypt.Remove(extension);
            SuccessMessage = "Extension removed.";
        }

        private void LoadSettings()
        {
            AppSettings settings = _backupManager.GetSettings();

            SelectedLogFormat = settings.LogFormat;
            BusinessSoftware = settings.BusinessSoftware;
            SelectedLanguage = settings.Language;

            ExtensionsToEncrypt.Clear();

            foreach (string extension in settings.ExtensionsToEncrypt)
            {
                ExtensionsToEncrypt.Add(extension);
            }
        }
    }
}