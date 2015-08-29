# Introduction #

If you consider downloading the current project, please take into account that it currently is in a beta status. It is being used under Windows Mobile with the Compact Framework and under Windows OS with the Full Framework but hasn't been tested with SQLite yet.


# Details #

I will update the project soon enough, but I'm currently busy with a more urgent project.

What has been added to the original project from OpenNETCF is:
  * Support for MS-SQL
  * Support for filtering of references based on a static trigger field (think of it as an "inactive" flag)
  * Support for multiple primary keys in tables
  * Sorting of the returned rows based on properties "Field" attributes
  * More FilterConditions
  * Quick and dirty Silverlight mockup to allow the use of the same classes in Silverlight based apps
  * CreateDatastore now supports **updates** of tables. This means that you can keep working with the current database content and add columns to it dynamically.

I will make more extensive tests and validations as soon as I get more time. Also, I need to strenghten the code and fix some things I don't like.