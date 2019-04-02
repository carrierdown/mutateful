using Mutate4l.Cli;
using System;
using System.Globalization;

namespace Mutate4l
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            CliHandler.Start();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
