using AIChatService.Models;
using System.Text.RegularExpressions;

namespace AIChatService.Validators;

public class EmailValidator : InputValidator
{
    // Validação simplificada de email (RFC 5322 básico)
    private static readonly Regex EmailRegex = new Regex(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public override ValidationResult Validate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return ValidationResult.Failure("❌ O e-mail não pode estar vazio.");
        }

        // Remove espaços extras
        input = input.Trim();

        // Valida o formato
        if (!EmailRegex.IsMatch(input))
        {
            return ValidationResult.Failure(
                "❌ Formato de e-mail inválido. Por favor, use um formato válido como:\n" +
                "• usuario@exemplo.com\n" +
                "• nome.sobrenome@empresa.com.br"
            );
        }

        // Valida comprimento máximo razoável
        if (input.Length > 254)
        {
            return ValidationResult.Failure("❌ O e-mail é muito longo. Use no máximo 254 caracteres.");
        }

        // Normaliza (converte para minúsculas)
        string normalized = input.ToLowerInvariant();

        return ValidationResult.Success(normalized);
    }
}
