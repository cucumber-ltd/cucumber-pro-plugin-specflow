# Release Process

## Overview

The plugin is published as a [nuget.org](https://www.nuget.org/) package.

[VSTS](https://cucumberbdd.visualstudio.com/cucumber-pro-plugin-specflow) builds a
nuget package for every successful build. 

Publishing a nuget package is a semi-automatic process. Tagging the git repo 
after a release is manual.

## Making a release

Go to the [build](https://cucumberbdd.visualstudio.com/cucumber-pro-plugin-specflow/_build/index?context=mine&path=%5C&definitionId=1&_a=completed) page and find the build you want to release.

Press the `Queue new build...` button. This should pop up a new dialogue.
Change the following fields:

* `PluginVersion` - whatever the new release should be
* `PluginVersionSuffix` - clear this field (for a release package) or set it to something like `-pre20180227a` for prerelease packages (NuGet users only see release packages when they install packages unless they check the "prerelease" checkbox).
* `NUGET_PUSH` - set this to `true`

Press `OK`. When the build is finished there should be a new nuget package
[here](https://www.nuget.org/packages/Cucumber.Pro.SpecFlowPlugin).
(You may have to wait about half an hour before it's available).

## Tagging a release

After you've made a successful release, update `CHANGELOG.md`:

* Remove any empty sections for the released version. 
* Update diff links at the bottom of the file.
* Commit everything.

Finally create a tag in git:

    git tag vX.Y.Z
    git push && git push --tags
