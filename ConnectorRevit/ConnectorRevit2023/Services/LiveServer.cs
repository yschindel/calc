﻿using Calc.ConnectorRevit.Helpers;
using Calc.ConnectorRevit.ViewModels;
using System.Diagnostics;
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

            Mediator.Register("OnBuildupItemSelectionChanged",
                selectedItem => UpdateLiveVisualization((NodeViewModel)selectedItem));
        }

        private void UpdateLiveVisualization(NodeViewModel nodeItem)
        {
            if (Server == null) return;
            if (Server.ConnectedClients == 0) return;
            if (nodeItem == null) return;

            var results = CalculationHelper.Calculate(nodeItem);
            _ = Task.Run(async () => await Server.SendResults(results));
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
            Debug.WriteLine("Server stopped");
        }
    }
}
