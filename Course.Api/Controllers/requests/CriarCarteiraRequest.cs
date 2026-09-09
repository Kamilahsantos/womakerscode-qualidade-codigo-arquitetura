using System.ComponentModel.DataAnnotations;
using Course.Api.Exceptions;

namespace Course.Api.Controllers.Requests;

public class CriarCarteiraRequest
{
    [Required(ErrorMessage = MensagensErro.NomeObrigatorio)]
    [StringLength(150, MinimumLength = 3, ErrorMessage = MensagensErro.NomeTamanho)]
    public required string Nome { get; set; }

    [Required(ErrorMessage = MensagensErro.CpfObrigatorio)]
    [RegularExpression(@"^(\d{3}\.\d{3}\.\d{3}-\d{2}|\d{11})$", ErrorMessage = MensagensErro.CpfFormatoInvalido)]
    public required string Cpf { get; set; }

    [Required(ErrorMessage = MensagensErro.SenhaObrigatoria)]
    [MinLength(6, ErrorMessage = MensagensErro.SenhaTamanhoMinimo)]
    public required string Senha { get; set; }

    [Required(ErrorMessage = MensagensErro.EmailObrigatorio)]
    [EmailAddress(ErrorMessage = MensagensErro.EmailFormatoInvalido)]
    public required string Email { get; set; }
}
