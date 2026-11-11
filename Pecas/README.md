# Microsserviço de Peças

API REST de gerenciamento do estoque da oficina, implementada em ASP.NET Core, EF Core e SQLite, seguindo a estrutura do template do projeto.

## Executar

```powershell
dotnet run --project Pecas.csproj
```

A documentação interativa do Scalar fica disponível em `/scalar/v1` e o OpenAPI em `/openapi/v1.json`. O banco `pecas.db` e a tabela `Pecas` são criados na inicialização.

## Endpoints

| Método | Rota | Descrição |
| --- | --- | --- |
| GET | `/api/pecas` | Lista as peças |
| GET | `/api/pecas/{id}` | Consulta uma peça |
| POST | `/api/pecas` | Cadastra uma peça |
| PUT | `/api/pecas/{id}` | Atualiza os dados da peça |
| DELETE | `/api/pecas/{id}` | Remove uma peça |
| POST | `/api/pecas/{id}/baixas` | Dá baixa na quantidade informada |

O código da peça é único. Preço e quantidade em estoque não podem ser negativos. A baixa usa uma atualização condicional no SQLite para impedir que o estoque fique negativo em requisições concorrentes; peça inexistente retorna `404`, código duplicado ou estoque insuficiente retorna `409`.

A baixa está disponível para integração REST pelo microsserviço de Ordens de Serviço. A quantidade consumida deve ser enviada no corpo JSON como `{ "quantidade": 1 }`.
