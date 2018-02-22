# cucumber-pro-plugin-specflow

[![Build status](https://gasparnagy.visualstudio.com/_apis/public/build/definitions/dc4f6ce1-e00f-4c7d-98fd-9397bf9a4281/43/badge)](https://gasparnagy.visualstudio.com/cucumber-pro-specflow-plugin/_build/index?context=allDefinitions&path=%5C&definitionId=43&_a=completed)

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
    [ ] readme.txt of the NuGet package
    [ ] icon, texts, etc of NuGet package
    [x] Use single json generator
    [ ] TFS Build Line++ issue
    [x] Send revision as part of json
    [ ] Replace Cucumber.Java with SpecFlow content type
    [ ] Change error message about GIT BRANCH setting, it should only recommend ENV
    [ ] Why is it working without sending SHA
    [ ] How to specify GIT COMMIT (SHA) and BRANCH locally
    [ ] Remove explicit opt-in for local publishing
    [x] Use BUILD_SOURCEVERSION for TFS for revision
    [ ] Remove smoke test token from code - move it to CI config
    [ ] make profile name as a top level config setting
    [ ] make info as default log level
    [ ] use config file w/o . by default, but allow one with . as well
    [ ] add json samples to cucumber-json-testdata-generator
    [ ] make INFO as default log level
    [ ] Cannot override file-based config setting with ENV -> change precedence
    [ ] Document release process

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
