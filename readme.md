# Digipolis Caching library

This library can be used for handling caching in your application. 
Short lived cache will be added as in memory cache while long lived cache will be added as a [Redis](https://redis.io) cache.
To request a Redis cache, please sent an [email](mailto:servicedesk@digipolis.be?subject=Aanvraag%20Redis%20cache) to the Digipolis servicedesk.  

## Table of Contents

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->


- [Target framework](#target-framework)
- [Installation](#installation)
- [Usage](#usage)
- [App settings](#app-settings)
- [Contributing](#contributing)
- [Support](#support)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Target framework

This package targets **.NET Core 3.1**.

## Installation

To add the library to a project, you add the package to the csproj file :

```xml
  <ItemGroup>
    <PackageReference Include="Digipolis.Caching" Version="3.0.0" />
  </ItemGroup>
```

In Visual Studio you can also use the NuGet Package Manager to do this.

## Usage

This library serves as the Digipolis Caching library. It contains the service collection extension method to register the
Digipolis caching options, to be called in the **ConfigureServices** method of the **Startup** class.

```csharp  
services.AddCache(configuration, environment);
```

Optionally you can also specify the configuration section as the third argument. 
That section should contain the caching settings specified in the [App settings](#app-settings) section.
When no configuration section is supplied the default expects a nested structure like so:

```json 
"DataAccess": {
    "Cache": {
        "Configuration": {},
        "CacheEnabled": "true",
        "Tier2Enabled": "true",
        "DefaultMinutesToCacheTier1": "100",
        "DefaultMinutesToCacheTier2": "100",
        "TimeoutAsyncAfterSeconds": "60"
    },
}
```

## App settings
You can determine the cache setting using your app settings. See table below for all caching options.

| Setting                    | Description                                                  | Default |
| -------------------------- | ------------------------------------------------------------ | ------- |
| Configuration              | This is the Redis cache configuration                        |         |
| CacheEnabled               | Determines whether the cache is enabled                      | false   |
| Tier2Enabled               | Determines whether the distributed (Redis) cache is enabled  | false   |
| DefaultMinutesToCacheTier1 | Determines the default storage keeping time in minutes of the local cache |         |
| DefaultMinutesToCacheTier2 | Determines the default storage keeping time in minutes of the distributed (Redis) cache |         |
| TimeoutAsyncAfterSeconds   | Determines the default timeout time in seconds for retrieving data from the distributed store. | 5       |

## Contributing

Pull requests are always welcome, however keep the following things in mind:

- New features (both breaking and non-breaking) should always be discussed with the [repo's owner](#support). If possible, please open an issue first to discuss what you would like to change.
- Fork this repo and issue your fix or new feature via a pull request.
- Please make sure to update tests as appropriate. Also check possible linting errors and update the CHANGELOG if applicable.

## Support

Erik Seynaeve (<Erik.Seynaeve@digipolis.be>)