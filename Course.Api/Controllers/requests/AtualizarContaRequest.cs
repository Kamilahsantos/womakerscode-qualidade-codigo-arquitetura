using System.ComponentModel.DataAnnotations;
using Course.Api.Exceptions;

namespace Course.Api.Controllers.Requests;

public class AtualizarContaRequest
{
    [Required(ErrorMessage = MensagensErro.NomeObrigatorio)]
    [StringLength(150, MinimumLength = 3, ErrorMessage = MensagensErro.NomeTamanho)]
    public required string NovoNome { get; set; }

    [Required(ErrorMessage = MensagensErro.EmailObrigatorio)]
    [EmailAddress(ErrorMessage = MensagensErro.EmailFormatoInvalido)]
    public required string NovoEmail { get; set; }

    [Required(ErrorMessage = MensagensErro.SenhaObrigatoria)]
    [MinLength(6, ErrorMessage = MensagensErro.SenhaTamanhoMinimo)]
    public required string NovaSenha { get; set; }
}
