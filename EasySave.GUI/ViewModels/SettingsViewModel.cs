using System.Collections.ObjectModel;

namespace EasySave.GUI.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private string _selectedLogFormat = "JSON";
        private string _businessSoftware = string.Empty;
        private string _newExtensionInput = string.Empty;
        private string _successMessage = string.Empty;
        private string _errorMessage = string.Empty;

        public SettingsViewModel()
        {
            AvailableFormats = new ObservableCollection<string>
            {
                "JSON",
                "XML"
            };

            ExtensionsToEncrypt = new ObservableCollection<string>
            {
                ".txt",
                ".pdf"
            };

            SaveCommand = new RelayCommand(_ => SaveSettings());
            AddExtensionCommand = new RelayCommand(_ => AddExtension(), _ => !string.IsNullOrWhiteSpace(NewExtensionInput));
            RemoveExtensionCommand = new RelayCommand(extension => RemoveExtension(extension as string));
        }

        public ObservableCollection<string> AvailableFormats { get; }

        public ObservableCollection<string> ExtensionsToEncrypt { get; }

        public string SelectedLogFormat
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

        public RelayCommand SaveCommand { get; }

        public RelayCommand AddExtensionCommand { get; }

        public RelayCommand RemoveExtensionCommand { get; }

        private void SaveSettings()
        {
            ErrorMessage = string.Empty;
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
    }
}