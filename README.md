Dragon Core
============

Dragon Core is a strong typed ORM tool for .NET 5.

> Use the [Dragon](http://github.com/yysun/dragon) ORM for .NET framework 2.x-4.x.

What's New
==========

* V 2.2 Supports .NET 5, C# 9 positional record types
* V 2.1 Supports .NET 5, C# 9 record types
* V 2.0 Supports .NET Core 3.1

Quick Start
===========

## Install

```
dotnet add package DragonCore
```

## Define C# Data Types

You can use positional record type from C# 9

```csharp
public record User (int UserId, string Email);
```

Or use record type

```C#
public record User
{
  public int UserId { get; init; }
  public string Email { get; init; }
}
```

Or just use regular C# class

```C#
public class User
{
  public int UserId { get; set; }
  public string Email { get; set; }
}
```


## Open the database with a connection string

```C#
Database database = Database.Open("Server=127.0.0.1;Database=test;User Id=<user-id>;Password=<YourStrong@Passw0rd>;");
```

## Query Database and Create Records/Objects

```C#
var users = database.Query<User>("select UserId, Email from UserProfile where UserId=@UserId", new { UserId = 2 }).ToList();
```

## Use the Data

```C#
Assert.AreEqual(2, users[0].UserId);
Assert.AreEqual("user@company.com", users[0].Email);
```

API
================
The Database class has four methods: _Open_, _Execute_, _QueryValue_, and _Query_.

* **Open**: opens database connection using a connection string.
* **Execute**: creates a command and runs ExecuteNonQuery.
* **QueryValue**: creates a command and runs ExecuteScalar.
* **Query**: creates a command with SQL statement(s) or stored procedure name and creates objects. The Query method supports up to 3 multiple record sets. The result objects are returned in a Tuple, E.g.

```C#
Database database = Database.Open(/*connection string*/);
var (users, members, roles) = database.Query<TestUser, TestMemberShip, TestUserRole>(@"
  select * from dbo.UserProfile
  Select * from dbo.webpages_Membership
  select * from dbo.webpages_Roles");

Assert.AreEqual(1, users.ToList()[0].UserId);
Assert.AreEqual(0, members.Count());
Assert.AreEqual("Sysadmin", roles.ToList()[0].RoleName);
```

Manage Transactions
===================

Use the [TransactionScope](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transactionscope?view=net-5.0), guaranteeing that database queries can commit or rollback as a single unit of work.

```
try
{
    Database database = Database.Open(/*connection string*/);
    using (TransactionScope scope = new TransactionScope())
    {
        database.execute(...);
        database.execute(...);
        database.execute(...);

        // The Complete method commits the transaction. If an exception has been thrown, the transaction is rolled back.
            scope.Complete();
        }
    }
    catch (TransactionAbortedException ex)
    {
        writer.WriteLine("TransactionAbortedException Message: {0}", ex.Message);
    }
}
```
Unit Test
=========

The unit test project creates a _test_ database on your SQL server and populates testing data. You can use an existing SQL Server / SQL Server LocalDB or use SQL Server from docker.

```
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=<YourStrong@Passw0rd>" -p 1433:1433 --name sql -h sql -d mcr.microsoft.com/mssql/server:2019-latest
```

Once you have identified the SQL server, change the connection string in DragonCore.Tests/DatabaseTest.cs.

```C#
static string connectionString = "Server=127.0.0.1;Database=test;User Id=<user-id>;Password=<YourStrong@Passw0rd>;";
```

And run the unit tests.

```
dotnet test
```

Source Code
===========
https://github.com/yysun/dragon-core

Pull Requests are welcome. Have fun coding.

(C) Copyright 2020, Yiyi Sun

