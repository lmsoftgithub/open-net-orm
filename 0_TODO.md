  * Implement One to One and Many to Many references.
  * Silverlight Mockup rewrite.
  * Oracle support in Full Framework.
  * Firebird support in Full Framework.
  * MySQL support in Full Framework.
  * CreateTable changes to better handle initial creation and updates.


Done:
  * Add transaction support for databases which support it. At the moment it only works under MS-SQL.
  * BulkInsert and BulkInsertOrUpdate to support arrays of items to simplify client application code.
  * CreateDatastore rewrite. I hacked very badly through the function to get it working as I wanted quickly but I want follow the initial principle and add another method CreateOrUpdateDatastore to handle my intended behavior.