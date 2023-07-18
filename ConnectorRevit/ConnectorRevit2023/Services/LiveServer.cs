﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calc.ConnectorRevit.Services
{
    public class LiveServer
    {
        public CalcWebSocketServer Server { get; set; }


        public LiveServer()
        {
            Server = new CalcWebSocketServer("http://127.0.0.1:8184/");
            _ = Server.Start();
        }

        public void Start()
        {
            if (Server.ConnectedClients == 0)
            {
                Process.Start("https://herzogdemeuron.github.io/calc/");
            }
        }
        public void Stop()
        {
            _ = Task.Run(async () => await Server.Stop());
        }
    }
}
