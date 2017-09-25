using Mutate4l.Dto;
using Mutate4l.IO;
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
                Console.WriteLine("Mutate4L: Enter command, or [l]ist commands | [h]elp | [q]uit");
                Console.Write("> ");
                command = Console.ReadLine();
                switch (command)
                {
                    case "help":
                    case "h":
                        Console.WriteLine("Help text coming here");
                        break;
                    case "quit":
                    case "q":
                        break;
                    case "hello":
                    case "test":
                        if (UdpConnector.TestCommunication())
                        {
                            Console.WriteLine("Communication with Ableton Live up and running!");
                        } else
                        {
                            Console.WriteLine("Error communicating with Ableton Live :(");
                        }
                        break;
                    default:
                        var lexer = new Lexer(command);

                        if (lexer.IsValidCommand())
                        {
                            ChainedCommand structuredCommand = Parser.ParseTokensToChainedCommand(lexer.GetTokens());
                            var result = clipProcessor.ProcessChainedCommand(structuredCommand);
                            if (result?.Success == false)
                            {
                                Console.WriteLine(result.ErrorMessage);
                            }
                        } else
                        {
                            Console.WriteLine($"Unknown command: {command}");






                        }
                        break;
                }
            }
        }
    }
}
