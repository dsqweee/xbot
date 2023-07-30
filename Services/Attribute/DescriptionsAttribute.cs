using System.Runtime.CompilerServices;
using XBOT.Services.Attribute.CommandList;

namespace XBOT.Services.Attribute
{
    class DescriptionsAttribute : SummaryAttribute
    {
        public DescriptionsAttribute([CallerMemberName] string memberName = "") : base(Initiliaze.Load(memberName).Desc) { }
    }
}
