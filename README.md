# ProSoft EasySave

          ______                 _____                 
         |  ____|               / ____|                
         | |__   __ _ ___ _   _| (___   __ ___   _____ 
         |  __| / _` / __| | | |\___ \ / _` \ \ / / _ \
         | |___| (_| \__ \ |_| |____) | (_| |\ V /  __/
         |______\__,_|___/\__, |_____/ \__,_| \_/ \___|
                           __/ |                       
                          |___/                        

EasySave is a simple tool for saving and loading data.


## Projects

### Console

A console application that demonstrates the usage of the EasySave library. It provides a CLI for saving and loading data.


## Getting Started

### Prerequisites

- .NET 10.0 SDK
- Visual Studio or Rider


### Installation

- Clone the repository
- Open the solution in Visual Studio or Rider
- Build the solution : `dotnet build`
- Run the project you want: `dotnet run --project EasySave/ConsoleApp`


# Architecture

```
EasySave/                                # Root directory
│
├── ConsoleApp/                          # Console application
│   ├── Program.cs                       # Entry point
│   ├── Model/
│   │   ├── BackupJob.cs                 # Backup job class
│   │   ├── BackupType.cs                # Backup type enum
│   │   ├── IRealTimeStateObserver.cs    # Observer interface
│   │   ├── LogEntry.cs                  # Log entry class
│   │   ├── RealTimeState.cs             # Real time state enum
│   │   └── Settings.cs                  # Settings class
│   ├── Ressources/
│   │   ├── Errors.resx                  # Error messages
│   │   ├── Messages.resx                # Success messages
│   ├── Service/
│   │   ├── BackupExecutor.cs            # Backup executor class: executes backup jobs
│   │   ├── BackupJobFactory.cs          # Backup job factory class: creates backup jobs
│   │   ├── BackupJobService.cs          # Backup job service class: manages backup jobs and state.json
│   │   └── SettingsService.cs           # Settings service class: manages settings.json
│   ├── Utils/
│   │   └── FileUtils.cs                 # File utils class 
│   ├── View/
│   │   └── ConsoleAppView.cs            # Console app view class: displays messages and prompts user input 
│   └── ViewModel/
│       └── BackupViewModel.cs           # Backup view model class: manages interaction between model and view
│
└── EasyLog/
    ├── ILoggerStrategy.cs               # Logger strategy interface
    ├── Logger.cs                        # Logger class: logs messages using selected strategies
    └── Strategies/
        └── JsonLoggerStrategy.cs        # Json logger strategy class: logs messages to a json file
```


## Contributors

- MIGNOT--PILON Antonin
- FARDELLA Timothé
- BOUZJALLIKHT Yanis
- OMNÈS Clément

<a href="https://github.com/clementmns/prosoft-easysave/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=clementmns/prosoft-easysave" />
</a>


## License

Distributed under the MIT License. See `LICENSE` for more information.