# LABDIO004# Gerador de Boletos - Azure Functions

Este projeto implementa uma Azure Function para geração de boletos bancários, utilizando .NET 8 e arquitetura serverless. O objetivo é fornecer uma API escalável para geração e validação de boletos, podendo ser integrada a sistemas financeiros ou e-commerces.

## Estrutura

- **fnGeradorBoletos/**: Código-fonte da Azure Function responsável por gerar boletos.
- **fnValidaBoleto/**: (Opcional) Função para validação de boletos.
- **front/**: (Opcional) Interface web para interação com as funções.

## Tecnologias Utilizadas

- .NET 8
- Azure Functions (Isolated Worker)
- [Newtonsoft.Json](https://www.newtonsoft.com/json)
- [SkiaSharp](https://github.com/mono/SkiaSharp) (para geração de imagens)
- Outros pacotes listados em `project.assets.json`

## Como Executar Localmente

1. Instale o [.NET 8 SDK](https://dotnet.microsoft.com/download).
2. Instale as [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local).
3. Navegue até a pasta do projeto:

   ```sh
   cd LAB004/LABServeless/fnGeradorBoletos/fnGeradorBoletos
   ```

4. Restaure os pacotes:

   ```sh
   dotnet restore
   ```

5. Execute a função localmente:

   ```sh
   func start
   ```

## Endpoints

- **POST /api/gerar-boleto**  
  Gera um novo boleto a partir dos dados enviados no corpo da requisição.

## Configuração

- Edite o arquivo `local.settings.json` para configurar strings de conexão e variáveis de ambiente necessárias.

## Deploy

O deploy pode ser feito diretamente pelo Visual Studio, VS Code ou via CLI com:

```sh
func azure functionapp publish <NOME_DA_SUA_FUNCTION_APP>
```

## Licença

Este projeto está licenciado sob os termos do arquivo [LICENSE](LICENSE).

---

> Projeto desenvolvido para fins educacionais e demonstração de arquitetura serverless com Azure Functions.