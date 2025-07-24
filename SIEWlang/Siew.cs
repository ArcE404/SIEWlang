using SIEWlang.Core.Interpreter;
using SIEWlang.Core.Lexer;
using SIEWlang.Core.Parser;
using SIEWlang.Core.Semantics;
using SystemEnv = System.Environment;

namespace SIEWlang;

internal class Siew
{
    private static readonly Interpreter Interpreter = new();
    private static bool HadError = false;
    private static bool HadRuntimeError = false;

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void Error(Token token, string message)
    {
        if (token.TokenType == TokenType.EOF)
        {
            Report(token.Line, " at end", message);
        }
        else
        {
            Report(token.Line, " at '" + token.Lexeme + "'", message);
        }
    }

    public static void Report(int line, string where, string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[line {line}] Error {where} : {message}");
        HadError = true;
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void RuntimeError(RuntimeError error)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(error.Message + "\n[line " + error.Token.Line + "]");
        HadRuntimeError = true;
        Console.ForegroundColor = ConsoleColor.White;
    }

    public void Start(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: siew [script-file]");
            SystemEnv.Exit(65);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    // run the given source code
    private void Run(string source)
    {
        Lexer lexer = new(source);

        IEnumerable<Token> tokens = lexer.ScanTokens();

        Parser parser = new([.. tokens]);

        List<Stmt> statements = parser.Parse();

        if (HadError) return;

        Resolver resolver = new(Interpreter);

        resolver.Resolve(statements);

        if (HadError) return;

        Interpreter.Interpret(statements);
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

        // indicate an error exit code if so
        if (HadError) SystemEnv.Exit(65);
        if (HadRuntimeError) SystemEnv.Exit(70);
    }

    // Read each line and run it if valid
    private void RunPrompt()
    {
        for (; ; )
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line is null)
            {
                break;
            }
            Run(line);
            HadError = false;
        }
    }
}