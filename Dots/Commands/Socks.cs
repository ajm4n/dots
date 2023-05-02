using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dots.Models;

namespace Dots.Commands
{
    public class SocksConnectCommand : DotsCommand
    {
        public override string Name => "socks_connect";

        public override DotsProperties DotsProperty { get; set; }

        public override string Execute(string[] args)
        {
            Socket remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            int rep;
            var socksClientId = args[2];
            var atype = args[3];

            try
            {
                remote.Connect(args[0], int.Parse(args[1]));
                rep = 0;
            }
            catch (SocketException e)
            {
                switch (e.ErrorCode)
                {
                    case (int)SocketError.AccessDenied:
                        rep = 2;
                        break;
                    case (int)SocketError.NetworkUnreachable:
                        rep = 3;
                        break;
                    case (int)SocketError.HostUnreachable:
                        rep = 4;
                        break;
                    case (int)SocketError.ConnectionRefused:
                        rep = 5;
                        break;
                    default:
                        rep = 6;
                        break;
                }

                return JsonSerializer.Serialize(new
                {
                    remote = (object)null,
                    socks_client_id = socksClientId,
                    rep = rep.ToString(),
                    atype,
                    bind_addr = (object)null,
                    bind_port = (object)null
                });
            }

            var bindAddr = ((IPEndPoint)remote.LocalEndPoint).Address.ToString();
            var bindPort = ((IPEndPoint)remote.LocalEndPoint).Port.ToString();

            int remote_key = DotsProperty.AddRemote(remote);

            return JsonSerializer.Serialize(new
            {
                remote = remote_key.ToString(),
                socks_client_id = socksClientId,
                rep = rep.ToString(),
                atype,
                bind_addr = bindAddr,
                bind_port = bindPort
            });
        }
    }
    public class SocksDisconnect : DotsCommand
    {
        public override string Name => "socks_disconnect";
        public override DotsProperties DotsProperty { get; set; }
        public override string Execute(string[] args)
        {
            if(int.TryParse(args[0], out int index))
            {
                Socket remote = DotsProperty.Remotes[index];
                remote.Close();
            } else
            {
                throw new ArgumentException($"Invalid remote index {index}");
            }
            return "Closed remote socket";
        }
    }

    public class SocksDownstream : DotsCommand
    {
        public override string Name => "socks_downstream";
        public override DotsProperties DotsProperty { get; set; }

        public override string Execute(string[] args)
        {
            Socket remote;
            if (!int.TryParse(args[0], out int index))
            {
                throw new ArgumentException($"Invalid remote index {index}");
            }
            remote = DotsProperty.Remotes[index];
            List<byte> downstreamData = new List<byte>();
            string socksClientId = args[1];

            if (!remote.Poll(0, SelectMode.SelectRead))
            {
                return JsonSerializer.Serialize(new
                {
                    remote = index.ToString(),
                    socks_client_id = socksClientId,
                    downstream_data = Convert.ToBase64String(downstreamData.ToArray())
                });
            }
            byte[] buffer = new byte[4096];
            int bytesRead = remote.Receive(buffer);
            downstreamData.AddRange(buffer.Take(bytesRead));
            string downstreamDataString = Convert.ToBase64String(downstreamData.ToArray());
            return JsonSerializer.Serialize(new
            {
                remote = index.ToString(),
                socks_client_id = socksClientId,
                downstream_data = downstreamDataString
            });
        }
    }

    public class SocksUpstream : DotsCommand
    {
        public override string Name => "socks_upstream";
        public override DotsProperties DotsProperty { get; set; }

        public override string Execute(string[] args)
        {
            Socket remote;
            if (!int.TryParse(args[0], out int index))
            {
                throw new ArgumentException($"Invalid remote index {index}");
            }
            remote = DotsProperty.Remotes[index];
            byte[] upstreamData = Convert.FromBase64String(args[1]);
            remote.Send(upstreamData);
            return "sent succesfully";
        }
    }
}
