using MenuSystem;

namespace ConsoleApp1;

public class ProgramMenus
{
    public static Menu GetMainMenu(Func<string?> newGameMethod, Func<string?> loadGameMethod)
    {
        return new Menu("----  U  N  O  ----", new List<MenuItem>()
        {
            new MenuItem()
            {
                Shortcut = "S",
                MenuLabel = "start a new game: ",
                MethodToRun = newGameMethod
            },
            new MenuItem() {
                Shortcut = "L",
                MenuLabel = "load game",
                MethodToRun = loadGameMethod
            }
        });
    }
    
}