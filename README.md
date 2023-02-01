# Blazor cache buster

This package deals with making sure lib.module.js files are clear when a new deployment is made or when you update them

For more information on javascript initializers:
(more info [here](https://docs.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-6.0#javascript-initializers))

## How to use

1. Add the nuget package in your **Client** (wasm) **AND** your **Server** (if using Blazor wasm hosted) projects.

```
dotnet add package BlazorCacheBuster
```

2. Publish your app in Release mode and test it!

```
dotnet publish Client\Sample.csproj -c Release
```

_Nuget package page can be found [here](https://www.nuget.org/packages/BlazorCacheBuster)._

### Configuration

The following options allow you to customize the tasks executed by this package.

### **Custom query string**

If you want to use a different query string for busting the cache, for example a specific version, add the following property in the **published** project's .csproj file (**Server** project if using Blazor hosted).

```xml
<!-- By default it will be a new guid for every publish. -->
<CacheId>1</CacheId>
```
### **Automated cache busting**

If you want a more automated way of doing this. By setting the following property this package will append the hash of the file as a query string

```xml
<!-- By default this is disabled, alteast for now. -->
<CacheBustWithHash>true</CacheBustWithHash>
```

### **Disable dll rename**

You can disable the cache busting add the following property in the **published** project's .csproj file (**Server** project if using Blazor hosted).

```xml
<DisableCacheBusting>true</DisableCacheBusting>
```

TODO:
Be able to cache bust scripts/styles in index.html
