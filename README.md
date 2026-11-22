# Sistema de Gerenciamento de Oficina Mecânica — Arquitetura em Microsserviços

Projeto final da disciplina de **Arquitetura de Software** (Engenharia de Software, Centro Universitário SATC), sob orientação do professor Eduardo Cizeski Meneghel.

**Equipe:** Gustavo D'Agostin Zanelato, Mirela Tiscoski Pasini e Gabriel Rodolfo Rabello.

---

# Documento de Requisitos

## 1. Propósito do sistema

Automatizar e otimizar os fluxos operacionais de uma oficina mecânica, integrando de forma desacoplada a gestão de **clientes**, **veículos**, **estoque de peças** e **ordens de serviço**. O sistema adota arquitetura de microsserviços com .NET, SQLite e APIs REST, buscando modularidade, escalabilidade e manutenibilidade.

## 2. Usuários

| Perfil | O que faz no sistema |
| --- | --- |
| **Atendente / Recepcionista** | Cadastra clientes e veículos e abre ordens de serviço. |
| **Mecânico / Técnico** | Consulta as ordens de serviço, associa as peças utilizadas e atualiza o status das manutenções. |
| **Gerente / Administrador** | Supervisiona o estoque, acompanha as ordens de serviço e gerencia os dados gerais do negócio. |

## 3. Requisitos funcionais

| ID | Microsserviço | Descrição |
| --- | --- | --- |
| RF01 | Clientes | Cadastrar clientes (nome, CPF/CNPJ, telefone, e-mail e endereço). |
| RF02 | Clientes | Consultar, alterar e remover clientes cadastrados. |
| RF03 | Veículos | Cadastrar veículos vinculados a um cliente (placa, modelo, ano e quilometragem). |
| RF04 | Veículos | Listar e atualizar os veículos cadastrados. |
| RF05 | Peças | Gerenciar o estoque de peças (código, nome, descrição, preço unitário e quantidade). |
| RF06 | Peças | Atualizar automaticamente a quantidade em estoque quando houver baixa por utilização. |
| RF07 | Ordens de Serviço | Abrir uma Ordem de Serviço associada a um cliente e a um veículo. |
| RF08 | Ordens de Serviço | Adicionar peças utilizadas no conserto dentro de uma Ordem de Serviço. |
| RF09 | Ordens de Serviço | Registrar o histórico de manutenções, reparos e status de cada Ordem de Serviço. |

---

# Descritivo Técnico

## 4. Microsserviços

A aplicação é composta por **quatro microsserviços independentes**, em .NET 10, cada um com seu próprio banco SQLite e comunicação via REST. Os relacionamentos entre serviços usam **chaves lógicas**: cada serviço guarda apenas o identificador do registro de outro serviço e o consulta por requisição HTTP.

| Microsserviço | Pasta | Porta local | Função |
| --- | --- | --- | --- |
| **Clientes** | [`Clientes/`](Clientes) | 5298 | Cadastro dos proprietários dos veículos (nome, CPF/CNPJ único, telefone, e-mail e endereço). Oferece cadastro, consulta, alteração e remoção. É consultado pelo serviço de Ordens de Serviço. |
| **Veículos** | [`Veiculos/`](Veiculos) | 5296 | Cadastro dos veículos atendidos (placa única, modelo, ano, quilometragem e cliente proprietário). Oferece cadastro, listagem e atualização. É consultado pelo serviço de Ordens de Serviço. |
| **Peças** | [`Pecas/`](Pecas) | 5297 | Estoque de peças e componentes (código único, nome, descrição, preço unitário e quantidade). Recebe do serviço de Ordens de Serviço a requisição que dá baixa automática no estoque. |
| **Ordens de Serviço** | [`OrdemServico/`](OrdemServico) | 5299 | Registro das manutenções e reparos: ordem (cliente, veículo, datas, status, observações, valores e taxa de serviço) e itens de peças utilizadas, com o preço unitário no momento da baixa. **Orquestra as integrações** abaixo. |

