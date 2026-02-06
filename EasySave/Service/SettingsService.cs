using System.Globalization;
using System.Text.Json;
using EasySave.Model;
using EasySave.Utils;

namespace EasySave.Service
{
    public class SettingsService
    {
        private static SettingsService? _instance;

        private readonly string _settingsFilePath;
        private readonly Settings _settings;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private SettingsService(string appDirectory)
        {
            if (!Directory.Exists(appDirectory))
            {
                FileUtils.CreateDirectory(appDirectory);
            }

            _settingsFilePath = Path.Combine(appDirectory, "settings.json");
            _settings = LoadOrCreateSettings();
        }

        /// <summary>
        /// Initialize the settings service
        /// </summary>
        /// <param name="appDirectory">Application content directory</param>
        public static void Init(string appDirectory)
        {
            if (_instance != null) return;
            {
                _instance ??= new SettingsService(appDirectory);
            }
        }
        
        public static SettingsService GetInstance => _instance ?? throw new Exception();
        
        public Settings Settings => _settings;
        
        /// <summary>
        /// Set the application language
        /// </summary>
        /// <param name="language">Language to set (ex: "en-US")</param>
        public void SetLanguage(string language)
        {
            ApplyCulture(language);
            var culture = CultureInfo.GetCultureInfo(language);
            if (_settings != null)
            {
                _settings.Language = language;
                SaveSettings(_settings);
            }
        }
        
        /// <summary>
        /// Load or create the settings file
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Can't load</exception>
        private Settings LoadOrCreateSettings()
        {
            if (!File.Exists(_settingsFilePath)) return CreateDefaultSettings();
            try
            {
                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<Settings>(json, JsonOptions);
                if (settings != null)
                {
                    if (settings.Version == GetAppVersion())
                    {
                        ApplyCulture(settings.Language);
                        return settings;
                    }
                    var newSettings = CreateDefaultSettings();
                    SaveSettings(newSettings);
                    return newSettings;
                }
            }
            catch
            {
                throw new Exception();
            }
            return CreateDefaultSettings();
        }

        /// <summary>
        /// Get the application version from the assembly
        /// </summary>
        /// <returns>app version</returns>
        private static string GetAppVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version is null ? "1.0.0" : $"{version.Major}.{version.Minor}.{version.Build}";
        }

        private Settings CreateDefaultSettings()
        {
            var defaultSettings = new Settings { Language = CultureInfo.InstalledUICulture.Name, Version = GetAppVersion() };
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        private void SaveSettings(Settings settings)
        {
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
        }

        private void ApplyCulture(string language)
        {
            try
            {
                var culture = CultureInfo.GetCultureInfo(language);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}
