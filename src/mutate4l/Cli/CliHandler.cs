using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l
{
    class CliHandler
    {
        public static void Start()
        {
            var command = "";
            Lexer lexer;
            ClipProcessor clipProcessor = new ClipProcessor();

            while (command != "q" && command != "quit")
            {
                if (command == "h" || command == "help")
                {
                    Console.WriteLine("Help text coming here");
                }
                Console.WriteLine("Mutate4L: Enter command, or [l]ist commands | [h]elp | [q]uit");
                Console.Write("> ");
                command = Console.ReadLine();
                lexer = new Lexer(command);
                Command structuredCommand = Parser.ParseTokensToCommand(lexer.GetTokens());
                clipProcessor.ProcessCommand(structuredCommand);
            }
        }
    }
}
