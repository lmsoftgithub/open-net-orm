# Introduction #
OpenNETCF provided a quality, royalty free, open source ORM they used for the .NET Compact Framework with SqlCe and SQLite.
This project aims to extend the ORM to support more databases (though not all supported by CF anymore) and add more functionalities like many-to-many joins, result set sorting, etc.


---


## Disclaimer ##
This project doesn't claim to be better than the original, it doesn't change anything to the principle or the inner working. It just adds functionalities I've been looking for in the original ORM.


---


## Important ##
The project is in a really early stage. Most added functionalities have been tested in a real world scenario for SqlCe and MS-SQL though not for SQLite yet.
So please don't judge the project by it's current state.
I have another important project running at the moment, so this one will be pushed back for a few weeks.

### Update ###
08 oct 2012.
Yet another big update. I've fully implemented Dynamic Entities into the ORM, in the same idea as what Chris made on his side. Though, I did implement it in a different way and I think I pushed it a little further.
I've made big progresses in the tests and the Main Demo example program. It is now tested on Windows Desktop with SqlCe and SQLite. I'm working on MS-SQL, but due to work projects it will be pushed back to next week in the best case.

My current plan is:
  * finish MS-SQL tests and the Main Demo app at the same time.
  * implement the basics of Firebird SQL into the ORM.
  * create an automated test application to remove the need for me to manually test everything.