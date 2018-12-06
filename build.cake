#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = "1.0.0";
var apiKey = Argument("NugetApiKey", "");
var nugetPublishUrl = Argument("NugetUrl", "");
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

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testAssemblies = GetFiles("./src/**/bin/" + configuration + "/*.Tests.dll");
	NUnit3(testAssemblies, new NUnit3Settings{
		Where = "cat != Integration"
	});
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
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);