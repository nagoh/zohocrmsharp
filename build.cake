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
	if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("build.number"))) {
		version	= EnvironmentVariable("build.number");
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
		var apiKey = Environment.GetEnvironmentVariable("env.NugetApiKey");
		var nugetPublishUrl = Environment.GetEnvironmentVariable("env.NugetUrl");

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