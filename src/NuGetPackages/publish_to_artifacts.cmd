@pushd %~dp0

@if "%NUGET_LOCAL_FEED%" == "" (
	@echo NUGET_LOCAL_FEED environment variable is not defined!
	@exit /b
)

set NUGET_LOCAL_FEED=%1
set VERSION=%2
set PKGVER=%2%3
set CONFIG="/p:Configuration=Release"
set ASSEMBLYINFO=Cucumber.Pro.SpecFlowPlugin\Properties\AssemblyInfo.cs

@echo publishing version %VERSION%, pkg version %PKGVER%, %CONFIG%, OK?

cd ..

copy /Y %ASSEMBLYINFO% %ASSEMBLYINFO%.bak
powershell -Command "(gc '%ASSEMBLYINFO%') -replace '1.0.0-localdev', '%PKGVER%' | Out-File '%ASSEMBLYINFO%'"
powershell -Command "(gc '%ASSEMBLYINFO%') -replace '1.0.0.0', '%VERSION%.0' | Out-File '%ASSEMBLYINFO%'"

msbuild Cucumber.Pro.SpecFlowPlugin.sln %CONFIG%

cd NuGetPackages

msbuild NuGetPackages.csproj "/p:NuGetVersion=%PKGVER%" /p:NugetPublishToLocalNugetFeed=true /t:Publish /p:NugetPublishLocalNugetFeedFolder=%NUGET_LOCAL_FEED%  %CONFIG%

cd ..
move /Y %ASSEMBLYINFO%.bak %ASSEMBLYINFO%

@popd
