using SIEWlang.Core.Lexer;
using System.Text;

namespace SIEWlang;

class Siew
{
    public void Start(string[] args) 
    { 
        if(args.Length > 1)
        {
            Console.WriteLine("Usage: siew [script-file]");
            return;
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
        byte[] bytes = File.ReadAllBytes(filePath);
        Run(Encoding.UTF8.GetString(bytes));
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
        }
    }

    // run the given source code
    private void Run(string source)
    {
        Lexer lexer = new(source);

        IEnumerable<Token> tokens = lexer.ScanTokens();
        
        foreach(var token in tokens)
        {
            // do something with the tokens 
        }
    }
}
