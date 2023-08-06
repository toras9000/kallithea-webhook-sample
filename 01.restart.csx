#r "nuget: Lestaly, 0.51.0"
#nullable enable
using System.Net.Http;
using System.Threading;
using Lestaly;
using Lestaly.Cx;

// This script is meant to run with dotnet-script (v1.5 or lator).
// You can install .NET SDK 8.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// Restart docker container.
// (If it is not activated, it is simply activated.)

var settings = new
{
    // Compose dir
    ComposeDir = ThisSource.RelativeDirectory("./docker"),

    // Service URL
    ServiceUrl = @"http://localhost:8800/",

    // Whether to open the URL after the UP.
    LaunchAfterUp = true,
};

await Paved.RunAsync(async () =>
{
    try
    {
        var composeFile = settings.ComposeDir.RelativeFile("docker-compose.yml");
        Console.WriteLine("Stop service");
        await "docker".args("compose", "--file", composeFile.FullName, "down", "--remove-orphans", "--volumes").silent();
        Console.WriteLine("Start service");
        await "docker".args("compose", "--file", composeFile.FullName, "up", "-d").silent().result().success();

        if (settings.LaunchAfterUp)
        {
            Console.WriteLine("Waiting for accessible ...");
            using var checker = new HttpClient();
            while ((await checker.TryGetAsync(new(settings.ServiceUrl))) == null) await Task.Delay(1000);
            Console.WriteLine("Launch site.");
            await CmdShell.ExecAsync(settings.ServiceUrl);
        }
    }
    catch (CmdProcExitCodeException err)
    {
        throw new PavedMessageException($"ExitCode: {err.ExitCode}\nOutput: {err.Output}", err);
    }
});
