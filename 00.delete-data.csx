#r "nuget: Lestaly, 0.51.0"
#nullable enable
using System.Net.Http;
using System.Threading;
using Lestaly;
using Lestaly.Cx;

// This script is meant to run with dotnet-script (v1.5 or lator).
// You can install .NET SDK 8.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// Delete container persistent data.

var settings = new
{
    // Compose dir
    ComposeDir = ThisSource.RelativeDirectory("./docker"),

    // local working base dir
    LocalWorkDir = ThisSource.RelativeDirectory("work"),
};

await Paved.RunAsync(async () =>
{
    var composeFile = settings.ComposeDir.RelativeFile("docker-compose.yml");
    Console.WriteLine("Stop service");
    await "docker".args("compose", "--file", composeFile.FullName, "down", "--remove-orphans", "--volumes").silent();

    Console.WriteLine("Delete data");
    var dataDir = settings.ComposeDir.RelativeDirectory("data");
    if (dataDir.Exists) { dataDir.DoFiles(c => c.File?.SetReadOnly(false)); dataDir.Delete(recursive: true); }

    var workDir = settings.LocalWorkDir;
    if (workDir.Exists) { workDir.DoFiles(c => c.File?.SetReadOnly(false)); workDir.Delete(recursive: true); }
});
