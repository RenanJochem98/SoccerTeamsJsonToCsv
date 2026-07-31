using SoccerTeamsJsonToCsv;

Console.WriteLine("Iniciando processamento...");

string jsonlFilePath = @"C:\Users\renan\Programacao\TesteTecnico\SoccerTeamsJsonToCsv\sample_clubes.jsonl";
string clubsCsvFilePath = @"C:\Users\renan\Programacao\TesteTecnico\SoccerTeamsJsonToCsv\OutputCsv\clubs.csv";
string playersCsvFilePath = @"C:\Users\renan\Programacao\TesteTecnico\SoccerTeamsJsonToCsv\OutputCsv\players.csv";

try
{
    List<string> championships = new() { "SERIE a", "SERIE b" };

    championships = championships.Select(p => p.ToUpper()).ToList();
    JsonlToCsvConverterprivate.ProcessJsonStreamAsync(jsonlFilePath, clubsCsvFilePath, playersCsvFilePath, championships).GetAwaiter().GetResult();
}
catch (Exception ex)
{
    Console.WriteLine($"Ocorreu um erro: {ex.Message}");
}
finally
{
    Console.WriteLine("Processamento finalizado.");
}   