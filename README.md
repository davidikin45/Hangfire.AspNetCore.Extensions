# Hangfire ASP.NET Core Extensions
[![nuget](https://img.shields.io/nuget/v/Hangfire.AspNetCore.Extensions.svg)](https://www.nuget.org/packages/Hangfire.AspNetCore.Extensions/) ![Downloads](https://img.shields.io/nuget/dt/Hangfire.AspNetCore.Extensions.svg "Downloads")

* Simple extensions method for adding Hangfire SqlServer, SQLite or MemoryStorage to .NET Core Console or Web Applications. Replaces the need for services.AddHangfire + services.AddHangfireServer. 

## Installation

### NuGet
```
PM> Install-Package Hangfire.AspNetCore.Extensions
```

### .Net CLI
```
> dotnet add package Hangfire.AspNetCore.Extensions
```

## Examples
```
const string connectionString = "Server=(localdb)\\mssqllocaldb;Database=Hangfire;Trusted_Connection=True;MultipleActiveResultSets=true;";
const string connectionString = "Data Source=:hangfire.db;";
const string connectionString = "Data Source=:memory:;";
const string connectionString = "";

services.AddHangfireServer("web-background", connectionString);
```

```
services.AddHangfireInMemoryServer("web-background");
```

```
services.AddHangfireSQLiteInMemoryServer("web-background");
```

## Authors

* **Dave Ikin** - [davidikin45](https://github.com/davidikin45)


## License

This project is licensed under the MIT License


## Acknowledgments

* [Hangfire](https://www.hangfire.io/)
* [Hangfire.SqlServer](https://github.com/HangfireIO/Hangfire/tree/master/src/Hangfire.SqlServer)
* [Hangfire.SQLite](https://github.com/wanlitao/HangfireExtension)
* [Hangfire.MemoryStorage](https://github.com/perrich/Hangfire.MemoryStorage)