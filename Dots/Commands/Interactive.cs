using System.Threading;
using Dots.Models;

namespace Dots.Commands
{
    public class InteractiveCommand : DotsCommand
    {
        public override string Name => "interactive";
        public override DotsProperties DotsProperty { get; set; }

        public override string Execute(TaskRequest task)
        {
            if (DotsProperty.SocketIOClient.Connected)
            {
                DotsProperty.SocketIOClient.DisconnectAsync();
                DotsProperty.RetrieveTasks = true;
                DotsProperty.TaskManager.RetrieveTasks();
                return "Interactive Stopped";
            } else
            {
                DotsProperty.SocketIOClient.ConnectAsync();
                DotsProperty.RetrieveTasks = false;
                return "Interactive Started";
            }
        }
    }
}
