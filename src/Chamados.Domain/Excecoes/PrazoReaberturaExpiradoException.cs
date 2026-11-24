namespace Chamados.Domain;

public sealed class PrazoReaberturaExpiradoException(Guid chamadoId, DateTimeOffset limite)
    : InvalidOperationException($"O prazo de reabertura do chamado {chamadoId} terminou em {limite:O}.")
{
    public Guid ChamadoId { get; } = chamadoId;

    public DateTimeOffset Limite { get; } = limite;
}
