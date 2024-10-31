# Getting Started

1. Fork or clone the repository to your machine
2. If cloned, rename the remote with `git remote rename origin boilerplate`
3. Ensure you have an appropriate `appsettings.development.json`, which matches the default apsettings file present in both `KDG.Boilerplate.Server` and `Migrations`.
4. Manage and execute migrations `cd Migrations && dotnet run`
5. Run the app via CLI with `cd KDG.Boilerplate.Server && dotnet watch run`
> This will install all required server and client dependencies

2. Your browser should open automatically, otherwise. If it doesn't, manucally navigate to https://localhost:5173

## Additional DevTools

Additional [DevTools](https://github.com/KDG-Development/KDG-Net-DevTools) are available to assist with development when using this boilerplate

1. Install with `git clone https://github.com/KDG-Development/KDG-Net-DevTools ./DevTools`
2. See the [README](https://github.com/KDG-Development/KDG-Net-DevTools/blob/main/README.md) for more information


# Staying up to date

This boilerplate is designed to be both a starting point and an ever-evolving foundation for your .NET/React applications.

In order to extract the most value from this boilerplate, it is important to keep your application up to date frequently reintegrating into your application with one of the applicable workflows:

## Cloned Repositories
- Run `git pull --rebase [branch] boilerplate`
## Forked Repositories
- Create a pull request from this repository into your fork

# Contributing

Pull requests are welcome and very much appreciated!

## Testing library changes locally

When working on common libraries that you want to integrate into this boilerplate, you should test them prior to releasing them on nuget.

1. Update your local packages version to a semantic development version for local consumption via .csproj or related
```
<PropertyGroup>
    <Version>0.0.1-some-feature-development-1</Version>
</PropertyGroup>
```

The first part of this temporary version is the version which you aspire to release. The latter half is your local iteration for testing.


2. Add the path to your local consuming project build via .csproj or similar
```
  <PropertyGroup>
    <RestoreSources>
    $(RestoreSources);
    <!-- Add your paths to local nuget directories here -->
    <!-- e.g., [...\folder\some-folder-with-a-nupkg-file]; without the brackets -->
    https://api.nuget.org/v3/index.json;
  </RestoreSources>
  </PropertyGroup>
```

3. Ensure the package reference inside the csproj references the most recent development build
```
<PackageReference Include="[your-package-name]" Version="0.0.1-some-feature-development-1" />
```
4. Restore with `dotnet restore`