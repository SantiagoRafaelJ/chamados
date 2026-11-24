# Chamados

Sistema de chamados (ordens de serviço) — projeto integrador da formação
*Desenvolvimento Web e Multiplataforma com .NET*. Estado: Módulo 5.

## Requisitos

- .NET SDK 10.0.1xx ou superior na linha 10.0 (fixado em `global.json`)
- Git 2.28+
- Visual Studio 2026 (Community serve) com a carga de trabalho **ASP.NET e desenvolvimento Web**

## Executar

No Visual Studio: abra `Chamados.slnx`, escolha o perfil **https** na barra de ferramentas e pressione `F5`.
O navegador abre no painel (`https://localhost:7080/`). Na primeira execução, aceite a confiança no certificado de desenvolvimento.

Pela CLI:

```bash
dotnet build
dotnet run --project src/Chamados.Web --launch-profile https
```

Portas fixas: http 5080, https 7080. Em Development a base em memória recebe 12 chamados de exemplo (três páginas de 5 na lista).

### Páginas (Razor Pages)

| Rota | Página | O que faz |
| --- | --- | --- |
| `/` | `Pages/Index` | Painel com totais por status (`ObterEstatisticasAsync`) |
| `/Chamados?status=&prioridadeMinima=&texto=&ordem=&pagina=` | `Pages/Chamados/Index` | Lista com filtros, ordenação e paginação (`ListarPaginaAsync`) |
| `/Chamados/Novo` | `Pages/Chamados/Novo` | Formulário de abertura (`AbrirAsync`), validação e Post/Redirect/Get |
| `/Chamados/Detalhes/{id}` | `Pages/Chamados/Detalhes` | Detalhe, histórico e handlers Iniciar, Resolver, Cancelar |
| `/Chamados/Editar/{id}` | `Pages/Chamados/Editar` | Edição com concorrência otimista (`EditarAsync`) |
| `/Erro`, `/Erro/{código}` | `Pages/Erro` | Erros sem detalhes internos; 404 e 400 amigáveis fora de `/api` |

### Páginas Blazor (componentes Razor, Módulo 5)

| Rota | Componente | O que faz |
| --- | --- | --- |
| `/atendimento` | `Components/Pages/Atendimento.razor` | Fila interativa (InteractiveServer): busca, iniciar, resolver e cancelar sem recarregar |
| `/modos` | `Components/Pages/Modos.razor` | O mesmo `Contador` renderizado de forma estática e interativa, lado a lado |

Razor Pages e Blazor convivem na mesma aplicação: `MapRazorPages()` e `MapRazorComponents<App>()` no `Program.cs`, o mesmo `site.css` e o mesmo `ChamadoServico`.

### API JSON (movida para `/api` no Módulo 3)

| Rota | Função |
| --- | --- |
| `GET /api/chamados?status=&prioridadeMinima=&texto=&ordem=` | Lista com filtros e ordenação |
| `GET /api/chamados/estatisticas` | Totais e tempo médio de resolução |
| `GET /api/chamados/{id}` | Um chamado |
| `POST /api/chamados` | Abre chamado (corpo: `{"titulo": "...", "prioridade": "Alta"}`) |
| `POST /api/chamados/{id}/atendimento` | Inicia atendimento (corpo: `{"responsavel": "..."}`) |
| `PUT /api/chamados/{id}` | Edita (corpo com `versaoEsperada`; versão desatualizada → 409) |
| `GET /api/status`, `GET /api/ambiente` | Herdadas do Módulo 1 (`/api/ambiente` só em Development) |

Erros de domínio na API voltam como Problem Details (RFC 9457): 400, 404 ou 409.

### Configuração

`Simulacao:LatenciaMs` (0 a 10000) atrasa a listagem para observar cancelamento.
Valor fora da faixa impede a aplicação de iniciar (`ValidateOnStart`). Para testar sem editar arquivo:

