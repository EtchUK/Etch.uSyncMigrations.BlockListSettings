# Etch.uSyncMigrations.BlockListSettings

> 📘 Note
>
> This package currently depends on some unreleased changes to uSync Migrations. Once these changes are released, I will update the package reference back to the official uSync Migrations package

Addon for uSync Migrations that duplicates any Block List content to Block List settings, facilitating the use of Block List settings elements.

All block list element types are duplicated (with the suffix "Settings") and all block list content is included in both settings and content.

Once the migration is finished, you can delete properties from the Content Element or the Settings Element, choosing which property one you want to keep in each case.

How to use:

1. Install `Etch.uSyncMigrations.BlockListSettings`.
2. Run a uSync Migration including only Data Types and Content Types, and complete the import of these.
3. Repeat step 2. This is necessary for the content element types to be duplicated.
4. Run a Content migration and import
