using System;
using Microsoft.Data.SqlClient;
using System.Linq;
using Dragon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DragonCore.Tests
{
	[TestClass()]
	public class DatabaseTest
	{
		#region Test Context
		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		static string connectionString = "Server=127.0.0.1;Database=test;User Id=sa;Password=<YourStrong@Passw0rd>;";

		#region Additional test attributes
		//
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			//var appData = (string) AppDomain.CurrentDomain.GetData("DataDirectory");
			//if (string.IsNullOrWhiteSpace(appData))
			//{
			//	appData = Environment.CurrentDirectory;
			//	AppDomain.CurrentDomain.SetData("DataDirectory", appData);
			//}
			//filename = System.IO.Path.Combine(appData, "Test.mdf");
			//if (!System.IO.File.Exists(filename)) CreateSqlExpressDatabase(filename);

			CreateSqlExpressDatabase("test");
		}
		#endregion
		#endregion

		#region create database
		public static void CreateSqlExpressDatabase(string databaseName)
		{
			bool new_db = false;
			using (var connection = new SqlConnection("Server=127.0.0.1;Database=master;" + "User Id=sa;Password=<YourStrong@Passw0rd>"))
			{
				connection.Open();
				using (var command = new SqlCommand($"SELECT db_id('{databaseName}')", connection))
				{
					if (command.ExecuteScalar() == DBNull.Value)
					{
						command.CommandText = "CREATE DATABASE " + databaseName;
						command.ExecuteNonQuery();
						new_db = true;
					}
				}
			}
			if (!new_db) return;
			var sql = @"
				IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserProfile]') AND type in (N'U')) DROP TABLE [dbo].[UserProfile];
				IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[webpages_Membership]') AND type in (N'U')) DROP TABLE [dbo].[webpages_Membership];
				IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[webpages_Roles]') AND type in (N'U')) DROP TABLE [dbo].[webpages_Roles];

				CREATE TABLE [dbo].[UserProfile](
					[UserId] [int] IDENTITY(1,1) NOT NULL,
					[Email] [nvarchar](56) NOT NULL
				) ON [PRIMARY];

				CREATE TABLE [dbo].[webpages_Membership](
					[UserId] [int] NOT NULL,
					[CreateDate] [datetime] NULL,
					[ConfirmationToken] [nvarchar](128) NULL,
					[IsConfirmed] [bit] NULL,
					[LastPasswordFailureDate] [datetime] NULL,
					[PasswordFailuresSinceLastSuccess] [int] NOT NULL,
					[Password] [nvarchar](128) NOT NULL,
					[PasswordChangedDate] [datetime] NULL,
					[PasswordSalt] [nvarchar](128) NOT NULL,
					[PasswordVerificationToken] [nvarchar](128) NULL,
					[PasswordVerificationTokenExpirationDate] [datetime] NULL
				) ON [PRIMARY];

				CREATE TABLE [dbo].[webpages_Roles](
					[RoleId] [int] IDENTITY(1,1) NOT NULL,
					[RoleName] [nvarchar](256) NOT NULL
				) ON [PRIMARY];

				Insert into [dbo].[UserProfile] (Email) Values ('admin@company.com');
				Insert into [dbo].[UserProfile] (Email) Values ('user@company.com');

				Insert into [dbo].[webpages_Roles] (RoleName) Values ('Sysadmin');
				Insert into [dbo].[webpages_Roles] (RoleName) Values ('Admin');
				Insert into [dbo].[webpages_Roles] (RoleName) Values ('User');";

			var db = Database.Open(connectionString);
			db.Execute(sql);
			db.Execute(@"
				CREATE PROCEDURE [dbo].[GetAllUserInfo] AS
				BEGIN
					SET NOCOUNT ON;
					select * from dbo.UserProfile
					select * from dbo.webpages_Membership
					select * from dbo.webpages_Roles
				END");
		}

		#endregion

		Database database = Database.Open(connectionString);

		[TestMethod]
		public void TestSqlStatement()
		{
			var users = database.Query<TestUser>("select UserId, Email from UserProfile").ToList();
			Assert.AreEqual(1, users[0].UserId);
			Assert.AreEqual(2, users[1].UserId);
			Assert.AreEqual("user@company.com", users[1].Email);
		}

		[TestMethod]
		public void TestParamster_AnonymousClass()
		{
			var users = database.Query<TestUser>("select UserId, Email from UserProfile where UserId=@UserId",
				new { UserId = 2 }).ToList();

			Assert.AreEqual(2, users[0].UserId);
			Assert.AreEqual("user@company.com", users[0].Email);
		}

		[TestMethod]
		public void TestParamster_StaticClass()
		{
			var users = database.Query<TestUser>("select UserId, Email from UserProfile where UserId=@UserId",
				new TestUser { UserId = 2 }).ToList();

			Assert.AreEqual(2, users[0].UserId);
			Assert.AreEqual("user@company.com", users[0].Email);
		}

		[TestMethod]
		public void TestQueryValue()
		{
			var value = database.QueryValue("select 10");
			Assert.AreEqual(10, value);
		}

		[TestMethod]
		public void TestMultipleRecordSets()
		{
			var (users, members, roles) = database.Query<TestUser, TestMemberShip, TestUserRole>(
				@"select * from dbo.UserProfile
				  select * from dbo.webpages_Membership
				  select * from dbo.webpages_Roles");

			Assert.AreEqual(1, users.ToList()[0].UserId);
			Assert.AreEqual(0, members.Count());
			Assert.AreEqual("Sysadmin", roles.ToList()[0].RoleName);
		}

		[TestMethod]
		public void TestStoredProcedure()
		{
			var (users, members, roles) = database.Query<TestUser, TestMemberShip, TestUserRole>("GetAllUserInfo");

			Assert.AreEqual(1, users.ToList()[0].UserId);
			Assert.AreEqual(0, members.Count());
			Assert.AreEqual("Sysadmin", roles.ToList()[0].RoleName);
		}

		[TestMethod]
		public void TestRecord()
		{
			var users = database.Query<TestUserRecord>("select UserId, Email from UserProfile").ToList();
			Assert.AreEqual(1, users[0].UserId);
			Assert.AreEqual(2, users[1].UserId);
			Assert.AreEqual("user@company.com", users[1].Email);
		}

		[TestMethod]
		public void TestPositionalRecord()
		{
			var users = database.Query<TestUserPRecord>("select UserId, Email from UserProfile").ToList();
			Assert.AreEqual(1, users[0].UserId);
			Assert.AreEqual(2, users[1].UserId);
			Assert.AreEqual("user@company.com", users[1].Email);
		}
	}

	#region Test Classes
	public class TestUser
	{
		public int UserId { get; set; }
		public string Email { get; set; }
	}

	public class TestMemberShip
	{
		public int UserId { get; set; }
		public DateTime CreateDate { get; set; }
		public bool IsConfirmed { get; set; }
	}

	public class TestUserRole
	{
		public int UserId { get; set; }
		public string RoleName { get; set; }
	}
  #endregion

  #region Test Classes
  public record TestUserPRecord(int UserId, string Email);

	public record TestUserRecord
	{
		public int UserId { get; init; }
		public string Email { get; init; }
	}
  #endregion
}
