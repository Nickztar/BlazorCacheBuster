# Blazor cache buster

This package deals with making sure lib.module.js files are clear when a new deployment is made or when you update them
Aswell as replacing script links in your index.html

Note: This is the poor mans version of [StaticAssets](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-9.0), but this works for standalone WASM...

For more information on javascript initializers:
(more info [here](https://docs.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-6.0#javascript-initializers))

## How to use

1. Add the nuget package in your **Client** (wasm) **AND** your **Server** (if using Blazor wasm hosted) projects.

```
dotnet add package BlazorCacheBuster
```

2. Update the script references in your **index.html** and append the query string for cache busting (q=cache)
```
<script src="_content/MudBlazor/MudBlazor.min.js?q=cache"></script>
<script src="test.js?q=cache"></script>
```

3. Publish your app in Release mode and test it!

```
dotnet publish Client\Sample.csproj -c Release
```

_Nuget package page can be found [here](https://www.nuget.org/packages/BlazorCacheBuster)._

### Configuration

The following options allow you to customize the tasks executed by this package.

### **Custom query string**

If you want to use a different query string for busting the cache (for FALLBACK non-relative), for example a specific version, add the following property in the **published** project's .csproj file (**Server** project if using Blazor hosted).

```xml
<!-- By default it will be a new guid for every publish. -->
<BlazorCacheId>1</BlazorCacheId>
```
### **Disable automated cache busting**

If you want a more automated way of doing this. By setting the following property this package will REPLACE the hash of the file as a query string

```xml
<!-- By default this is enabled, alteast for now. -->
<BlazorCacheBustIndexHtml>false</BlazorCacheBustIndexHtml>
```

### **Disable cache busting**

You can disable the cache busting add the following property in the **published** project's .csproj file (**Server** project if using Blazor hosted).

```xml
<BlazorDisableCacheBusting>true</BlazorDisableCacheBusting>
```
