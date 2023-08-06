#r "nuget: System.Data.SQLite.Core, 1.0.118"
#r "nuget: Dapper, 2.1.24"
#r "nuget: Lestaly, 0.51.0"
#nullable enable
using System.Data.SQLite;
using Dapper;
using Lestaly;

// This script is meant to run with dotnet-script (v1.5 or lator).
// You can install .NET SDK 8.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// 

return await Paved.RunAsync(async () =>
{
    // check db file.
    var dbFile = ThisSource.RelativeFile("./docker/data/config/kallithea.db");
    if (!dbFile.Exists) throw new PavedMessageException("db file not found.");

    // Force update of admin's API key. 
    Console.WriteLine("Turn on the extra field setting..");
    var db_settings = new SQLiteConnectionStringBuilder();
    db_settings.DataSource = dbFile.FullName;
    db_settings.FailIfMissing = true;
    using var db = new SQLiteConnection(db_settings.ConnectionString);
    var field_setting = await db.ExecuteScalarAsync($"select app_settings_id from settings where app_settings_name = 'repository_fields'");
    if (field_setting == null)
    {
        await db.ExecuteAsync("insert into settings(app_settings_name, app_settings_value, app_settings_type) values ('repository_fields', 'True', 'bool')");
    }
    else
    {
        await db.ExecuteAsync($"update settings set app_settings_value = 'True' where app_settings_name = 'repository_fields'");
    }
    Console.WriteLine("...Success");

});
