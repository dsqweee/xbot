using Microsoft.EntityFrameworkCore;
using XBOT.DataBase.Models.Roles_data;

namespace XBOT.Services;

public static class GOC
{
    public static async Task<User> GetUser(this Db ctx, ulong userId)
    {
        var user = ctx.User.Include(x=>x.RefferalInvite).Include(x=>x.User_Warn).FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            user = new User { Id = userId, money = User.DefaultMoney };
            ctx.Add(user);
            await ctx.SaveChangesAsync();
        }
        return user;
    }

    public static async Task<TextChannel> GetTextChannel(this Db ctx, ulong textChannelId)
    {
        var textChannel = ctx.TextChannel.FirstOrDefault(u => u.Id == textChannelId);
        if (textChannel == null)
        {
            textChannel = new TextChannel { Id = textChannelId,giveXp = true,useCommand = true, useAdminCommand = true, useRPcommand = true };
            ctx.Add(textChannel);
            await ctx.SaveChangesAsync();
        }
        return textChannel;
    }

    public static async Task<Roles> GetRole(this Db ctx, ulong RoleId)
    {
        var role = ctx.Roles.FirstOrDefault(u => u.Id == RoleId);
        if (role == null)
        {
            role = new Roles { Id = RoleId };
            ctx.Add(role);
            await ctx.SaveChangesAsync();
        }

        return role;
    }
}
