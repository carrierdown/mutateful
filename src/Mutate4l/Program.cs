using Mutate4l.Cli;
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
            try
            {
                CliHandler.Start();
            }
            finally
            {
                ClipProcessor.UdpConnector.Close();
            }
        }
    }
}
