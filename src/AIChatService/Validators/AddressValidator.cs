using AIChatService.Models;
using System.Text.RegularExpressions;

namespace AIChatService.Validators;

public class AddressValidator : InputValidator
{
    private const int MinimumLength = 10;
    private const int MaximumLength = 500;

    public override ValidationResult Validate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return ValidationResult.Failure("❌ O endereço não pode estar vazio.");
        }

        // Remove espaços extras no início e fim
        input = input.Trim();

        // Valida comprimento mínimo
        if (input.Length < MinimumLength)
        {
            return ValidationResult.Failure(
                $"❌ O endereço é muito curto. Por favor, forneça um endereço completo com pelo menos {MinimumLength} caracteres.\n" +
                "Exemplo: Rua das Flores, 123, Centro, São Paulo - SP"
            );
        }

        // Valida comprimento máximo
        if (input.Length > MaximumLength)
        {
            return ValidationResult.Failure(
                $"❌ O endereço é muito longo. Use no máximo {MaximumLength} caracteres."
            );
        }

        // Verifica se não é apenas caracteres especiais ou números
        if (Regex.IsMatch(input, @"^[\s\d\-,\.]+$"))
        {
            return ValidationResult.Failure(
                "❌ O endereço deve conter texto descritivo (nome da rua, bairro, etc).\n" +
                "Exemplo: Rua das Flores, 123, Centro, São Paulo - SP"
            );
        }

        // Normaliza espaços múltiplos
        string normalized = Regex.Replace(input, @"\s+", " ");

        return ValidationResult.Success(normalized);
    }
}
