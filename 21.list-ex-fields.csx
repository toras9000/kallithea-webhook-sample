#r "nuget: Lestaly, 0.51.0"
#r "nuget: KallitheaApiClient, 0.7.0-lib.23.private.1"
#nullable enable
using KallitheaApiClient;
using KallitheaApiClient.Utils;
using Lestaly;

// This script is meant to run with dotnet-script (v1.5 or lator).
// You can install .NET SDK 8.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

var settings = new
{
    // Service URL
    ServiceUrl = new Uri(@"http://localhost:8800/"),

    // API key
    ApiKey = "1111222233334444555566667777888899990000",
};

await Paved.RunAsync(config: o => o.AnyPause(), action: async () =>
{
    using var signal = ConsoleWig.CreateCancelKeyHandlePeriod();

    // create API client
    using var client = new SimpleKallitheaClient(new Uri(settings.ServiceUrl, "/_admin/api"), settings.ApiKey);

    // list repositories
    Console.WriteLine("List repository extra fields");
    var repos = await client.GetReposAsync(signal.Token);
    foreach (var repo in repos)
    {
        Console.WriteLine($"  {repo.repo_name}");
        if (0 < repo.extra_fields?.Length)
        {
            foreach (var field in repo.extra_fields)
            {
                Console.WriteLine($"    {field.key} = {field.value}");
            }
        }
        else
        {
            Console.WriteLine($"    ... No extra fields");
        }
    }

});
