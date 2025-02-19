﻿using Microsoft.Extensions.DependencyInjection;

namespace XBOT.Services.Attribute;

sealed class BirthDatePermission : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    { 
        var _db = services.GetRequiredService<Db>();
        //using var db = new Db();
        var userDb = await _db.GetUser(context.User.Id);
        if(userDb.BirthDate.Year == 1)
            return PreconditionResult.FromSuccess();

        string Reason = "Вы уже указали свою дату рождения. Повторно сменить ее невозможно.";
        return PreconditionResult.FromError(Reason);
    }
}
