using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Dots.Models
{
    public class DotsProperties
    {
        public double Sleep = new Random().NextDouble() * (10.0 - 5.0) + 5.0;
        public double Jitter = new Random().NextDouble() * (80.0 - 20.0) + 20.0;
        //public int Delay => (int)(Sleep * Jitter);
        public int Delay => 0;

        public ConcurrentDictionary<int, Socket> Remotes = new ConcurrentDictionary<int, Socket>();
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
    }
}
