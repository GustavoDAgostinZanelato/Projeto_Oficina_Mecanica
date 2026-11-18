# Microsserviço de Clientes

API REST de cadastro dos proprietários dos veículos da oficina, implementada em ASP.NET Core, EF Core e SQLite, seguindo a estrutura do template do projeto (mesma do microsserviço de Peças).

## Executar

```powershell
dotnet run --project Clientes.csproj
```

Porta local: `http://localhost:5298`. A documentação interativa do Scalar fica em `/scalar/v1` e o OpenAPI em `/openapi/v1.json`. O banco `clientes.db` e a tabela `Clientes` são criados na inicialização.

## Endpoints

| Método | Rota | Descrição |
| --- | --- | --- |
| GET | `/api/clientes` | Lista os clientes |
| GET | `/api/clientes/{id}` | Consulta um cliente |
| POST | `/api/clientes` | Cadastra um cliente |
| PUT | `/api/clientes/{id}` | Atualiza os dados do cliente |
| DELETE | `/api/clientes/{id}` | Remove um cliente |

O CPF/CNPJ é único e armazenado somente com dígitos (a pontuação enviada é removida). CPF/CNPJ duplicado retorna `409`; cliente inexistente retorna `404`.

## Integração

O microsserviço de Ordens de Serviço consulta `GET /api/clientes/{id}` para exibir os dados do proprietário do veículo (busca simples 1).
