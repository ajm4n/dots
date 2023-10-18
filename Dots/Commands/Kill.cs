using System;
using Dots.Models;

namespace Dots.Commands
{
    public class KillCommand : DotsCommand
    {
        public override string Name => "kill";
        public override DotsProperties DotsProperty { get ; set; }
        public override string Execute(TaskRequest task)
        {
            DotsProperty.ExecuteTasks.Cancel();
            DotsProperty.RetrieveTasks = false;
            if (DotsProperty.SocketIOClient.Connected)
            {
                DotsProperty.SocketIOClient.DisconnectAsync();
            }
            return "Finished";
        }
    }
}
