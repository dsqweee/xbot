using System.Runtime.CompilerServices;
using XBOT.Services.Attribute.CommandList;

namespace XBOT.Services.Attribute
{
    public class AliasesAttribute : AliasAttribute
    {
        public AliasesAttribute([CallerMemberName] string memberName = "") : base(Initiliaze.Load(memberName.ToLowerInvariant()).Usage.Skip(1).ToArray()) { }
    }
}
