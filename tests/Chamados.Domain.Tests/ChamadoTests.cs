namespace Chamados.Domain.Tests;

public class ChamadoTests
{
    [Fact]
    public void ChamadoNovo_NasceComStatusAberto()
    {
        var chamado = new Chamado("Impressora parada");

        Assert.Equal(StatusChamado.Aberto, chamado.Status);
    }

    [Fact]
    public void TituloEmBranco_LancaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Chamado("   "));
    }
}