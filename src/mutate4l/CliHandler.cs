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

            while (command != "q" && command != "quit")
            {
                if (command == "h" || command == "help")
                {
                    Console.WriteLine("Help text coming here");
                }
                Console.WriteLine("Mutate4L: Enter command, or [l]ist commands | [h]elp | [q]uit");
                Console.Write("> ");
                command = Console.ReadLine();
            }
        }
    }
}
