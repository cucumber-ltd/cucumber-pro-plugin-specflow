# cucumber-pro-plugin-specflow

[![Build status](https://gasparnagy.visualstudio.com/_apis/public/build/definitions/dc4f6ce1-e00f-4c7d-98fd-9397bf9a4281/43/badge)](https://gasparnagy.visualstudio.com/cucumber-pro-specflow-plugin/_build/index?context=allDefinitions&path=%5C&definitionId=43&_a=completed)

# TODO

    [ ] Handle results on tags (instead of branches)
    [ ] Publish through a separate tool
    [x] Configure profile
    [ ] Allow specifying custom config file
    [x] Only publish on CI, except forced
    [x] Automated smoke test
    [x] NuGet package
    [x] Release script
    [x] TFS Support
    [x] Demo project
    [x] Verbose logging
    [ ] Demo
    [ ] Support scenario outlines
    [x] Support undefined/skipped/pending(?) result
    [ ] Support background(?)
    [ ] Support hooks (errors in hooks)
    [ ] Support step timings
    [x] Support parallel run
    [x] Convert feature file paths to relative
    [ ] readme.txt of the NuGet package
    [ ] icon, texts, etc of NuGet package
    [x] Configure results.json file
    [x] Use temp results.json file by default
    [ ] TFS Build Line++ issue
    [ ] Replace Cucumber.Java with SpecFlow content type
    [ ] Setup myget for build results

# Questions

- How to configure profile (now: from config file)
- Should we use INFO as default log level?
- How to configure the case, when you want to publish the results to a file, but not send them up?
- Setting cucumberpro.connection.ignoreerror cannot be applied
- Cannot override file-based config setting with ENV
- How to release, how to release prelim versions?
- Max length for error message, entire json?
- Including skipped scenario steps (after a failing step): Is this necessary - we don't have them by default.

# Known Issues

- No line number for undefined steps
