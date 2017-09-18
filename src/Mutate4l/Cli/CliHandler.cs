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
                        if (await ClipProcessor.UdpConnector.TestCommunication())
                        {
                            Console.WriteLine("Communication with Ableton Live up and running!");
                        } else
                        {
                            Console.WriteLine("Error communicating with Ableton Live :(");
                        }
                        break;
                    default:
                        var lexer = new Lexer(command);
                        Command structuredCommand = Parser.ParseTokensToCommand(lexer.GetTokens());
                        var result = clipProcessor.ProcessCommand(structuredCommand);
                        break;
                }
            }
        }
    }
}
