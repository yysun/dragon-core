Dragon Core
============

Dragon Core is a simple and strong typed ORM tool for .NET Core.

> It is a port of the [Dragon](http://github.com/yysun/dragon) ORM tool for .NET framework.

How to Use
==========

```C#
Database database = Database.Open(<...connection string...>);
var users = database.Query<TestUser>("select UserId, Email from UserProfile where UserId=@UserId", new { UserId = 2 }).ToList();
Assert.AreEqual(2, users[0].UserId);
Assert.AreEqual("user@company.com", users[0].Email);
```

API
================
The Database Class has methods: _Open_, _Execute_, _QueryValue_ and _Query_.

* **Open**: opens database connection using a connection string.
* **Execute**: creates a command and runs ExecuteNonQuery.
* **QueryValue**: creates a command and runs ExecuteScalar.
* **Query**: creates a command with SQL statement(s) or stored procedure name and creates objects. The Query method supports up to 3 multiple record sets. The result objects are returned in a Tuple, E.g.

```C#
Database database = Database.Open(<...connection string...>);
var (users, members, roles) = database.Query<TestUser, TestMemberShip, TestUserRole>(@"
  select * from dbo.UserProfile
  Select * from dbo.webpages_Membership
  select * from dbo.webpages_Roles");

Assert.AreEqual(1, users.ToList()[0].UserId);
Assert.AreEqual(0, members.Count());
Assert.AreEqual("Sysadmin", roles.ToList()[0].RoleName);
```

Source Code
===========
https://github.com/yysun/dragon-core

(C) Copyright 2020, Yiyi Sun

