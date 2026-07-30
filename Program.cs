using SoccerTeamsJsonToCsv;

Console.WriteLine("Iniciando processamento...");

JsonlToCsvConverterprivate.ProcessJsonStreamAsync("sample_clubes.jsonl", "clubs.csv", "players.csv").GetAwaiter().GetResult();

Console.WriteLine("Processamento concluído.");
