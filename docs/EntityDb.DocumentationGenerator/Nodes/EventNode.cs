using System.Reflection;

namespace EntityDb.DocumentationGenerator.Nodes
{
    public class EventNode : MemberInfoNode
    {
        private readonly EventInfo eventInfo;

        public EventNode(EventInfo eventInfo) : base(eventInfo)
        {
            this.eventInfo = eventInfo;
        }
    }
}