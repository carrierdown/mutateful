using Mutate4l.Cli;

namespace Mutate4l
{
    class Program
    {
        static void Main(string[] args)
        {
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
