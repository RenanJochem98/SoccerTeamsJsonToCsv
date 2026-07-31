using System.Text;
using System.Text.Json;

namespace SoccerTeamsJsonToCsv;

internal class JsonlToCsvConverter
{
    const int BufferSize = 64 * 1024; // Buffer de 64KB para leitura

    public static async Task ProcessJsonStreamAsync(string inputJsonlPath, string csvPathClubs, string csvPathPlayers, HashSet<string> championshipsFilter)
    {
        // 1. Configurar Streams com FileOptions.SequentialScan para o SO otimizar o I/O
        await using var inputStream = new FileStream(
            inputJsonlPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: BufferSize,
            options: FileOptions.SequentialScan | FileOptions.Asynchronous);

        await using var writerClubs = new StreamWriter(csvPathClubs, append: false, Encoding.UTF8, bufferSize: BufferSize);
        await using var writerPlayers = new StreamWriter(csvPathPlayers, append: false, Encoding.UTF8, bufferSize: BufferSize);

        // Escrever cabeçalhos dos CSVs
        await writerClubs.WriteLineAsync("Id do Clube,Nome,Campeonato,Data de Fundação,Cidade,Estado,País,Estádio,Presidente,Apelido,Cores,");
        await writerPlayers.WriteLineAsync("Id do Clube,Id do Jogador,Nome,Idade,Gols,Data de Estreia,Posição,Número da Camisa,");

        byte[] buffer = new byte[BufferSize];
        int bytesRead = 0;
        int bytesInBuffer = 0;
        long totalConsumed = 0;

        JsonReaderOptions readerOptions = new JsonReaderOptions
        {
            AllowMultipleValues = true,
            CommentHandling = JsonCommentHandling.Skip
        };

        JsonReaderState jsonState = new JsonReaderState(readerOptions); ;

        while ((bytesRead = await inputStream.ReadAsync(buffer.AsMemory(bytesInBuffer, buffer.Length - bytesInBuffer))) > 0 || bytesInBuffer > 0)
        {
            bytesInBuffer += bytesRead;

            var reader = ProcessAndWriteJsonObject(buffer, bytesInBuffer, bytesRead, jsonState, writerClubs, writerPlayers, championshipsFilter);

            // Salva o número de bytes consumidos para ajustar o buffer
            totalConsumed = reader.BytesConsumed;

            // Preserva estado do parser para a próxima leitura do Stream
            jsonState = reader.CurrentState;

            // Remaneja os bytes não consumidos no buffer para o início
            int unconsumedBytes = bytesInBuffer - (int)totalConsumed;
            if (unconsumedBytes > 0)
            {
                Array.Copy(buffer, totalConsumed, buffer, 0, unconsumedBytes);
            }
            bytesInBuffer = unconsumedBytes;

            // Se não houver mais bytes a serem lidos e o buffer estiver vazio, encerra o loop para evitar uma iteração desnecessária
            if (bytesRead == 0) break;
        }
    }

    private static Utf8JsonReader ProcessAndWriteJsonObject(byte[] buffer, int bytesInBuffer, int bytesRead, JsonReaderState jsonState, 
        StreamWriter writerClubes, StreamWriter writerJogadores, HashSet<string> championshipsFilter)
    {
        var reader = new Utf8JsonReader(buffer.AsSpan(0, bytesInBuffer), isFinalBlock: bytesRead == 0, jsonState);

        while (reader.Read())
        {
            // Identifica o início de um Objeto JSON
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Transforma o fragmento do objeto atual em um JsonDocument para extração rápida
                using (var doc = JsonDocument.ParseValue(ref reader))
                {
                    ProcessAndWriteClub(doc.RootElement, writerClubes, writerJogadores, championshipsFilter);
                }
            }
        }

        return reader;
    }

    private static void ProcessAndWriteClub(JsonElement root, StreamWriter writerClubes, StreamWriter writerJogadores, HashSet<string> championshipsFilter)
    {
        string championship = root.GetPropertyOrEmpty("championship");

        if (championshipsFilter.Contains(championship.ToUpper()))
        {
            string clubId = root.GetPropertyOrEmpty("club_id");
            string name = root.GetPropertyOrEmpty("name");

            string foundingDate = root.GetDateTimePropertyOrEmpty("founding_date");
            string city = root.GetPropertyOrEmpty("city");
            string state = root.GetPropertyOrEmpty("state");
            string country = root.GetPropertyOrEmpty("country");
            string stadium = root.GetPropertyOrEmpty("stadium");
            string president = root.GetPropertyOrEmpty("president");
            string nickname = root.GetPropertyOrEmpty("nickname");

            string colorsFormatted = ProcessClubColors(root, "colors", "|");            

            writerClubes.WriteLine($"{clubId},{EscapeCsv(name)},{EscapeCsv(championship)},{foundingDate},{EscapeCsv(city)},{state},{EscapeCsv(country)},{EscapeCsv(stadium)},{EscapeCsv(president)},{EscapeCsv(nickname)},{EscapeCsv(colorsFormatted)},");

            ProcessAndWritePlayers(root, writerJogadores, clubId);
        }
    }

    private static void ProcessAndWritePlayers(JsonElement root, StreamWriter writerJogadores, string clubId)
    {
        if (root.TryGetProperty("players", out var playersProp) && playersProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var player in playersProp.EnumerateArray())
            {
                string playerId = player.GetPropertyOrEmpty("player_id");
                string playerName = player.GetPropertyOrEmpty("name");
                int age = player.TryGetProperty("age", out var ageProp) ? ageProp.GetInt32() : 0;
                int goals = player.TryGetProperty("goals", out var goalsProp) ? goalsProp.GetInt32() : 0;
                string debutDate = player.GetDateTimePropertyOrEmpty("debut_date");
                string position = player.GetPropertyOrEmpty("position");
                int shirtNumber = player.TryGetProperty("shirt_number", out var shirtProp) ? shirtProp.GetInt32() : 0;

                // Grava o jogador no CSV relacionando com o club_id
                writerJogadores.WriteLine($"{clubId},{playerId},{EscapeCsv(playerName)},{age},{goals},{debutDate},{EscapeCsv(position)},{shirtNumber},");
            }
        }
    }

    private static string ProcessClubColors(JsonElement root, string propertyName, string charToConcatColorsList)
    {
        string colorsFormatted = "";
        if (root.TryGetProperty(propertyName, out var colorsProp) && colorsProp.ValueKind == JsonValueKind.Array)
        {
            var colorsList = new List<string>();
            foreach (var colorElement in colorsProp.EnumerateArray())
            {
                colorsList.Add(colorElement.GetString() ?? "");
            }
            colorsFormatted = string.Join(charToConcatColorsList, colorsList);
        }
        return colorsFormatted;
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

    public static string GetDateTimePropertyOrEmpty(this JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
        {
            string dateValue = prop.GetString() ?? "";
            if (DateTime.TryParseExact(dateValue, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return dateValue;
            }

            //O enunciado fala que o formtato esperado é yyyy-MM-dd, mas não fica claro se é apenas para a saída do CSV ou se o JSON de entrada também tem que estar nesse formato.
            if (DateTime.TryParseExact(dateValue, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime resultBrazilFormat))
            {
                return resultBrazilFormat.ToString("yyyy-MM-dd");
            }
        }
        return "";
    }
}
