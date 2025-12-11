using AIChatService.Models;

namespace AIChatService.Validators;

public abstract class InputValidator
{
    public abstract ValidationResult Validate(string input);
}
