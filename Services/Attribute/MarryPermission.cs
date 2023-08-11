using Microsoft.Extensions.DependencyInjection;

namespace XBOT.Services.Attribute;

public class MarryPermission : PreconditionAttribute
{
    public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        var db = services.GetRequiredService<Db>();

        var SocketContext = context as SocketCommandContext;
        var UserMarry = await db.GetUser(context.User.Id);

        if (command.Name == "marry" && UserMarry.MarriageId != null)
            return await Task.FromResult(PreconditionResult.FromError($"Вы не можете жениться, так как вы уже женаты!"));
        else if (command.Name == "divorce" && UserMarry.MarriageId == null)
            return await Task.FromResult(PreconditionResult.FromError($"Вы не можете развестись, так как вы не женаты!"));

        return await Task.FromResult(PreconditionResult.FromSuccess());

    }
}
