﻿using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using uSync.Migrations.Notifications;

namespace LimeWoodHotel.Website.Migrations;

public class BlockSettingsNotificationComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<SyncMigratedNotification<DataType>, BlockSettingsDataTypeMigratedHandler>();
        builder.AddNotificationHandler<SyncMigratedNotification<Content>, BlockSettingsContentMigratedHandler>();
    }
}
