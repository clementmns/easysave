# EasyLog

EasyLog is a logging library designed for use in the ProSoft company. It provides a flexible interface and logging strategies to record events and information in various formats.


## Supported Formats

- **JSON**


## Usage

```csharp
using EasyLog;
using EasyLog.Strategies;

class Program
{
    static void Main(string[] args)
    {
        // Suggested directory for storing logs and config, e.g.:
        string appSaveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ProSoft",
            "EasySave"
        );
        Logger.Init(appSaveDirectory, [ new JsonLoggerStrategy() ]);

        Logger.Instance.Write("log");

        // Note:
        // The first parameter to Logger.Init is now the directory path where logs and config files will be stored.
        // Be sure to create the parent directory if it does not exist. See documentation for more details.
    }
}
```