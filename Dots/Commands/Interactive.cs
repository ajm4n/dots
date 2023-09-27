using System.Threading;
using Dots.Models;

namespace Dots.Commands
{
    public class InteractiveCommand : DotsCommand
    {
        public override string Name => "interactive";
        public override DotsProperties DotsProperty { get; set; }

        public override string Execute(string[] args)
        {
            if (DotsProperty.Interactive)
            {
                DotsProperty.SocketIOClient.DisconnectAsync();
                DotsProperty.Interactive = false;
                DotsProperty.ProcessTasks = true;
                DotsProperty.TaskManager.Start(DotsProperty);
                return "Interactive Stopped";
            } else
            {
                try
                {
                    DotsProperty.SocketIOClient.ConnectAsync();
                    DotsProperty.Interactive = true;
                    DotsProperty.ProcessTasks = false;
                } catch {
                    return "Failed to connect";
                }
                return "Interactive Started";
            }
        }
    }
}
