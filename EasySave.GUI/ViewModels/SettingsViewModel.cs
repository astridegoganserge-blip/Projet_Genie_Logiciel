using System.Collections.Generic;
using System.Collections.ObjectModel;
using EasyLog;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.Core.Repositories;
using EasySave.GUI.Services;



namespace EasySave.GUI.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly BackupManager _backupManager;



        private LogFormat _selectedLogFormat = LogFormat.Json;
        private string _businessSoftware = string.Empty;
        private string _newExtensionInput = string.Empty;
        private string _newPriorityExtensionInput = string.Empty;
        private long _maxFileSizeKb;
        private LogMode _selectedLogMode = LogMode.Local;
        private string _dockerLogServerUrl = string.Empty;
        private string _successMessage = string.Empty;
        private string _errorMessage = string.Empty;
        private string _selectedLanguage = "fr";
        private bool _deleteOrphanFilesInDifferential;



        public SettingsViewModel()
        : this(new BackupManager(new JsonJobRepository(), new JsonSettingsRepository()))
        {
        }



        public SettingsViewModel(BackupManager backupManager)
        {
            _backupManager = backupManager;



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



            AvailableLogModes = new ObservableCollection<LogMode>
 {
 LogMode.Local,
 LogMode.Docker,
 LogMode.Both
 };



            ExtensionsToEncrypt = new ObservableCollection<string>();
            PriorityExtensions = new ObservableCollection<string>();



            SaveCommand = new RelayCommand(_ => SaveSettings());



            AddExtensionCommand = new RelayCommand(
            _ => AddExtension(),
            _ => !string.IsNullOrWhiteSpace(NewExtensionInput));



            RemoveExtensionCommand = new RelayCommand(
            extension => RemoveExtension(extension as string));



            AddPriorityExtensionCommand = new RelayCommand(
            _ => AddPriorityExtension(),
            _ => !string.IsNullOrWhiteSpace(NewPriorityExtensionInput));



            RemovePriorityExtensionCommand = new RelayCommand(
            extension => RemovePriorityExtension(extension as string));



            LoadSettings();
        }



        public ObservableCollection<LogFormat> AvailableFormats { get; }



        public ObservableCollection<string> AvailableLanguages { get; }



        public ObservableCollection<LogMode> AvailableLogModes { get; }



        public ObservableCollection<string> ExtensionsToEncrypt { get; }



        public ObservableCollection<string> PriorityExtensions { get; }



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



        public string NewPriorityExtensionInput
        {
            get => _newPriorityExtensionInput;
            set
            {
                _newPriorityExtensionInput = value;
                OnPropertyChanged();
                AddPriorityExtensionCommand.RaiseCanExecuteChanged();
            }
        }



        public long MaxFileSizeKb
        {
            get => _maxFileSizeKb;
            set
            {
                _maxFileSizeKb = value < 0 ? 0 : value;
                OnPropertyChanged();
            }
        }



        public LogMode SelectedLogMode
        {
            get => _selectedLogMode;
            set
            {
                _selectedLogMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowDockerUrl));
            }
        }



        public string DockerLogServerUrl
        {
            get => _dockerLogServerUrl;
            set
            {
                _dockerLogServerUrl = value;
                OnPropertyChanged();
            }
        }



        public bool ShowDockerUrl => SelectedLogMode != LogMode.Local;



        public bool DeleteOrphanFilesInDifferential
        {
            get => _deleteOrphanFilesInDifferential;
            set
            {
                _deleteOrphanFilesInDifferential = value;
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



        public RelayCommand SaveCommand { get; }



        public RelayCommand AddExtensionCommand { get; }



        public RelayCommand RemoveExtensionCommand { get; }



        public RelayCommand AddPriorityExtensionCommand { get; }



        public RelayCommand RemovePriorityExtensionCommand { get; }



        private void SaveSettings()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;



            if (!ValidateSettings())
            {
                return;
            }



            var settings = new AppSettings
            {
                LogFormat = SelectedLogFormat,
                Language = SelectedLanguage,
                BusinessSoftware = BusinessSoftware.Trim(),
                ExtensionsToEncrypt = new List<string>(ExtensionsToEncrypt),
                PriorityExtensions = new List<string>(PriorityExtensions),
                MaxFileSizeKb = MaxFileSizeKb,
                LogMode = SelectedLogMode,
                DockerLogServerUrl = DockerLogServerUrl.Trim(),
                DeleteOrphanFilesInDifferential = DeleteOrphanFilesInDifferential
            };



            _backupManager.SaveSettings(settings);
            LocalizationService.ApplyLanguage(SelectedLanguage);



            object? resource = System.Windows.Application.Current?.TryFindResource("MsgSettingsSaved");
            SuccessMessage = resource?.ToString() ?? "Settings saved successfully.";
        }



        private bool ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(SelectedLanguage))
            {
                ErrorMessage = "Language is required.";
                return false;
            }



            if (SelectedLogMode != LogMode.Local && string.IsNullOrWhiteSpace(DockerLogServerUrl))
            {
                ErrorMessage = "Docker log server URL is required when Docker log mode is enabled.";
                return false;
            }



            return true;
        }



        private void AddExtension()
        {
            AddNormalizedExtension(
            NewExtensionInput,
            ExtensionsToEncrypt,
            () => NewExtensionInput = string.Empty,
            "Encryption extension added.");
        }



        private void RemoveExtension(string? extension)
        {
            RemoveExtensionFromCollection(
            extension,
            ExtensionsToEncrypt,
            "Encryption extension removed.");
        }



        private void AddPriorityExtension()
        {
            AddNormalizedExtension(
            NewPriorityExtensionInput,
            PriorityExtensions,
            () => NewPriorityExtensionInput = string.Empty,
            "Priority extension added.");
        }



        private void RemovePriorityExtension(string? extension)
        {
            RemoveExtensionFromCollection(
            extension,
            PriorityExtensions,
            "Priority extension removed.");
        }



        private void AddNormalizedExtension(
        string input,
        ObservableCollection<string> targetCollection,
        System.Action clearInput,
        string successMessage)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;



            string extension = NormalizeExtension(input);



            if (string.IsNullOrWhiteSpace(extension))
            {
                ErrorMessage = "Extension is required.";
                return;
            }



            if (targetCollection.Contains(extension))
            {
                ErrorMessage = "This extension already exists.";
                return;
            }



            targetCollection.Add(extension);
            clearInput();



            SuccessMessage = successMessage;
        }



        private void RemoveExtensionFromCollection(
        string? extension,
        ObservableCollection<string> targetCollection,
        string successMessage)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;



            if (string.IsNullOrWhiteSpace(extension))
            {
                return;
            }



            targetCollection.Remove(extension);
            SuccessMessage = successMessage;
        }



        private void LoadSettings()
        {
            AppSettings settings = _backupManager.GetSettings();



            SelectedLogFormat = settings.LogFormat;
            BusinessSoftware = settings.BusinessSoftware;
            SelectedLanguage = string.IsNullOrWhiteSpace(settings.Language)
            ? "fr"
            : settings.Language;



            MaxFileSizeKb = settings.MaxFileSizeKb;
            SelectedLogMode = settings.LogMode;
            DockerLogServerUrl = settings.DockerLogServerUrl;
            DeleteOrphanFilesInDifferential = settings.DeleteOrphanFilesInDifferential;



            ExtensionsToEncrypt.Clear();



            foreach (string extension in settings.ExtensionsToEncrypt)
            {
                ExtensionsToEncrypt.Add(NormalizeExtension(extension));
            }



            PriorityExtensions.Clear();



            foreach (string extension in settings.PriorityExtensions)
            {
                PriorityExtensions.Add(NormalizeExtension(extension));
            }
        }



        private static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return string.Empty;
            }



            string normalized = extension.Trim();



            if (!normalized.StartsWith("."))
            {
                normalized = "." + normalized;
            }



            return normalized.ToLowerInvariant();
        }
    }
}