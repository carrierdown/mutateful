using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
