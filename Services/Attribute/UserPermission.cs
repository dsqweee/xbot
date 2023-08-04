using Microsoft.EntityFrameworkCore;
using XBOT.Services.Configuration;

namespace XBOT.Services.Attribute
{
    public class UserPermission : PreconditionAttribute
    {
        private readonly RolePermission _permissionName;
        public enum RolePermission : byte
        {
            Admin,
            Moder,
            Iventer
        }
        public UserPermission(RolePermission permissionName) { 
            _permissionName = permissionName;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            using (var _db = new db())
            {

                string PermissionTypeInString = "";
                var GuildUser = context.User as SocketGuildUser;

                if(GuildUser.Id == BotSettings.xId)
                    return await Task.FromResult(PreconditionResult.FromSuccess());

                switch (_permissionName)
                {
                    case RolePermission.Admin:
                        {
                            var Settings = _db.Settings.Include(x => x.AdminRole).FirstOrDefault();
                            if (!GuildUser.Roles.Any(x => x.Id == Settings.AdminRoleId))
                                PermissionTypeInString = "администратора";
                        }
                        break;
                    case RolePermission.Moder:
                        {
                            var Settings = _db.Settings.Include(x => x.ModeratorRole).FirstOrDefault();
                            if (!GuildUser.Roles.Any(x => x.Id == Settings.ModeratorRoleId || x.Id == Settings.AdminRoleId))
                                PermissionTypeInString = "модератора";
                        }
                        break;
                    case RolePermission.Iventer:
                        {
                            var Settings = _db.Settings.Include(x => x.IventerRole).FirstOrDefault();
                            if (!GuildUser.Roles.Any(x => x.Id == Settings.IventerRoleId || x.Id == Settings.AdminRoleId))
                                PermissionTypeInString = "ивентера";
                        }
                        break;
                }

                if (!string.IsNullOrWhiteSpace(PermissionTypeInString))
                    return await Task.FromResult(PreconditionResult.FromError($"Данная команда/модуль, доступна только с правами {PermissionTypeInString}."));

                return await Task.FromResult(PreconditionResult.FromSuccess());
            }
        }
    }
}
