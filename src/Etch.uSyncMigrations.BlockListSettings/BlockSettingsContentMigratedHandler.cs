using System.Xml.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using uSync.Migrations.Notifications;
using uSync.Core;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Newtonsoft.Json.Linq;
using uSync.Migrations.Context;

namespace LimeWoodHotel.Website.Migrations;

public class BlockSettingsContentMigratedHandler : INotificationHandler<SyncMigratedNotification<Content>>
{
    private readonly IContentTypeService contentTypeService;

    public BlockSettingsContentMigratedHandler(IContentTypeService contentTypeService)
    {
        this.contentTypeService = contentTypeService;
    }

    public void Handle(SyncMigratedNotification<Content> notification)
    {
        var alias = this.GetContentType(notification.Xml);
        var contentType = this.contentTypeService.Get(alias);
        if (contentType == null)
        {
            return;
        }

        var properties = notification.Xml.Element("Properties")?.Elements() ?? Enumerable.Empty<XElement>();

        foreach (var property in properties)
        {
            if (this.PopulateBlockListSettings(property.Name.LocalName, property.Value, contentType, notification.Context, out var newValue))
            {
                property.ReplaceAll(new XElement("Value", new XCData(newValue ?? string.Empty)));
            }
        }
    }

    private bool PopulateBlockListSettings(string alias, string? value, IContentType contentType, SyncMigrationContext context, out string? newValue)
    {
        newValue = value;

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        var editorAlias = contentType.CompositionPropertyTypes
            .FirstOrDefault(pt => pt.Alias == alias)?
            .PropertyEditorAlias;
        if (editorAlias != Constants.PropertyEditors.Aliases.BlockList)
        {
            return false;
        }

        var model = JsonConvert.DeserializeObject<BlockValue>(value);
        if (model == null)
        {
            return false;
        }

        var settingsBlocks = new List<BlockItemData>();
        var layouts = model.Layout[Constants.PropertyEditors.Aliases.BlockList].ToObject<List<BlockListLayoutItem>>();

        foreach (var layout in layouts)
        {
            var contentBlock = model.ContentData.FirstOrDefault(c => c.Udi == layout.ContentUdi);
            if (contentBlock == null)
            {
                continue;
            }

            var blockContentType = this.contentTypeService.Get(contentBlock.ContentTypeKey);

            // recurse into block properties to populate settings in nested blocks
            if (blockContentType != null)
            {
                foreach (var property in contentBlock.RawPropertyValues)
                {
                    if (this.PopulateBlockListSettings(property.Key, property.Value?.ToString(), blockContentType, context, out var newBlockValue))
                    {
                        contentBlock.RawPropertyValues[property.Key] = newBlockValue;
                    }
                }
            }

            if (layout.SettingsUdi != null)
            {
                continue;
            }

            var contentBlockAlias = context.ContentTypes.GetAliasByKey(contentBlock.ContentTypeKey);
            var settingsBlockKey = context.ContentTypes.GetKeyByAlias($"{contentBlockAlias}Settings");
            var settingsBlockUdi = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
            var settingsBlock = new BlockItemData()
            {
                ContentTypeKey = settingsBlockKey,
                Udi = settingsBlockUdi,
                RawPropertyValues = contentBlock.RawPropertyValues,
            };

            settingsBlocks.Add(settingsBlock);
            layout.SettingsUdi = settingsBlockUdi;
        }

        model.SettingsData = settingsBlocks;
        model.Layout = new Dictionary<string, JToken>
            {
                { Constants.PropertyEditors.Aliases.BlockList, JArray.FromObject(layouts) },
            };

        newValue = JsonConvert.SerializeObject(model, Formatting.Indented);
        return true;
    }

    protected string GetContentType(XElement source)
        => source.Element("Info")?.Element("ContentType").ValueOrDefault(string.Empty) ?? string.Empty;
}
