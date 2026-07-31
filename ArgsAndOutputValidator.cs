using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerTeamsJsonToCsv;

internal class ArgsAndOutputValidator
{
    internal static void ValidateAppPath(string? appPath)
    {
        if (string.IsNullOrEmpty(appPath))
            throw new ArgumentException("Não foi possível determinar o caminho da aplicação.");
    }

    internal static void ValidateOutputDirectoryAndCreateIfNotExists(string outputCsvDir)
    {
        if (string.IsNullOrEmpty(outputCsvDir))
        {
            throw new ArgumentException("Caminho do diretório de saída não fornecido ou inválido.");
        }

        if (!Directory.Exists(outputCsvDir))
        {
            try
            {
                Directory.CreateDirectory(outputCsvDir);
            }
            catch (Exception ex)
            {
                throw new IOException($"Falha ao criar o diretório de saída: {outputCsvDir}. Erro: {ex.Message}");
            }
        }
    }

    internal static void ValidateJsonlInputPathArgument(string jsonlFilePath)
    {
        if (string.IsNullOrEmpty(jsonlFilePath))
        {
            throw new ArgumentException("Caminho do arquivo JSONL não fornecido ou inválido.");
        }
        if (jsonlFilePath.Split('.').Last() != "jsonl")
        {
            throw new ArgumentException("O arquivo fornecido não é um arquivo JSONL válido.");
        }
        if (!File.Exists(jsonlFilePath))
        {
            throw new FileNotFoundException($"O arquivo JSONL fornecido não existe: {jsonlFilePath}");
        }
    }
}
