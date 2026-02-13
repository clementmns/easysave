using System.Globalization;
using System.Text.Json;
using EasyLog;
using EasyLog.Strategies;
using EasySave.Core.Model;
using EasySave.Core.Utils;

namespace EasySave.Core.Service
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

        private SettingsService(IAppProperties properties)
        {
            if (!Directory.Exists(properties.AppSaveDirectory))
            {
                FileUtils.CreateDirectory(properties.AppSaveDirectory);
            }

            _settingsFilePath = Path.Combine(properties.AppSaveDirectory, "settings.json");
            _settings = LoadOrCreateSettings();
            _settings.AppSaveDirectory = properties.AppSaveDirectory;
            _settings.MaxJobs = properties.MaxJobs;
        }

        /// <summary>
        /// Initialize the settings service
        /// </summary>
        /// <param name="properties"></param>
        public static void Init(IAppProperties properties)
        {
            if (_instance != null) return;
            {
                _instance ??= new SettingsService(properties);
                Logger.Init(properties.AppSaveDirectory, [GetLoggerStrategyFromLogFormat(_instance.Settings.LogFormat)]);
            }
        }
        
        public static SettingsService GetInstance => _instance ?? throw new Exception();
        
        public Settings Settings => _settings;

        /// <summary>
        /// Get the logger strategy from the log format
        /// </summary>
        /// <param name="logFormat">Logging format</param>
        /// <returns>Logging strategy</returns>
        private static ILoggerStrategy GetLoggerStrategyFromLogFormat(LogFormat logFormat)
        {
            ILoggerStrategy loggerStrategy = logFormat switch
            {
                LogFormat.Json => new JsonLoggerStrategy(),
                LogFormat.Xml => new XmlLoggerStrategy(),
                _ => new JsonLoggerStrategy()
            };
            return loggerStrategy;
        }
        
        /// <summary>
        /// Change the log format
        /// </summary>
        /// <param name="format">Log format</param>
        public void ChangeLogFormat(LogFormat format)
        {
            if (_settings.LogFormat == format) return;

            SaveSettings(_settings);
            Logger.ModifyStrategies([GetLoggerStrategyFromLogFormat(format)]);
        }
        
        /// <summary>
        /// Set the application language
        /// </summary>
        /// <param name="language">Language to set (ex: "en-US")</param>
        public void SetLanguage(string language)
        {
            ApplyCulture(language);
            _settings.Language = language;
            SaveSettings(_settings);
        }
        
        /// <summary>
        /// Set extensions that will be encrypted
        /// </summary>
        /// <param name="extensions">List of extensions</param>
        public void SetCryptedExtensions(List<string> extensions)
        {
            _settings.CryptExtensions = extensions;
            SaveSettings(_settings);
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
            var defaultSettings = new Settings
            {
                Language = CultureInfo.InstalledUICulture.Name,
                Version = GetAppVersion(),
                LogFormat = LogFormat.Json
            };
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        private void SaveSettings(Settings settings)
        {
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
        }

        private static void ApplyCulture(string language)
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
