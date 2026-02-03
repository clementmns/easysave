using System.Globalization;
using System.Text.Json;
using EasySave.ConsoleApp.Model;
using EasySave.ConsoleApp.Utils;

namespace EasySave.ConsoleApp.Service
{
    public class SettingsService
    {
        private static SettingsService? _instance;
        private static readonly Lock Lock = new();

        private readonly string _settingsFilePath;
        private readonly Settings _settings;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private SettingsService(string appDirectory)
        {
            if (!FileUtils.DirectoryExists(appDirectory))
            {
                FileUtils.CreateDirectory(appDirectory);
            }

            _settingsFilePath = Path.Combine(appDirectory, "settings.json");
            _settings = LoadOrCreateSettings();
        }

        /// <summary>
        /// Settings service singleton instance
        /// </summary>
        /// <exception cref="InvalidOperationException">Settings service must be initialized first.</exception>
        public static SettingsService Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException(
                        "SettingsService has not been initialized. Call Init() first.");
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initialize the settings service
        /// </summary>
        /// <param name="appDirectory">Application content directory</param>
        public static void Init(string appDirectory)
        {
            if (_instance != null) return;
            lock (Lock)
            {
                _instance ??= new SettingsService(appDirectory);
            }
        }
        
        public Settings Settings => _settings;
        
        /// <summary>
        /// Set the application language
        /// </summary>
        /// <param name="language">Language to set (ex: "en-US")</param>
        public void SetLanguage(string language)
        {
            var culture = CultureInfo.GetCultureInfo(language);

            _settings.Language = language;

            // Change culture(lang) for all threads
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            SaveSettings(_settings);
        }
        
        private Settings LoadOrCreateSettings()
        {
            if (!FileUtils.FileExists(_settingsFilePath)) return CreateDefaultSettings();
            try
            {
                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<Settings>(json, JsonOptions);
                if (settings != null) return settings;
            }
            catch
            {
                // ignored because we want to create default settings if loading fails
            }

            return CreateDefaultSettings();
        }

        private Settings CreateDefaultSettings()
        {
            var defaultSettings = new Settings(); // that uses default values
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        private void SaveSettings(Settings settings)
        {
            lock (Lock)
            {
                var json = JsonSerializer.Serialize(settings, JsonOptions);
                File.WriteAllText(_settingsFilePath, json);
            }
        }
    }
}
