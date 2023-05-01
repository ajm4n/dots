using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dots.Models
{
    public class DotsTask
    {
        [JsonPropertyName("jsonrpc")]
        public string JSONRPC { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    public class TaskRequest : DotsTask
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("params")]
        public string[] Params { get; set; }
    }

    public class TaskResult : DotsTask
    {
        [JsonPropertyName("result")]
        public string Result { get; set; }
    }

    public class TaskError : DotsTask
    {
        public TaskErrorDetails ErrorDetails { get; set; }
    }

    public class TaskErrorDetails
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