```powershell
$env:Simulacao__LatenciaMs = "20000"; dotnet run --project src/Chamados.Web
```

### Bibliotecas do lado do cliente

`wwwroot/lib` traz jQuery 3.7.1, jQuery Validation 1.21.0 e jQuery Validation Unobtrusive 4.0.0 (licenças MIT e Apache 2.0, arquivos de licença incluídos), usados só pela validação no cliente. O `libman.json` registra a origem; no Visual Studio, botão direito em `libman.json` › **Restaurar Bibliotecas do Lado do Cliente** baixa de novo.

## Testar

No Visual Studio: **Teste > Gerenciador de Testes > Executar Todos** (`Ctrl+R, A`). Pela CLI: `dotnet test`.

## Estrutura

| Projeto | Papel | Depende de |
| --- | --- | --- |
| `src/Chamados.Domain` | Entidades, regras e casos de uso | — |
| `src/Chamados.Web` | Páginas Razor, API JSON, composição e pipeline | Domain |
| `tests/Chamados.Domain.Tests` | Testes xUnit do domínio | Domain |

## Decisões

1. **.NET 10 LTS** como alvo: suporte até 14/11/2028; .NET 8 e 9 encerram em 10/11/2026.
2. **Formato `.slnx`** para a solução, padrão do SDK 10.
3. **Chamados.Web como projeto de inicialização**, declarado no `.slnx` com `DefaultStartup="true"`.
4. **Domain não referencia nenhum outro projeto**: as camadas externas dependem dele, nunca o contrário.
5. **Repositório como interface** (`IChamadoRepositorio`): a implementação em memória será trocada pelo EF Core no Módulo 7.
6. **`TimeProvider` injetado** em vez de `DateTimeOffset.UtcNow` direto: permite testar datas com um relógio controlado.
7. **Sem camada Application separada** por enquanto: os casos de uso (`ChamadoServico`) ficam no Domain.
8. **Razor Pages para os formulários** (Módulo 3): uma página por tela, com o `PageModel` chamando só o `ChamadoServico`, nunca o repositório.
9. **Validação em duas camadas**: Data Annotations no modelo do formulário (mensagem amigável, no servidor) e invariantes no domínio (palavra final). Os limites de tamanho são constantes do `Chamado`, reusadas nos atributos.
10. **Sem validação no cliente por enquanto**: nenhuma biblioteca JavaScript; o servidor valida sempre. Validação no cliente é conforto, não segurança (Módulo 4).
11. **API sob `/api`** com `MapGroup` e um filtro único de exceções de domínio, para não disputar a rota `/Chamados` com as páginas.
12. **Concorrência otimista com número de versão** (Módulo 4): `Chamado.Versao` aumenta a cada alteração; quem edita informa a versão lida. No Módulo 7 o EF Core assume a conferência com um token de concorrência.
13. **Paginação no serviço, não na página**: `ListarPaginaAsync` devolve `Pagina<T>`. Quando o EF Core chegar, `Skip`/`Take` viram SQL sem mudar as páginas.
14. **Partial para o que só exibe, View Component para o que busca dados, Tag Helper para marcação repetida**: `_CamposChamado`, `ResumoStatus` e `<selo>`.
15. **Páginas de status só fora de `/api`**: `UseWhen` cria um ramo do pipeline para que a API continue respondendo JSON.
16. **Blazor no mesmo projeto Web** (Módulo 5): Razor Pages ficam com os formulários de cadastro; a fila de atendimento, que muda o tempo todo, vira um componente interativo no servidor (`InteractiveServer`). Nenhum projeto novo.
17. **Interatividade por página, não global**: só `/atendimento` declara `@rendermode InteractiveServer`. O resto continua estático, sem conexão aberta com o servidor.
18. **Componente pede, página executa**: `CartaoChamado` só avisa a página por `EventCallback<PedidoTransicao>`; quem chama o `ChamadoServico` e trata as exceções é `Atendimento.razor`.