```
                      ┌──────────────┐
              ┌──────▶│   Clientes   │  (busca 1: valida cliente)
              │       └──────────────┘
┌─────────────┴─┐     ┌──────────────┐
│ Ordens de     │────▶│   Veículos   │  (busca 2: valida veículo)
│ Serviço       │     └──────────────┘
└─────────────┬─┘     ┌──────────────┐
              └──────▶│    Peças     │  (alteração: baixa de estoque)
                      └──────────────┘
```

## 5. Integrações entre os microsserviços

| Integração | Origem → Destino | Funcionamento |
| --- | --- | --- |
| **Busca simples 1** | Ordens de Serviço → Clientes | Ao abrir ou atualizar uma OS, consulta `GET /api/clientes/{id}` para confirmar a existência do proprietário. |
| **Busca simples 2** | Ordens de Serviço → Veículos | Ao abrir ou atualizar uma OS, consulta `GET /api/veiculos/{id}` para confirmar a existência do veículo. |
| **Alteração de dados** | Ordens de Serviço → Peças | Ao adicionar uma peça à OS, consulta o preço (`GET /api/pecas/{id}`) e dispara `POST /api/pecas/{id}/baixas`, que dá baixa automática no estoque. |

Tratamento de erros nas integrações: referência inexistente → `400`; peça sem estoque suficiente → `409`; microsserviço indisponível ou lento (timeout de 30 s) → `502`.

## 6. Modelo de dados

Cada microsserviço possui seu próprio banco SQLite. *FK lógica* indica referência a um registro de outro microsserviço, obtido por integração REST.

**Clientes — `Clientes`**

| Coluna | Tipo | Nulo? | Restrição |
| --- | --- | --- | --- |
| Id | INTEGER | Não | PK |
| Nome | TEXT | Não | — |
| CpfCnpj | TEXT | Não | UNIQUE |
| Telefone | TEXT | Sim | — |
| Email | TEXT | Sim | — |
| Endereco | TEXT | Sim | — |

**Veículos — `Veiculos`**

| Coluna | Tipo | Nulo? | Restrição |
| --- | --- | --- | --- |
| Id | INTEGER | Não | PK |
| ClienteId | INTEGER | Não | FK lógica (Clientes) |
| Placa | TEXT | Não | UNIQUE |
| Modelo | TEXT | Não | — |
| Ano | INTEGER | Não | — |
| Quilometragem | INTEGER | Sim | — |

**Peças — `Pecas`**

| Coluna | Tipo | Nulo? | Restrição |
| --- | --- | --- | --- |
| Id | INTEGER | Não | PK |
| Codigo | TEXT | Não | UNIQUE |
| Nome | TEXT | Não | — |
| Descricao | TEXT | Sim | — |
| PrecoUnitario | REAL | Não | — |
| QuantidadeEstoque | INTEGER | Não | — |

**Ordens de Serviço — `OrdensServico`**

| Coluna | Tipo | Nulo? | Restrição |
| --- | --- | --- | --- |
| Id | INTEGER | Não | PK |
| ClienteId | INTEGER | Não | FK lógica (Clientes) |
| VeiculoId | INTEGER | Não | FK lógica (Veículos) |
| DataCriacaoServico | DATETIME | Não | — |
| DataInicioServico | DATETIME | Não | — |
| DataConclusaoServico | DATETIME | Sim | — |
| Status | TEXT | Não | Aberta, Em Andamento, Concluída ou Cancelada |
| ValorTotal | DECIMAL | Sim | Valor base + taxa de serviço |
| Observacoes | TEXT | Sim | — |
| TaxaServico | DECIMAL | Sim | — |
| ValorBaseServico | DECIMAL | Sim | Soma de (quantidade × preço) dos itens |

**Ordens de Serviço — `ItensOrdemServico`**

| Coluna | Tipo | Nulo? | Restrição |
| --- | --- | --- | --- |
| Id | INTEGER | Não | PK (autoincremento) |
| OrdemServicoId | INTEGER | Não | FK (OrdensServico) |
| PecaId | INTEGER | Não | FK lógica (Peças) |
| Quantidade | INTEGER | Não | — |
| PrecoUnitarioMomento | DECIMAL | Não | Preço da peça no momento da baixa |

## 7. Principais endpoints

Todos os serviços expõem `GET /health` e a documentação interativa (Scalar) em `/scalar/v1`, com o OpenAPI em `/openapi/v1.json`.

