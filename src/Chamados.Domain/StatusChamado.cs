namespace Chamados.Domain;

/// <summary>
/// Estados possíveis de um chamado ao longo do atendimento.
/// </summary>
public enum StatusChamado
{
    Aberto,
    EmAtendimento,
    Resolvido,
    Cancelado
}
