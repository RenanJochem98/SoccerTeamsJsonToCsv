# SoccerTeamsJsonToCsv

Aplicação criada para converter arquivos JSON contendo informações de times de futebol em arquivos CSV.

Foi criada e testada utilizando **.NET 10.0** e **C# 14.0**.

## Passo a passo para executar a aplicação
1. Clone o repositório para sua máquina local: 
   ```
   git clone https://github.com/RenanJochem98/SoccerTeamsJsonToCsv.git
   ```
2. Entre no diretório:
   ```   
   cd SoccerTeamsJsonToCsv
   ```
3. Rode o comando:
   ```   
   dotnet run "path/to/inputJsonl.jsonl"
   ```

## Parametros
A aplicação aceita duas entradas, sendo 1 obrigatória.

- **Caminho do arquivo JSONL** *(obrigatório)*: É o local do arquivo JSONL que será usado como fonte de dados.
Caso não seja fornecido, a aplicação irá utilizar um arquivo padrão de testes que está no repositório.

- **Campeonatos para filtro** *(opcional)*: 
Este parâmetro pode ser usado para filtrar clubes de determinados campeonatos nas saídas CSV. 
O parâmetro espera uma string, com os campeonatos desejados separados por vírgula, no formato **"Campeonato 1, Campeonato 2"**. 
Por padrão, os filtros iniciais são: "SERIE A, SERIE B".

## Resultados
A aplicação gera dois arquivos `.csv`, um chamado *club.csv* e outro chamado *players.csv*.
Os dois arquivos ficam armazenado dentro da pasta **OutputCsv**, que está na raíz do projeto.