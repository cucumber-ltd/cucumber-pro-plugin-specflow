# cucumber-pro-plugin-specflow

[![Build status](https://gasparnagy.visualstudio.com/_apis/public/build/definitions/dc4f6ce1-e00f-4c7d-98fd-9397bf9a4281/43/badge)](https://gasparnagy.visualstudio.com/cucumber-pro-specflow-plugin/_build/index?context=allDefinitions&path=%5C&definitionId=43&_a=completed)

# TODO

    [x] Handle results on tags (instead of branches)
    [ ] Publish through a separate tool -- later
    [x] Configure profile
    [ ] Allow specifying custom config file -- not needed
    [x] Only publish on CI, except forced
    [x] Automated smoke test
    [x] NuGet package
    [x] Release script
    [x] TFS Support
    [x] Demo project
    [x] Verbose logging
    [ ] Demo
    [x] Support scenario outlines
    [x] Support undefined/skipped/pending(?) result
    [x] Support background(?)
    [x] Support hooks (errors in hooks)
    [x] Support step timings
    [x] Support parallel run
    [x] Convert feature file paths to relative
    [ ] readme.txt of the NuGet package
    [ ] icon, texts, etc of NuGet package
    [x] Configure results.json file
    [x] Use temp results.json file by default
    [ ] TFS Build Line++ issue
    [ ] Replace Cucumber.Java with SpecFlow content type
    [x] Build NuGet from CI
    [ ] Change error message about GIT BRANCH setting, it should only recommend ENV
    [ ] Why is it working without sending SHA
    [ ] How to specify GIT COMMIT (SHA) and BRANCH locally
    [ ] Remove explicit opt-in for local publishing
    [ ] Use BUILD_SOURCEVERSION for TFS for revision
    [ ] Remove smoke test token from code - move it to CI config
    [ ] make profile name as a top level config setting
    [ ] make info as default log level
    [ ] use config file w/o . by default, but allow one with . as well
    [ ] add json samples to cucumber-json-testdata-generator

# Questions

+ How to configure profile (now: from config file) -> OK
+ Should we use INFO as default log level? -> OK
- How to configure the case, when you want to publish the results to a file, but not send them up?
+ Setting cucumberpro.connection.ignoreerror cannot be applied -> OK
+ Cannot override file-based config setting with ENV -> change precedence
+ How to release, how to release prelim versions?
+ Max length for error message, entire json? -> not now
+ Skip publishing for pull requests? -> will be handled on the server

# Known Issues

- Background steps are reported as normal steps, but with a line number pointing to the background step. Ok?
- Including skipped scenario steps (after a failing step): Is this necessary - we don't have them by default.
- No line number for undefined steps
- Line number for Scenario Outline examples is pointing to the SO header line, type="scenario"
- Hook errors are not captured
