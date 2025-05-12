using SIEWlang.Core.Lexer;
using System.Text;
using SystemEnv = System.Environment;

namespace SIEWlang;

class Siew
{
    private static bool HadError = false;
    public void Start(string[] args) 
    { 
        if(args.Length > 1)
        {
            Console.WriteLine("Usage: siew [script-file]");
            SystemEnv.Exit(65);
        }else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    // Read the bytes of a file given a file path and runs it
    private void RunFile(string filePath)
    {
        // we use this method to read the files and not with bytes because in this way we will have not "" at the begininig or end of our source code
        string text = "";

        try
        {
            text = File.ReadAllText(filePath);
        }
        catch (Exception)
        {
            SystemEnv.Exit(65);
        }

        Run(text);
        if (HadError) return;
    }

    // Read each line and run it if valid
    private void RunPrompt()
    {
        for(;;)
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if(line is null)
            {
                break;
            }
            Run(line);
            HadError = false;
        }
    }

    // run the given source code
    private void Run(string source)
    {
        Lexer lexer = new(source);

        IEnumerable<Token> tokens = lexer.ScanTokens();
        
        foreach(var token in tokens)
        {
            Console.WriteLine(token.TokenType.ToString());
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void Report(int line, string where, string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[line {line}] Error {where} : {message}");
        HadError = true;
        Console.ForegroundColor = ConsoleColor.White;
    }
}
