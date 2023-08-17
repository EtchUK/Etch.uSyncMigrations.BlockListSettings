using Newtonsoft.Json;
using System.Xml.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using uSync.Core;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;

namespace Etch.uSyncMigrations.BlockListSettings;

public class BlockSettingsDataTypeMigratedHandler : INotificationHandler<SyncMigratedNotification<DataType>>
{
    private readonly IContentTypeService contentTypeService;

    public BlockSettingsDataTypeMigratedHandler(IContentTypeService contentTypeService)
    {
        this.contentTypeService = contentTypeService;
    }

    public void Handle(SyncMigratedNotification<DataType> notification)
    {
        // if this is a block list editor then extract the config from the XML and add new doc types to the context (if they haven't already been added) for settings elements, then add the settings element type keys to the data type config for each block
        var editorAlias = this.GetEditorAlias(notification.Xml);
        if (editorAlias != Constants.PropertyEditors.Aliases.BlockList)
        {
            return;
        }

        var configJson = this.GetConfig(notification.Xml);
        var config = JsonConvert.DeserializeObject<BlockListConfiguration>(configJson);

        foreach (var block in config.Blocks)
        {
            if (block.SettingsElementTypeKey.HasValue)
            {
                continue;
            }

            var contentType = this.contentTypeService.Get(block.ContentElementTypeKey);
            if (contentType == null)
            {
                continue;
            }
            var folder = this.contentTypeService.GetContainer(contentType.ParentId);
            var alias = $"{contentType.Alias}Settings";
            notification.Context.ContentTypes.TryGetCompositionsByAlias(contentType.Alias, out var compositionAliases);
            var settingsContentType = new uSync.Migrations.Models.NewContentTypeInfo
            {
                Alias = alias,
                Name = $"{contentType.Name} Settings",
                Description = $"Settings for {contentType.Name}",
                Folder = folder?.Name,
                Icon = "icon-settings",
                IsElement = true,
                Key = alias.ToGuid(),
                Properties = contentType.PropertyGroups
                    .SelectMany(pg => pg.PropertyTypes
                        .Select(pt =>
                        {
                            var dataType = notification.Context.DataTypes.GetByDefinition(pt.DataTypeKey);
                            return new uSync.Migrations.Models.NewContentTypeProperty
                            {
                                Alias = pt.Alias,
                                Name = pt.Name,
                                DataTypeAlias = dataType.DataTypeName,
                                OriginalEditorAlias = pt.PropertyEditorAlias,
                                TabAlias = pg.Alias,
                            };
                        }))
                    .ToList(),
                 CompositionAliases = compositionAliases?.ToList() ?? new List<string>(),
                 Tabs = contentType.PropertyGroups.Select(pg => new NewContentTypeTab
                 {
                    Name = pg.Name ?? pg.Alias,
                    Alias = pg.Alias,
                    SortOrder = pg.SortOrder,
                    Type = pg.Type.ToString()
                 })
            };

            notification.Context.ContentTypes.AddNewContentType(settingsContentType);

            block.SettingsElementTypeKey = settingsContentType.Key;
        }

        notification.Xml
            .Element("Config")?
            .ReplaceAll(new XCData(JsonConvert.SerializeObject(config)));
    }

    protected string GetEditorAlias(XElement source)
        => source.Element("Info")?.Element("EditorAlias").ValueOrDefault(string.Empty) ?? string.Empty;

    protected string GetConfig(XElement source)
        => source.Element("Config")?.ValueOrDefault(string.Empty) ?? string.Empty;

}
