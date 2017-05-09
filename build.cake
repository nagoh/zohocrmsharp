#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

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
    MSBuild("./src/Deveel.ZohoCRM.sln", settings =>
        settings.SetConfiguration(configuration));
});

Task("Package")
	.IsDependentOn("Build")
	.Does(() => 
	{
		  var nuGetPackSettings   = new NuGetPackSettings {
			OutputDirectory         = "./artefacts"
		  };

		  NuGetPack("./src/Deveel.ZohoCRM/Deveel.ZohoCRM.nuspec", nuGetPackSettings);
	});

Task("Push")
	.IsDependentOn("Package")
	.Does(() => {
		var apiKey = BuildSystem.TeamCity.Environment.GetEnvironmentString("NugetApiKey");
		var nugetPath = BuildSystem.TeamCity.Environment.GetEnvironmentString("NugetUrl");

		 NuGetPush("./artefacts/PartPay.ZohoCrm.1.0.0.nupkg", new NuGetPushSettings {
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