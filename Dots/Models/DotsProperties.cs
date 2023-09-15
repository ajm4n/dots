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
        private Random rand = new Random();

        public ConcurrentDictionary<int, Socket> Remotes = new ConcurrentDictionary<int, Socket>();
        public List<Object> Commands = new List<Object>();
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
            int sleep = rand.Next(2000, 6001);
            int jitter = rand.Next(20, 51);

            double jitterAmount = sleep * (jitter / 100.0);
            return sleep + (int)jitterAmount;
        }
    }
}
