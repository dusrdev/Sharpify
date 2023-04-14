# CHANGELOG

## v1.0.1

* `Utils` class was made upper class of `Env`, `Mathematics` and `DateAndTime` to allow better categorization and maintenance
* Fixed invalid options in the .csproj file

### `Utils.Env`

* Added `GetBaseFolder` which returns the base path of the application directory
* Added `IsRunningAsAdmin` and `IsRunningOnWindows` which are self-explanatory
* Added `IsInternetAvailable` which checks for internet connection

### `Utils.Mathematics`

* Added `FibonacciApproximation` and `Factorial` functions
