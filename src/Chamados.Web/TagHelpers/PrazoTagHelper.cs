using Chamados.Domain;
using Chamados.Web.Apresentacao;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Chamados.Web.TagHelpers;

[HtmlTargetElement("prazo", Attributes = "criado-em,prioridade,status", TagStructure = TagStructure.WithoutEndTag)]
public sealed class PrazoTagHelper(TimeProvider relogio) : TagHelper
{
    public DateTimeOffset CriadoEm { get; set; }

    public Prioridade Prioridade { get; set; }

    public StatusChamado Status { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Status is StatusChamado.Resolvido or StatusChamado.Cancelado)
        {
            output.SuppressOutput();
            return;
        }

        var agora = relogio.GetUtcNow();
        var vencimento = PrazoAtendimento.VenceEm(CriadoEm, Prioridade);
        var atrasado = agora > vencimento;
        var texto = atrasado
            ? $"Atrasado há {(agora - vencimento).Duracao()}"
            : $"Vence em {(vencimento - agora).Duracao()}";

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", atrasado ? "prazo prazo-atrasado" : "prazo prazo-no-prazo");
        output.Content.SetContent(texto);
    }
}
