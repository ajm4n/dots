using System;
using Dots.Models;

namespace Dots.Commands
{
    public class KillCommand : DotsCommand
    {
        public override string Name => "kill";
        public override DotsProperties DotsProperty { get ; set; }
        public override string Execute(string[] args)
        {
            DotsProperty.ExecuteTasks.Cancel();
            DotsProperty.ProcessTasks = false;
            if (DotsProperty.SocketIOClient.Connected)
            {
                DotsProperty.SocketIOClient.DisconnectAsync();
            }
            return "Finished";
        }
    }
}
