using AIChatService.Models;
using System.Text.RegularExpressions;

namespace AIChatService.Validators;

public class PhoneValidator : InputValidator
{
    // Aceita formatos: (11) 99999-9999, 11-99999-9999, 11999999999, +5511999999999
    private static readonly Regex PhoneRegex = new Regex(
        @"^(\+55\s?)?(\(?\d{2}\)?[\s-]?)?9?\d{4}[\s-]?\d{4}$",
        RegexOptions.Compiled
    );

    public override ValidationResult Validate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return ValidationResult.Failure("❌ O número de telefone não pode estar vazio.");
        }

        // Remove espaços extras
        input = input.Trim();

        // Valida o formato
        if (!PhoneRegex.IsMatch(input))
        {
            return ValidationResult.Failure(
                "❌ Formato de telefone inválido. Por favor, use um formato válido como:\n" +
                "• (11) 99999-9999\n" +
                "• 11-99999-9999\n" +
                "• 11999999999\n" +
                "• +5511999999999"
            );
        }

        // Normaliza o número (remove tudo exceto dígitos e +)
        string normalized = Regex.Replace(input, @"[^\d+]", "");

        // Valida comprimento mínimo (10 ou 11 dígitos sem código do país)
        string digitsOnly = normalized.Replace("+", "");
        if (digitsOnly.Length < 10 || digitsOnly.Length > 13)
        {
            return ValidationResult.Failure(
                "❌ O número de telefone deve conter entre 10 e 11 dígitos (com DDD)."
            );
        }

        return ValidationResult.Success(normalized);
    }
}
