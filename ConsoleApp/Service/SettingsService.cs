using System.Globalization;
using System.Text.Json;
using EasySave.ConsoleApp.Model;
using EasySave.ConsoleApp.Utils;

namespace EasySave.ConsoleApp.Service
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
                if (settings != null)
                {
                    if (settings.Version == GetAppVersion()) return settings;
                    var newSettings = CreateDefaultSettings();
                    SaveSettings(newSettings);
                    return newSettings;
                }
            }
            catch
            {
                // ignored because we want to create default settings if loading fails
            }
            return CreateDefaultSettings();
        }

        private static string GetAppVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version is null ? "1.0.0" : $"{version.Major}.{version.Minor}.{version.Build}";
        }

        private Settings CreateDefaultSettings()
        {
            var defaultSettings = new Settings { Version = GetAppVersion() };
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        private void SaveSettings(Settings settings)
        {
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
        }
    }
}
