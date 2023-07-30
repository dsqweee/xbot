using XBOT.Services.Attribute;

namespace XBOT.Modules.Command
{
    [UserPermission(UserPermission.RolePermission.Iventer)]
    public class IventerModule : ModuleBase<SocketCommandContext>
    {
    }
}
