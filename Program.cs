using SoccerTeamsJsonToCsv;

Console.WriteLine("Iniciando processamento...");

string jsonlFilePath = @"C:\Users\renan\Programacao\TesteTecnico\SoccerTeamsJsonToCsv\sample_clubes.jsonl";
string clubsCsvFilePath = @"C:\Users\renan\Programacao\TesteTecnico\SoccerTeamsJsonToCsv\OutputCsv\clubs.csv";
string playersCsvFilePath = @"C:\Users\renan\Programacao\TesteTecnico\SoccerTeamsJsonToCsv\OutputCsv\players.csv";

JsonlToCsvConverterprivate.ProcessJsonStreamAsync(jsonlFilePath, clubsCsvFilePath, playersCsvFilePath).GetAwaiter().GetResult();

Console.WriteLine("Processamento concluído.");
