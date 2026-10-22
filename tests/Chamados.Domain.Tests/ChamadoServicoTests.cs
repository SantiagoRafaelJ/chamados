using Chamados.Domain;

namespace Chamados.Domain.Tests;

public class ChamadoServicoTests
{
    private static readonly DateTimeOffset Inicio = new(2026, 10, 5, 9, 0, 0, TimeSpan.Zero);

    private static (ChamadoServico Servico, RelogioFixo Relogio) CriarServico()
    {
        var relogio = new RelogioFixo(Inicio);
        var repositorio = new RepositorioChamadosEmMemoria(relogio);
        return (new ChamadoServico(repositorio, relogio), relogio);
    }

    [Fact]
    public async Task Listar_OrdenaPorPrioridadeEDepoisPorAbertura()
    {
        var (servico, relogio) = CriarServico();
        await servico.AbrirAsync(new NovoChamado("Baixa antiga", Prioridade.Baixa));
        relogio.Avancar(TimeSpan.FromMinutes(1));
        await servico.AbrirAsync(new NovoChamado("Critica", Prioridade.Critica));
        relogio.Avancar(TimeSpan.FromMinutes(1));
        await servico.AbrirAsync(new NovoChamado("Alta primeira", Prioridade.Alta));
        relogio.Avancar(TimeSpan.FromMinutes(1));
        await servico.AbrirAsync(new NovoChamado("Alta segunda", Prioridade.Alta));

        var lista = await servico.ListarAsync(FiltroChamados.Todos);

        Assert.Equal(
            ["Critica", "Alta primeira", "Alta segunda", "Baixa antiga"],
            lista.Select(r => r.Titulo).ToArray());
    }

    [Fact]
    public async Task Listar_FiltraPorTextoNaDescricaoSemDiferenciarMaiusculas()
    {
        var (servico, _) = CriarServico();
        await servico.AbrirAsync(new NovoChamado("Ar-condicionado", Prioridade.Alta, "Desliga na SALA 204"));
        await servico.AbrirAsync(new NovoChamado("Lâmpada", Prioridade.Baixa, "Corredor"));

        var lista = await servico.ListarAsync(new FiltroChamados(Texto: "sala"));

        var resumo = Assert.Single(lista);
        Assert.Equal("Ar-condicionado", resumo.Titulo);
    }

    [Fact]
    public async Task Listar_CombinaStatusEPrioridadeMinima()
    {
        var (servico, _) = CriarServico();
        var alta = await servico.AbrirAsync(new NovoChamado("Alta em atendimento", Prioridade.Alta));
        var baixa = await servico.AbrirAsync(new NovoChamado("Baixa em atendimento", Prioridade.Baixa));
        await servico.AbrirAsync(new NovoChamado("Alta aberta", Prioridade.Alta));
        await servico.IniciarAtendimentoAsync(alta.Id, "Equipe A");
        await servico.IniciarAtendimentoAsync(baixa.Id, "Equipe B");

        var lista = await servico.ListarAsync(
            new FiltroChamados(StatusChamado.EmAtendimento, Prioridade.Alta));

        var resumo = Assert.Single(lista);
        Assert.Equal("Alta em atendimento", resumo.Titulo);
    }

    [Fact]
    public async Task Estatisticas_CalculaTempoMedioDeResolucaoComRelogioAvancado()
    {
        var (servico, relogio) = CriarServico();
        var a = await servico.AbrirAsync(new NovoChamado("A"));
        var b = await servico.AbrirAsync(new NovoChamado("B"));
        await servico.IniciarAtendimentoAsync(a.Id, "Equipe");
        await servico.IniciarAtendimentoAsync(b.Id, "Equipe");

        relogio.Avancar(TimeSpan.FromHours(1));
        await servico.ResolverAsync(a.Id, "Feito");
        relogio.Avancar(TimeSpan.FromHours(2));
        await servico.ResolverAsync(b.Id, "Feito");

        var estatisticas = await servico.ObterEstatisticasAsync();

        Assert.Equal(TimeSpan.FromHours(2), estatisticas.TempoMedioResolucao);
    }

    [Fact]
    public async Task Resolver_IdInexistente_LancaChamadoNaoEncontrado()
    {
        var (servico, _) = CriarServico();

        await Assert.ThrowsAsync<ChamadoNaoEncontradoException>(
            () => servico.ResolverAsync(Guid.NewGuid(), "Feito"));
    }

    [Fact]
    public async Task Listar_ComTokenJaCancelado_LancaOperationCanceled()
    {
        var (servico, _) = CriarServico();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => servico.ListarAsync(FiltroChamados.Todos, cts.Token));
    }
}