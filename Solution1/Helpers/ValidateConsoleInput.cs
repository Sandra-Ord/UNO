namespace Helpers;

public static class ConsoleInput
{
    public static string Validate(List<string> validChoices, 
        string? prompt = null, 
        string errorText = "Please enter a valid option.", 
        string defaultOption = "", 
        bool optionToUpper = true)
    {
        string? userInput;
        do
        {
            Console.Write(prompt ?? "Your choice: ");
            userInput = optionToUpper ? Console.ReadLine()?.Trim().ToUpper() : Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(userInput)) userInput = defaultOption;
            if (validChoices.Count == 0 && !string.IsNullOrEmpty(userInput) && userInput.Length > 0) break;
            if (!validChoices.Contains(userInput)) Console.WriteLine(errorText);
        } while (!validChoices.Contains(userInput));
        return userInput;
    }
}