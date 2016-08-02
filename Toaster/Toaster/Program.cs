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
using NLua;
namespace Toaster
{
    class Program
    {
		/*
		 * Toaster
		 * Version 1.3
		 * Copyright 2016 Carver Harrison
		 * By: Carver, William and Michael
		*/
		public static NetObjectServer _server = null;
        public static IPAddress Thingip = IPAddress.Parse("0.0.0.0");
        public static void Main()
        {
			try
			{
				_server = new NetObjectServer();
				_server.OnReceived += new NetClientReceivedEventHandler<NetObject>(OnReceived);
				_server.OnClientConnected += (o, e) =>
				{
					int value = new Random().Next(0, 99999);
					guids.Add(value.ToString(), e.Guid);
					e.Reject = false;
					Console.WriteLine("Client Connected");
				};
				_server.OnClientAccepted += (o, a) =>
				{
					_server.DispatchTo(a.Guid, new NetObject("400", GetLocalId(a.Guid)));
				};
				_server.Start(Thingip, 13370);
				Console.WriteLine($"Server started. IP: {_server.Address} : {_server.Port}");
			}
			catch (Exception ex)
			{
				Console.WriteLine("[Error] " + ex);
			}
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
								string data = "# Directory of " + file_req.Split("ltp://".ToCharArray())[1] + "\n";
								foreach (string file in Directory.GetFiles(file_req))
								{
									data += $"- [ltp://{Thingip}/{file}]({file_req.Split("ltp://".ToCharArray())[1]})\n";
								}
								_server.DispatchTo(guids[replyto], new NetObject("100", data));
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
