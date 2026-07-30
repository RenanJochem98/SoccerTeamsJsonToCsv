using System.Text;
using System.Text.Json;

namespace SoccerTeamsJsonToCsv;

internal class JsonlToCsvConverterprivate
{
    const int BufferSize = 64 * 1024; // Buffer de 64KB para leitura

    public static async Task ProcessJsonStreamAsync(string inputJsonlPath, string csvPath1, string csvPath2)
    {
        // 1. Configurar Streams com FileOptions.SequentialScan para o SO otimizar o I/O
        await using var inputStream = new FileStream(
            inputJsonlPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: BufferSize,
            options: FileOptions.SequentialScan | FileOptions.Asynchronous);

        await using var writer1 = new StreamWriter(csvPath1, append: false, Encoding.UTF8, bufferSize: BufferSize);
        await using var writer2 = new StreamWriter(csvPath2, append: false, Encoding.UTF8, bufferSize: BufferSize);

        // Escrever cabeçalhos dos CSVs
        await writer1.WriteLineAsync("Id do Clube,Nome,Campeonato,Data de Fundação,Cidade,Estado,País,Estádio,Presidente,Apelido,Cores");
        await writer2.WriteLineAsync("Id do Clube,Id do Jogador,Nome,Idade,Gols,Data de Estreia,Posição,Número da Camisa");

        byte[] buffer = new byte[BufferSize];
        int bytesRead = 0;
        int bytesInBuffer = 0;
        long totalConsumed = 0;

        JsonReaderState jsonState = default;

        while ((bytesRead = await inputStream.ReadAsync(buffer.AsMemory(bytesInBuffer, buffer.Length - bytesInBuffer))) > 0 || bytesInBuffer > 0)
        {
            bytesInBuffer += bytesRead;
            var reader = new Utf8JsonReader(buffer.AsSpan(0, bytesInBuffer), isFinalBlock: bytesRead == 0, jsonState);

            while (reader.Read())
            {
                // Identifica o início de um Objeto JSON
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    // Transforma o fragmento do objeto atual em um JsonDocument para extração rápida
                    using (var doc = JsonDocument.ParseValue(ref reader))
                    {
                        ProcessAndWriteObject(doc.RootElement, writer1, writer2);
                    }
                }

                // Salva o número de bytes consumidos para ajustar o buffer
                totalConsumed = reader.BytesConsumed;
            }

            // Preserva estado do parser para a próxima leitura do Stream
            jsonState = reader.CurrentState;

            // Remaneja os bytes não consumidos no buffer para o início
            int unconsumedBytes = bytesInBuffer - (int)totalConsumed;
            if (unconsumedBytes > 0)
            {
                Array.Copy(buffer, totalConsumed, buffer, 0, unconsumedBytes);
            }
            bytesInBuffer = unconsumedBytes;
        }
    }

    
    private static void ProcessAndWriteObject(JsonElement root, StreamWriter writerClubes, StreamWriter writerJogadores)
    {
        // -------------------------------------------------------------
        // 1. Extração dos Dados do Clube (CSV 1)
        // -------------------------------------------------------------
        string clubId = root.GetPropertyOrEmpty("club_id");
        string name = root.GetPropertyOrEmpty("name");
        string championship = root.GetPropertyOrEmpty("championship");
        string foundingDate = root.GetPropertyOrEmpty("founding_date");
        string city = root.GetPropertyOrEmpty("city");
        string state = root.GetPropertyOrEmpty("state");
        string country = root.GetPropertyOrEmpty("country");
        string stadium = root.GetPropertyOrEmpty("stadium");
        string president = root.GetPropertyOrEmpty("president");
        string nickname = root.GetPropertyOrEmpty("nickname");
        int titles = root.TryGetProperty("titles", out var titlesProp) ? titlesProp.GetInt32() : 0;

        // Trata o array de cores simples ["preto", "branco"] concatenando em uma única string
        string colorsFormatted = "";
        if (root.TryGetProperty("colors", out var colorsProp) && colorsProp.ValueKind == JsonValueKind.Array)
        {
            var colorsList = new List<string>();
            foreach (var colorElement in colorsProp.EnumerateArray())
            {
                colorsList.Add(colorElement.GetString() ?? "");
            }
            colorsFormatted = string.Join("|", colorsList);
        }

        // Grava a linha do Clube
        writerClubes.WriteLine($"{clubId},{EscapeCsv(name)},{EscapeCsv(championship)},{foundingDate},{EscapeCsv(city)},{state},{EscapeCsv(country)},{EscapeCsv(stadium)},{EscapeCsv(president)},{EscapeCsv(nickname)},{EscapeCsv(colorsFormatted)},{titles}");

        // -------------------------------------------------------------
        // 2. Extração dos Jogadores do Clube (CSV 2)
        // -------------------------------------------------------------
        if (root.TryGetProperty("players", out var playersProp) && playersProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var player in playersProp.EnumerateArray())
            {
                string playerId = player.GetPropertyOrEmpty("player_id");
                string playerName = player.GetPropertyOrEmpty("name");
                int age = player.TryGetProperty("age", out var ageProp) ? ageProp.GetInt32() : 0;
                int goals = player.TryGetProperty("goals", out var goalsProp) ? goalsProp.GetInt32() : 0;
                string debutDate = player.GetPropertyOrEmpty("debut_date");
                string position = player.GetPropertyOrEmpty("position");
                int shirtNumber = player.TryGetProperty("shirt_number", out var shirtProp) ? shirtProp.GetInt32() : 0;
                string nationality = player.GetPropertyOrEmpty("nationality");
                decimal marketValue = player.TryGetProperty("market_value", out var valProp) ? valProp.GetDecimal() : 0m;

                // Grava o jogador no CSV relacionando com o club_id
                writerJogadores.WriteLine($"{clubId},{playerId},{EscapeCsv(playerName)},{age},{goals},{debutDate},{EscapeCsv(position)},{shirtNumber},{EscapeCsv(nationality)},{marketValue}");
            }
        }
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}

public static class JsonElementExtensions
{
    public static string GetPropertyOrEmpty(this JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
        {
            return prop.GetString() ?? "";
        }
        return "";
    }
}
