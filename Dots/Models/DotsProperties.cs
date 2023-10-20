using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using SocketIOClient;

namespace Dots.Models
{
    public class DotsProperties
    {
        private Random rand = new Random();
        public Dictionary<long, Socket> RemoteConnections = new Dictionary<long, Socket>();
        public Socket StreamServeListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public CancellationTokenSource ExecuteTasks = new CancellationTokenSource();
        public bool RetrieveTasks = true;
        public TaskManager TaskManager { get; set; }
        public SocketIO SocketIOClient { get; set; }
        public List<object> Commands { get; } = new List<object>();
        public int Sleep { get; set; } = -1;
        public int Jitter { get; set; } = -1;

        public int GenerateDelay()
        {
            if (Sleep < 0)
            {
                Sleep = rand.Next(2000, 6001);
            }

            if (Jitter < 0)
            {
                Jitter = rand.Next(20, 51);
            }

            double jitterAmount = Sleep * (Jitter / 100.0);
            return Sleep + (int)jitterAmount;
        }

        public void RegisterEventHandler(string eventName, Action<object> eventHandler)
        {
            SocketIOClient.On(eventName, response =>
            {
                eventHandler(response.GetValue<string>());
            });
        }

    }
}
