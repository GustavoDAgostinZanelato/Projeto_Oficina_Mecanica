# Microsserviço de Ordens de Serviço

API REST responsável por registrar ordens de serviço e peças utilizadas, implementada em ASP.NET Core, EF Core e SQLite.

## Executar

```powershell
dotnet run --project PedidoServico.csproj
```

A documentação interativa do Scalar fica disponível em `/scalar/v1` e o OpenAPI em `/openapi/v1.json`. O banco `pedidoservico.db` e as tabelas `OrdensServico` e `ItensOrdemServico` são criados na inicialização.

## Integrações

As URLs dos microsserviços são configuradas em `Microservices` no `appsettings.json`:

- `ClientesUrl`: consulta a existência do cliente ao criar ou atualizar uma OS;
- `VeiculosUrl`: consulta a existência do veículo ao criar ou atualizar uma OS;
- `PecasUrl`: consulta o preço da peça e solicita a baixa automática no estoque ao adicionar uma peça.

## Endpoints

| Método | Rota | Descrição |
| --- | --- | --- |
| GET | `/api/ordens-servicos/health` | Verifica a disponibilidade do serviço |
| GET | `/api/ordens-servicos` | Lista as ordens de serviço |
| GET | `/api/ordens-servicos/{id}` | Consulta uma ordem de serviço |
| POST | `/api/ordens-servicos` | Abre uma ordem de serviço |
| PUT | `/api/ordens-servicos/{id}` | Atualiza dados e status da ordem |
| DELETE | `/api/ordens-servicos/{id}` | Remove uma ordem de serviço |
| POST | `/api/ordens-servicos/{id}/pecas` | Adiciona uma peça e dá baixa no estoque |

Status válidos: `Aberta`, `Em Andamento`, `Concluída` e `Cancelada`. Ao adicionar uma peça, o valor base é recalculado pelo preço no momento da baixa e o valor total recebe também a taxa de serviço.
