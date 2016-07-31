using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Toaster
{
    class Program
    {
        public static void Main()
        {

            while (true)
            {
                try
                {
                    IPAddress ipAd = IPAddress.Parse("0.0.0.0");
                    // use local m/c IP address, and 
                    // use the same in the client

                    /* Initializes the Listener */
                    TcpListener myList = new TcpListener(ipAd, 13370);

                    /* Start Listeneting at the specified port */
                    myList.Start();

                    Console.WriteLine("The local End point is: " +
                                      myList.LocalEndpoint);
                    Console.WriteLine("Waiting for a connection...");

                    Socket s = myList.AcceptSocket();
                    Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);
                    ASCIIEncoding asen = new ASCIIEncoding();

                    s.Send(File.ReadAllBytes("default.md"));
                    s.Send(new byte[] { 4 });

                    /* clean up */
                    s.Close();
                    myList.Stop();

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error! " + e.StackTrace);
                }
            }
        }
    }
}