| Serviço | Endpoints |
| --- | --- |
| Clientes | `GET/POST /api/clientes`, `GET/PUT/DELETE /api/clientes/{id}` |
| Veículos | `GET/POST /api/veiculos`, `GET/PUT /api/veiculos/{id}` |
| Peças | `GET/POST /api/pecas`, `GET/PUT/DELETE /api/pecas/{id}`, `POST /api/pecas/{id}/baixas` |
| Ordens de Serviço | `GET/POST /api/ordens-servicos`, `GET/PUT/DELETE /api/ordens-servicos/{id}`, `POST /api/ordens-servicos/{id}/pecas` |

Cada pasta possui também um arquivo `.http` com exemplos de chamadas.

---

# Como executar

## Localmente

Pré-requisito: [.NET SDK 10](https://dotnet.microsoft.com/download). Em quatro terminais (ou pela solução `Projeto_Oficina_Mecanica.slnx`, com múltiplos projetos de inicialização):

```powershell
dotnet run --project Clientes/Clientes.csproj
dotnet run --project Veiculos/Veiculos.csproj
dotnet run --project Pecas/Pecas.csproj
dotnet run --project OrdemServico/OrdemServico.csproj
```

As URLs usadas pelo serviço de Ordens de Serviço para chamar os demais ficam em `Microservices` no `OrdemServico/appsettings.json` (ou nas variáveis de ambiente `Microservices__ClientesUrl`, `Microservices__VeiculosUrl` e `Microservices__PecasUrl`).

Fluxo de teste sugerido: cadastrar um cliente → cadastrar um veículo para ele → cadastrar uma peça → abrir uma OS → adicionar a peça à OS e conferir a baixa no estoque.

## Docker

Cada microsserviço possui seu `Dockerfile` (porta `8080`, banco SQLite em `/data`):

```bash
docker build -t oficina-clientes ./Clientes
docker run -p 5298:8080 oficina-clientes
```

## Render

Aplicação publicada no [Render](https://render.com/) (plano gratuito). Cada microsserviço é um Web Service Docker criado a partir deste repositório (`Dockerfile` de cada pasta como caminho do Dockerfile e a própria pasta como contexto de build). O [`render.yaml`](render.yaml) documenta essa configuração.

| Microsserviço | URL | Documentação (Scalar) |
| --- | --- | --- |
| Clientes | https://clientes-aj8n.onrender.com | [/scalar/v1](https://clientes-aj8n.onrender.com/scalar/v1) |
| Veículos | https://veiculos-qebj.onrender.com | [/scalar/v1](https://veiculos-qebj.onrender.com/scalar/v1) |
| Peças | https://pecas.onrender.com | [/scalar/v1](https://pecas.onrender.com/scalar/v1) |
| Ordens de Serviço | https://ordemservico-mdbx.onrender.com | [/scalar/v1](https://ordemservico-mdbx.onrender.com/scalar/v1) |

A raiz (`/`) de cada serviço redireciona para o Scalar, onde é possível testar os endpoints. Variáveis de ambiente usadas: `PORT=8080` em todos e, no serviço de Ordens de Serviço, `Microservices__ClientesUrl`, `Microservices__VeiculosUrl` e `Microservices__PecasUrl` com as URLs da tabela acima.

> No plano gratuito os serviços "dormem" após ~15 min sem uso (a primeira chamada pode levar cerca de 50 s) e o SQLite é recriado a cada deploy/restart. Para testar o fluxo completo, acorde os 4 serviços acessando o `/health` de cada um e cadastre cliente, veículo e peça antes de abrir a OS.

--- | --- |
| Clientes | https://oficina-mecanica-clientes.onrender.com |
| Veículos | https://oficina-mecanica-veiculos.onrender.com |
| Peças | https://oficina-mecanica-pecas.onrender.com |
| Ordens de Serviço | https://oficina-mecanica-ordens-servico.onrender.com |

--- | --- |
| Clientes | _a definir_ |
| Veículos | _a definir_ |
| Peças | _a definir_ |
| Ordens de Serviço | _a definir_ |

---

# Tecnologias

.NET 10 · ASP.NET Core (controllers) · Entity Framework Core · SQLite · REST/JSON · OpenAPI + Scalar · Docker
