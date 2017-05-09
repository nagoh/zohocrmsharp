#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = "1.0.0";

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Restore-NuGet-Packages")
    .Does(() =>
{
    NuGetRestore("./src/Deveel.ZohoCRM.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
	if (!string.IsNullOrEmpty(Argument("BuildNumber", ""))) {
		version	= Argument("BuildNumber", version);
	}
	
    MSBuild("./src/Deveel.ZohoCRM.sln", settings =>
        settings.SetConfiguration(configuration));
});

Task("Package")
	.IsDependentOn("Build")
	.Does(() => 
	{
		  var nuGetPackSettings = new NuGetPackSettings {
				OutputDirectory = "./artefacts",
				Version = version
		  };

		  NuGetPack("./src/Deveel.ZohoCRM/Deveel.ZohoCRM.nuspec", nuGetPackSettings);
	});

Task("Push")
	.IsDependentOn("Package")
	.Does(() => {
		var apiKey = Argument("NugetApiKey", "");
		var nugetPublishUrl = Argument("NugetUrl", "");

		if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(nugetPublishUrl))
			return;

		 NuGetPush("./artefacts/PartPay.ZohoCrm." + version + ".nupkg", new NuGetPushSettings {
			 Source = nugetPublishUrl,
			 ApiKey = apiKey
		 });
	});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);