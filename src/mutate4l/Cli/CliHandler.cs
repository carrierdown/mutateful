using Mutate4l.Dto;
using System;

namespace Mutate4l.Cli
{
    class CliHandler
    {
        public static void Start()
        {
            var command = "";
            var clipProcessor = new ClipProcessor();

            while (command != "q" && command != "quit")
            {
                if (command == "h" || command == "help")
                {
                    Console.WriteLine("Help text coming here");
                }
                Console.WriteLine("Mutate4L: Enter command, or [l]ist commands | [h]elp | [q]uit");
                Console.Write("> ");
                command = Console.ReadLine();
                var lexer = new Lexer(command);
                Command structuredCommand = Parser.ParseTokensToCommand(lexer.GetTokens());
                var result = clipProcessor.ProcessCommand(structuredCommand);
            }
        }
    }
}
