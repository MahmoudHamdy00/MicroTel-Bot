﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class GetBillInfoDialogBotComponent : BotComponent
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Anything that could be done in Startup.ConfigureServices can be done here.
        // In this case, the MultiplyDialog needs to be added as a new DeclarativeType.
        services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<GetBillInfo>(GetBillInfo.Kind));
    }
}