namespace Chamados.Domain;

public static class PrazoAtendimento
{
    public static TimeSpan Para(Prioridade prioridade) => prioridade switch
    {
        Prioridade.Critica => TimeSpan.FromHours(4),
        Prioridade.Alta => TimeSpan.FromHours(8),
        Prioridade.Media => TimeSpan.FromHours(24),
        Prioridade.Baixa => TimeSpan.FromHours(72),
        _ => throw new ArgumentOutOfRangeException(nameof(prioridade), prioridade, "Prioridade inexistente.")
    };

    public static DateTimeOffset VenceEm(DateTimeOffset criadoEm, Prioridade prioridade)
        => criadoEm.Add(Para(prioridade));
}
