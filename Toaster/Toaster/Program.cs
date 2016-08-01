using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using NetSockets;

namespace Toaster
{
    class Program
    {
        public static NetObjectServer _server = null;

        public static void Main()
        {
            _server = new NetObjectServer();
            _server.OnReceived += new NetClientReceivedEventHandler<NetObject>(OnReceived);
            _server.OnClientConnected += (o, e) =>
            {
                Console.WriteLine("Client connected.");
                int value = new Random().Next(0, 99999);
                guids.Add(value.ToString(), e.Guid);
                e.Reject = false;
            };
            _server.OnClientAccepted += (o, a) =>
            {
                _server.DispatchTo(a.Guid, new NetObject("400", GetLocalId(a.Guid)));
            };
            _server.Start(13370);
            Console.WriteLine($"Server started. IP: {_server.Address} : {_server.Port}");
        }

        public static string GetLocalId(Guid guid)
        {
            foreach(var g in guids)
            {
                if (g.Value == guid)
                    return g.Key;
            }
            return "";
        }

        public static Dictionary<string, Guid> guids = new Dictionary<string, Guid>();

        private static void OnReceived(object sender, NetClientReceivedEventArgs<NetObject> e)
        {
            try
            {
                var header = e.Data.Name.Split(' ');
                string replyto = header[0];
                int statcode = Convert.ToInt32(header[1]);
                switch(statcode)
                {
                    case 102:
                        bool nf = false;
                        string file_req = e.Data.Object as string;
                        file_req = file_req.Replace("/", "\\");
                        if (file_req.EndsWith(".md"))
                        {
                            if (File.Exists("wwl" + file_req))
                            {
                                _server.DispatchTo(guids[replyto], new NetObject("100", File.ReadAllText("wwl" + file_req)));
                            }
                            else
                            {
                                nf = true;
                            }
                        }
                        else if(Directory.Exists("wwl" + file_req))
                        {
                            if(File.Exists("wwl" + file_req + "\\index.md"))
                            {
                                _server.DispatchTo(guids[replyto], new NetObject("100", File.ReadAllText("wwl" + file_req + "\\index.md")));
                            }
                            else
                            {
                                nf = true;
                            }
                        }
                        else
                        {
                            if (File.Exists("wwl" + file_req))
                            {
                                _server.DispatchTo(guids[replyto], new NetObject("101", File.ReadAllText("wwl" + file_req)));
                            }
                            else
                            {
                                nf = true;
                            }
                        }
                        if (nf == true)
                            _server.DispatchTo(guids[replyto], new NetObject("201", null));
                        break;
                }
            }
            catch
            {
                _server.DispatchAll(new NetObject("300", null));
            }
        }
    }
}
