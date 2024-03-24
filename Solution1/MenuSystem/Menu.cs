namespace MenuSystem;

public class Menu
{
    
    public string? Title { get; set; }
    
    public Dictionary<string, MenuItem> MenuItems { get; set; } = new();

    private const string MenuSeparator = "--------------------------";
    
    private static readonly HashSet<string> ReservedShortcuts = new() {"X", "B", "R"};

    public Menu(string? title, List<MenuItem> menuItems)
    {
        Title = title;
        foreach (var menuItem in menuItems)
        {
            // Validating list of menuItems, composing menuItems dictionary
            if (ReservedShortcuts.Contains(menuItem.Shortcut.ToUpper())) 
                throw new ApplicationException(
                    $"Menu shortcut '{menuItem.Shortcut.ToUpper()}' in not allowed list!");
            

            if (MenuItems.ContainsKey(menuItem.Shortcut.ToUpper()))
                throw new ApplicationException(
                    $"Menu shortcut '{menuItem.Shortcut.ToUpper()}' is already registered!");

            MenuItems[menuItem.Shortcut.ToUpper()] = menuItem;
        }
    }

    private void Draw(EMenuLevel menuLevel)
    {
        if (!string.IsNullOrWhiteSpace(Title))
        {
            // If title is empty then -> no need to write out the title or the separator.
            Console.WriteLine(Title);
            Console.WriteLine(MenuSeparator);
        }
        
        foreach (var menuItem in MenuItems)
        {
            // Write out every menuitem (shortcut) label/function label
            Console.Write("(");
            Console.Write(menuItem.Key);
            Console.Write(") ");
            Console.WriteLine(menuItem.Value.MenuLabelFunction != null
                ? menuItem.Value.MenuLabelFunction()
                : menuItem.Value.MenuLabel);
        }

        // Depending on which menu level is being run, write either (B), (R), (X) or all
        if (menuLevel != EMenuLevel.First) Console.WriteLine("(B) back");
        Console.WriteLine("(X) exit");

        Console.WriteLine(MenuSeparator);
        Console.Write("Your choice: ");
    }

    public string? Run(EMenuLevel menuLevel = EMenuLevel.First)
    {
        Console.Clear();
        string? userChoice;
        
        do
        {
            Draw(menuLevel);
            userChoice = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(userChoice) && MenuItems.ContainsKey(userChoice.ToUpper()))
            {
                string? result;
                if (MenuItems[userChoice.ToUpper()].SubMenuToRun != null)
                {
                    if (menuLevel == EMenuLevel.First)
                    {
                         result = MenuItems[userChoice.ToUpper()].SubMenuToRun!(EMenuLevel.Second);
                    }
                    else
                    {
                        result = MenuItems[userChoice.ToUpper()].SubMenuToRun!(EMenuLevel.Other);
                    }
                    
                }
                else if (MenuItems[userChoice.ToUpper()].MethodToRun != null)
                {
                    result = MenuItems[userChoice.ToUpper()].MethodToRun!();
                    if (result?.ToUpper() == "X") userChoice = "X";
                    if (userChoice.ToUpper() == "B") continue; // method is executed and menu is draw out again.
                }
            }
            else if (!ReservedShortcuts.Contains(userChoice?.ToUpper()!))
                Console.WriteLine("Undefined shortcut....");

            Console.WriteLine();
            
        } while (!ReservedShortcuts.Contains(userChoice?.ToUpper()!));
        
        return userChoice;
    }
}