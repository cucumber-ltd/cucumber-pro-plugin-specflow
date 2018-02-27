# cucumber-pro-plugin-specflow

[![Build status](https://cucumberbdd.visualstudio.com/_apis/public/build/definitions/3f6ffcb6-fe64-4a4f-85bd-eed3893f83fd/1/badge)](https://cucumberbdd.visualstudio.com/cucumber-pro-plugin-specflow/_build/index?context=mine&path=%5C&definitionId=1&_a=completed)

# Run tests

    mono packages/xunit.runner.console.2.3.1/tools/net452/xunit.console.exe tests/Cucumber.Pro.SpecFlowPlugin.Tests/bin/Debug/Cucumber.Pro.SpecFlowPlugin.Tests.dll

# TODO

    [x] Handle results on tags (instead of branches)
    [x] Configure profile
    [x] Only publish on CI, except forced
    [x] Automated smoke test
    [x] NuGet package
    [x] Release script
    [x] TFS Support
    [x] Demo project
    [x] Verbose logging
    [x] Demo
    [x] Support scenario outlines
    [x] Support undefined/skipped/pending(?) result
    [x] Support background(?)
    [x] Support hooks (errors in hooks)
    [x] Support step timings
    [x] Support parallel run
    [x] Convert feature file paths to relative
    [x] Configure results.json file
    [x] Use temp results.json file by default
    [x] Build NuGet from CI
    [x] Use single json generator
    [x] Send revision as part of json
    [x] Change error message about GIT BRANCH setting, it should only recommend ENV
    [x] Why is it working without sending SHA
    [x] How to specify GIT COMMIT (SHA) and BRANCH locally
    [x] Use BUILD_SOURCEVERSION for TFS for revision
    [x] Fix file saving - save entire json message
    [x] make profile name as a top level config setting
    [x] make info as default log level
    [x] use config file w/o . by default, but allow one with . as well
    [x] make INFO as default log level
    [x] Cannot override file-based config setting with ENV -> change precedence
    [x] readme.txt of the NuGet package
    [x] Remove explicit opt-in for local publishing
    [x] Add basic documentation
    [x] Remove smoke test token from code - move it to CI config
    [x] Configure CI on cucumberbdd.visualstudio.com
    [ ] icon, texts, etc of NuGet package
    [ ] TFS Build Line++ issue
    [ ] Replace Cucumber.Java with SpecFlow content type
    [ ] Document release process
    [ ] Support .NET Core (currently only .NET 4.6 is supported, which is what SpecFlow 2.3.0 supports)
    [ ] add json samples to cucumber-json-testdata-generator
    [ ] CI build with multiple versions:
      [ ] SpecFlow version (e.g. 2.2.1 and 2.3.0)
      [ ] JSON.net version
      [ ] Yaml version
    [ ] Make tests run on OSX/Linux (currently fails with newline issues, and hangs)

# Questions

# Differences from the Java Plugin

- Profile can be configured from the config file (and not at the plugin configuration)
- Default log level is INFO
- Setting cucumberpro.connection.ignoreerror is not used (the event where SpecFlow sends the messages swallowes the errors anyway)
- Environment variables have precedence over the config file settings (in order to be override them from build config)

# Known Issues

- Background steps are reported as normal steps, but with a line number pointing to the background step. Ok?
- Including skipped scenario steps (after a failing step): Is this necessary - we don't have them by default.
- No line number for undefined steps
- Line number for Scenario Outline examples is pointing to the SO header line, type="scenario"
- Hook errors are not captured

# Ideas for later improvements

* Publish through a separate tool
* Allow specifying custom config file
* Publish results to a file, but not send them to CPro
