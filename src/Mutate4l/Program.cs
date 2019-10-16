using Mutate4l.Cli;
using System;
using System.Globalization;

namespace Mutate4l
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // todo: Check all clip functions for mutating operations - any mutating operations will cause problems with svg rendering.
            var customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            Console.WriteLine("Welcome to mutate4l!");
            Console.WriteLine("Open Ableton Live, drop mutate4l-connector.amxd onto one of the tracks, and start entering formulas.");
            CliHandler.Start();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
