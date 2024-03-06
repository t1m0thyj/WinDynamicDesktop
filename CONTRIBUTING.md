# Contributing

Pull requests are welcome. For major changes, please [open an issue](https://github.com/t1m0thyj/WinDynamicDesktop/issues/new?assignees=&labels=&projects=&template=feature_request.md) to discuss them first.

## How to Build

1. Install [Visual Studio](https://visualstudio.microsoft.com/vs/) (Community edition is fine) with the following workloads:
    * .NET desktop deployment
    * Universal Windows Platform deployment

2. Create an env file by copying `.env.example` to `.env`. Define the following variables in it:
    * `LOCATIONIQ_API_KEY` (optional) - [LocationIQ API key](https://help.locationiq.com/support/solutions/articles/36000172496-how-do-i-get-the-api-key-access-token-) used to get latitude and longitude based on city name
    * `POEDITOR_API_TOKEN` (optional) - [POEditor API token](https://poeditor.com/account/api) used to test new translations from the POEditor website

3. Open [the solution file](./src/WinDynamicDesktop.sln) in Visual Studio and build one of the projects:
    * `WinDynamicDesktop` - Main project to run the app
    * `WinDynamicDesktop.Package` - UWP project for Microsoft Store app
    * `WinDynamicDesktop.Tests` - Unit tests
