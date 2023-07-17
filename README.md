# Etch.uSyncMigrations.BlockListSettings

> 📘 Note
>
> This package currently depends on some unreleased changes to uSync Migrations. Once these changes are released, I will update the package reference back to the official uSync Migrations package

Addon for uSync Migrations that duplicates any Block List content to Block List settings, facilitating the use of Block List settings elements.

All block list element types are duplicated (with the suffix "Settings") and all block list content is included in both settings and content.

Once the migration is finished, you can delete properties from the Content Element or the Settings Element, choosing which property you want to keep in each case.

How to use:

1. Install `Etch.uSyncMigrations.BlockListSettings`.
2. Run the uSync Migration "Convert files" step, and then click to import Settings (i.e. data types and content types only)
3. Reload the uSync Migrations page and go into the conversion, and click "Run conversion again", and re-import Settings. You have to do this conversion twice for the Settings doc types to be created.
4. Run a Content import
5. Manually go through each Block List Content and Settings element and decide which properties to keep on each.
