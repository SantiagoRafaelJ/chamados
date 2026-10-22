# Chamados

SDK exigida: .NET SDK 10.0.401

## Como executar

dotnet build

dotnet run --project src/Chamados.Web

## Como testar

dotnet test

## Decisões

- Alvo: .NET 10 LTS, com suporte até 14 de novembro de 2028.
- Formato de solução: .slnx (XML, mais simples de ler e de versionar que o .sln).
- Abstração do Repositório (`IChamadoRepositorio`): o serviço de domínio depende apenas da interface para desacoplar a regra de negócio do armazenamento em memória, facilitando a futura substituição pelo Entity Framework Core no Módulo 7[cite: 9, 20].
- Injeção de `TimeProvider`: o relógio do sistema é injetado como dependência para permitir o controlo e avanço previsível do tempo em testes unitários através do `RelogioFixo`[cite: 3, 17, 20].
- Casos de uso no `Chamados.Domain`: os casos de uso foram mantidos diretamente no domínio, evitando uma camada de aplicação extra (*Chamados.Application*) desnecessária nesta fase[cite: 11, 20].

## Roteiro

- Módulo 2: expande o Chamados.Domain[cite: 33].
- Módulo 3: transforma o Chamados.Web em aplicação ASP.NET Core completa[cite: 20, 33].
