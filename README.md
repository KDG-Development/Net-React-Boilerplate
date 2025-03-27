# Introduction

Follow the instructions under "Getting Started" to develop features directly applicable to the boilerplate project.

In order to build a new project using the boilerplate code, follow the instructions under "Creating a Project Based on Boilerplate".

# Prerequisites

1. Install Docker Desktop

# Getting Started

1. Fork or clone the boilerplate repository to your machine
2. If cloned, rename the remote with `git remote rename origin boilerplate`
3. Ensure you have an appropriate `appsettings.development.json`, which matches the default appsettings.json at the root of the project
4. Start docker desktop and build the project with `docker compose up --build` or run `docker compose up` to start the project without building

> Your browser should open automatically, otherwise. If it doesn't, manually navigate to https://localhost:5173

## Configuring the Azure Deployment Pipeline

1. Create the container registry in azure portal for your environment
2. Create a service connection in azure devops of type docker container registry
3. Create a service connection in azure devops of type azure resource manager
4. Rename the azure-pipelines-example.yml file to azure-pipelines.[environment].yml
5. Update the azure-pipelines.[environment].yml file with the appropriate variable values, and values enclosed in square brackets
6. Create a pipeline in azure devops with the azure-pipelines.[environment].yml file
7. Ensure you have Continuous deployment enabled in the azure portal for your web app

# Creating a Project Based on Boilerplate

1. Initialize project repository
2. Clone project repository to your machine
3. Add the boilerplate origin with `git remote add https://github.com/KDG-Development/Net-React-Boilerplate.git boilerplate`
4. Prevent accidental pushes to boilerplate origin with `git remote set-url boilerplate --push "do not push"`
5. Pull boilerplate code into project repository with `git pull boilerplate [boilerplate repository branch]`
6. Push boilerplate code to project origin with `git push`
7. Follow steps 3-5 under "Getting Started" in project repository.
8. Include a note in project documentation to follow steps 3-5 in this section while setting up the project on a new machine.

## Additional DevTools

Additional [DevTools](https://github.com/KDG-Development/KDG-Net-DevTools) are available to assist with development when using this boilerplate

1. At the root of the project, run `mkdir DevTools` if the folder doesnt exist
1. Install with `git clone https://github.com/KDG-Development/KDG-Net-DevTools ./DevTools`
2. See the [README](https://github.com/KDG-Development/KDG-Net-DevTools/blob/main/README.md) for more information


# Staying up to date

This boilerplate is designed to be both a starting point and an ever-evolving foundation for your .NET/React applications. This section is applicable both for developing features directly for the boilerplate, as well as project repositories.

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