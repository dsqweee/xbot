using Microsoft.Extensions.DependencyInjection;
using XBOT.Services.Configuration;

namespace XBOT.Services.Attribute;

public class UserPermission : PreconditionAttribute
{
    private readonly RolePermission _permissionName;
    public enum RolePermission : byte
    {
        Admin,
        Moder,
        Iventer
    }
    public UserPermission(RolePermission permissionName)
    {
        _permissionName = permissionName;
    }

    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        //var db = services.GetRequiredService<Db>();
        using var db = new Db();
        var guildUser = context.User as SocketGuildUser;

        if (guildUser.Id == BotSettings.xId)
            return PreconditionResult.FromSuccess();

        var settings = db.Settings.FirstOrDefault();

        if (!HasRequiredPermission(guildUser, settings))
        {
            var permissionTypeInString = GetPermissionTypeName(_permissionName);
            var errorMessage = $"Данная команда/модуль доступна только с правами {permissionTypeInString}.";
            return PreconditionResult.FromError(errorMessage);
        }

        return PreconditionResult.FromSuccess();

    }

    private bool HasRequiredPermission(SocketGuildUser user, Settings settings)
    {
        switch (_permissionName)
        {
            case RolePermission.Admin:
                return user.Roles.Any(role => role.Id == settings.AdminRoleId);
            case RolePermission.Moder:
                return user.Roles.Any(role => role.Id == settings.ModeratorRoleId || role.Id == settings.AdminRoleId);
            case RolePermission.Iventer:
                return user.Roles.Any(role => role.Id == settings.IventerRoleId || role.Id == settings.AdminRoleId);
            default:
                return false;
        }
    }


    private string GetPermissionTypeName(RolePermission permission)
    {
        return permission switch
        {
            RolePermission.Admin => "администратора",
            RolePermission.Moder => "модератора",
            RolePermission.Iventer => "ивентера"
        };
    }
}
