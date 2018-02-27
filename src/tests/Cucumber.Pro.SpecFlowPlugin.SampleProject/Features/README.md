# SpecFlow plugin for Cucumber Pro

This SpecFlow plugin publishes test results to Cucumber Pro.

## Requirements

Your SpecFlow project must be stored in a Git repository.

It's recommended (but not required) that you use a CI server to publish the test results.
Any CI server works. If you use on of the following CI servers, some of the configuration
will be automatic.

* TFS/VSTS
* Bamboo
* Circle CI
* Jenkins
* Travis
* Wercker

## Installation

### Install plugin from NuGet into your SpecFlow project.

    PM> Install-Package Cucumber.Pro.SpecFlowPlugin

### Configure

The majority of the settings have usable defaults or can be detected from the CI server. You should however set the following configuration settings:

* `projectname`: The name of the Cucumber Pro project you send the results to
* `token`: The security token for publishing the results
* `url`: The URL of the Cucumber Pro installation, if you are using a privately hosted Cucumber Pro appliance

The configurations can be specified by setting environment variables or using a configuration file. See configuration details below.

### Profiles

If you run SpecFlow test several times as part of your build (with different options, perhaps different tags), you can
specify a different *profile name* for each run. This allows Cucumber Pro to show separate results for each profile.

The profile name is specified using the configuration file or environment variable.

## Configuration

The default configuration of the plugin is as follows:

```yaml
cucumberpro:
  # The plugin sends your local environment variables to Cucumber Pro so it can detect the CI build number, 
  # git branch/tag and other information about the build. This mask is a regular expression for filtering
  # out sensitive values that should not be sent to Cucumber Pro.
  envmask: SECRET|KEY|TOKEN|PASSWORD|PWD

  # Sets the log level to one of `DEBUG`, `INFO`, `WARN`, `ERROR` or `FATAL`. Defaults to `INFO`.
  # Setting it to `DEBUG` will also print the current configuration when the plugin runs.
  logging: 

  # Writes out the log messages to the specified file. 
  # The file path can be absolute or relative to the test assembly. Environment variables can also
  # be used with the syntax: %TEMP%\results.json.
  logfile:

  # Not needed if the build is running on a preferred CI server and the Cucumber Pro project 
  # name is identical to the CI server project name. Define this environment variable to override 
  # the project name.
  projectname: 

  # Sets the execution profile for publishing the results. If omitted the value "default" is used.
  # When the same tests are executed in different configuration, you should override the profile name
  # by setting the `CUCUMBERPRO_PROFILE` environment variable on the build server.
  profile:

  # (This configuration can be skipped for private Cucumber Pro appliance installations where results publishing is open).
  # Results are published to Cucumber Pro using HTTP/HTTPS. Each Cucumber Pro project has a token for this purpose.
  # You can find it in the project settings (press `?` to display it).
  # This token should be assigned to a `CUCUMBERPRO_TOKEN` environment variable on the build server, on a per-project basis.
  # Consult your CI server's documentation for details about defining per-project environment variables.
  # Some CI servers such as Travis and Circle CI allow you to define environment variables in a file checked into git.
  # *DO NOT DO THIS* - as it would allow anyone with read acceess to your repository to publish results.
  token: 

  # Override this if you are using a privately hosted Cucumber Pro appliance.
  # We recommend setting this with a CUCUMBERPRO_URL environment variable defined globally on your build server.
  url: https://app.cucumber.pro/

  connection:
    # If a http or ssh connection takes longer than this (milliseconds), time out the connection.
    timeout: 5000

  results:
    # Saves the test results JSON file to the specified file path. If omitted the results will be
    # published wihtout saving them to a file.
    # The file path can be absolute or relative to the test assembly. Environment variables can also
    # be used with the syntax: %TEMP%\results.json.
    file:
```

Depending on your environment you will have to override some of the defaults, or specify some of the
settings that don't have a default value.

The simplest way to override the the default configuration file added to your project by the NuGet package.

The plugin will find files `cucumberpro.yml` or `.cucumberpro.yml` in the project folder or in the folders above.

You can also create a `cucumber.yml` file in your `HOME` directory to define global settings.

Every setting can also be overridden with environment variables.

For example, if you want to increase logging:

```
# Linux / OS X
export CUCUMBERPRO_LOGGING=DEBUG

# Windows
SET CUCUMBERPRO_LOGGING=DEBUG
```

## Activating the plugin

The plugin will only attempt to publish results if it detects that it's running in a CI environment. The plugin
detects a CI environment by checking environment variables for well-known CI servers.

If you want to activate the plugin from a regular work station you can define the following environment variables:

* `GIT_COMMIT` - you can find it by running `git rev-parse HEAD`
* `GIT_BRANCH` - you can find it by running `git rev-parse --abbrev-ref HEAD`
