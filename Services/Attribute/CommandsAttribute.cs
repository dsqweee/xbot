using System.Runtime.CompilerServices;
using XBOT.Services.Attribute.CommandList;

namespace XBOT.Services.Attribute;

public class CommandsAttribute : CommandAttribute
{
    public CommandsAttribute([CallerMemberName] string memberName = "") : base(Initiliaze.Load(memberName.ToLowerInvariant()).Usage[0]) { }
}
