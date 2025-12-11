namespace AIChatService.Models;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string NormalizedValue { get; set; } = string.Empty;

    public static ValidationResult Success(string normalizedValue)
    {
        return new ValidationResult
        {
            IsValid = true,
            NormalizedValue = normalizedValue
        };
    }

    public static ValidationResult Failure(string errorMessage)
    {
        return new ValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}
