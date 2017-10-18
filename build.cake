var target = Argument<string>("target", "Default");
var solutionPath = "./CIE.MRTD.SDK.sln";

Task("Restore")
	.Does(() => {
		DotNetCoreRestore(".");
	});

Task("Build")
	.IsDependentOn("Restore")
	.Does(() => {
		MSBuild(solutionPath, new MSBuildSettings 
		{
			Verbosity = Verbosity.Minimal,
			ToolVersion = MSBuildToolVersion.VS2015,
			Configuration = "Release",
			PlatformTarget = PlatformTarget.MSIL
		});
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() => {
		//DotNetCoreTest("./src/tests/Specify.Tests");
		//DotNetCoreTest("./src/tests/Specify.IntegrationTests");
		//DotNetCoreTest("./src/Samples/Specify.Samples");
	});

Task("Default")
	.IsDependentOn("Test");

RunTarget(target);
