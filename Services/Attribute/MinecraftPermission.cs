using Microsoft.Extensions.DependencyInjection;

namespace XBOT.Services.Attribute;

sealed class MinecraftPermission : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        var db = services.GetRequiredService<Db>();

        var Date = db.Settings.FirstOrDefault().MinecraftOpen;
        if (Date > DateTime.Now)
            return PreconditionResult.FromError("Майнкрафт сервер скоро откроется...");

        return PreconditionResult.FromSuccess();
    }
}
