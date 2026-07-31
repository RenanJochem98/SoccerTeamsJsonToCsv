using SoccerTeamsJsonToCsv;

try
{
    //...\SoccerTeamsJsonToCsv\bin\Debug\net10.0\ para ...\SoccerTeamsJsonToCsv 
    string? appPath = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;

    ArgsAndOutputValidator.ValidateAppPath(appPath);

    string? outputCsvPath = Path.Combine(appPath!, "OutputCsv");

    ArgsAndOutputValidator.ValidateOutputDirectoryAndCreateIfNotExists(outputCsvPath);

    Console.WriteLine("Iniciando processamento...");

    string jsonlFilePath = Path.Combine(appPath!, "sample_clubes.jsonl");
    string clubsCsvFilePath = Path.Combine(outputCsvPath, "clubs.csv");
    string playersCsvFilePath = Path.Combine(outputCsvPath, "players.csv");

    IEnumerable<string> championships = ["SERIE A", "SERIE B"];

    if (args.Length == 0)
        Console.WriteLine($"Nenhum argumento fornecido. Utilizando valores padrão: JsonlFilePath: \"{jsonlFilePath}\", Championships: {string.Join(", ", championships)}");
    else
    {
        jsonlFilePath = args[0].Trim();

        if (args.Length > 1 && !string.IsNullOrEmpty(args[1]))
            championships = args[1].Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p));
    }

    ArgsAndOutputValidator.ValidateJsonlInputPathArgument(jsonlFilePath);

    championships = championships.Select(p => p.ToUpper());

    Console.WriteLine($"Utilizando valores: JsonlFilePath: \"{jsonlFilePath}\", Championships: {string.Join(", ", championships)}");

    JsonlToCsvConverter.ProcessJsonStreamAsync(jsonlFilePath, clubsCsvFilePath, playersCsvFilePath, championships.ToList()).GetAwaiter().GetResult();

    Console.WriteLine($"O arquivo clubs.csv foi gerado no caminho: {clubsCsvFilePath}");
    Console.WriteLine($"O arquivo players.csv foi gerado no caminho: {playersCsvFilePath}");
}
catch (Exception ex)
{
    Console.WriteLine($"Ocorreu um erro: {ex.Message}");
}
finally
{
    Console.WriteLine("Processamento finalizado.");
}
