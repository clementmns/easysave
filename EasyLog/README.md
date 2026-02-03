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
        Logger.Init("EasySave", [ new JsonLoggerStrategy() ]);
        
        Logger.Instance.Write("log");
    }
}
```