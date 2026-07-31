using SoccerTeamsJsonToCsv;

//...\SoccerTeamsJsonToCsv\bin\Debug\net10.0\ para ...\SoccerTeamsJsonToCsv 
string? appPath = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;

if (string.IsNullOrEmpty(appPath))
{
    Console.WriteLine("Não foi possível determinar o caminho da aplicação.");
    return;
}

Console.WriteLine("Iniciando processamento...");

string jsonlFilePath = Path.Combine(appPath, "sample_clubes.jsonl");
string clubsCsvFilePath = Path.Combine(appPath, "OutputCsv", "clubs.csv");
string playersCsvFilePath = Path.Combine(appPath, "OutputCsv", "players.csv");

IEnumerable<string> championships = ["SERIE A", "SERIE B"];

if (args.Length == 0)
{
    Console.WriteLine($"Nenhum argumento fornecido. Utilizando valores padrão.: JsonlFilePath: \"{jsonlFilePath}\", Championships: {string.Join(", ", championships)}");
}
else
{
    jsonlFilePath = args[0].Trim();

    if (args.Length > 1 && !string.IsNullOrEmpty(args[1]))
    {
        championships = args[1].Split(',').Select(p => p.Trim());
    }
}

try
{
    championships = championships.Select(p => p.ToUpper());

    Console.WriteLine($"Utilizando valores: JsonlFilePath: \"{jsonlFilePath}\", Championships: {string.Join(", ", championships)}");

    JsonlToCsvConverterprivate.ProcessJsonStreamAsync(jsonlFilePath, clubsCsvFilePath, playersCsvFilePath, championships.ToList()).GetAwaiter().GetResult();
}
catch (Exception ex)
{
    Console.WriteLine($"Ocorreu um erro: {ex.Message}");
}
finally
{
    Console.WriteLine("Processamento finalizado.");
    Console.WriteLine($"O arquivo clubs.csv foi gerado no caminho: {clubsCsvFilePath}");
    Console.WriteLine($"O arquivo players.csv foi gerado no caminho: {playersCsvFilePath}");
}