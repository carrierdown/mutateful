using Mutate4l.Cli;
using System;
using System.Globalization;

namespace Mutate4l
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            Console.WriteLine("Welcome to mutateful!");
            Console.WriteLine("Open Ableton Live, drop mutateful-connector.amxd onto one of the tracks, and start entering formulas.");
            CliHandler.Start();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
