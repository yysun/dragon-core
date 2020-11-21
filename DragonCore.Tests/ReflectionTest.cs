using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dragon;

namespace DragonCore.Tests
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public record PersonRecord
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }

    }


    public record PersonPositionalRecord(string FirstName, string LastName);

    [TestClass()]
    public class ReflectionTest
    {

        public void SetValue(IEnumerable<FastPropertyInfo> properties, object newobj, string fieldName, object value)
        {
            var pinfo = properties.Where(p=>p.Name == fieldName).FirstOrDefault();
            if(pinfo!=null) pinfo.SetValue(newobj, value, null);
        }

        [TestMethod]
        public void FastProperty_Should_Work_With_Class()
        {
            var properties = typeof(Person).GetProperties().Select(p => new FastPropertyInfo(p));
            var newobj = System.Activator.CreateInstance<Person>();

            SetValue(properties, newobj, "FirstName", "Hello");
            SetValue(properties, newobj, "LastName", "World");
            Assert.AreEqual("Hello", newobj.FirstName);
            Assert.AreEqual("World", newobj.LastName);
        }


        [TestMethod]
        public void FastProperty_Should_Work_With_Record()
        {
            var properties = typeof(PersonRecord).GetProperties().Select(p => new FastPropertyInfo(p));
            var newobj = System.Activator.CreateInstance<PersonRecord>();
            SetValue(properties, newobj, "FirstName", "Hello");
            SetValue(properties, newobj, "LastName", "World");
            Assert.AreEqual("Hello", newobj.FirstName);
            Assert.AreEqual("World", newobj.LastName);
        }

        [TestMethod]
        public void FastProperty_Should_Work_With_Positional_Record()
        {
            var t0 = typeof(Person);
            var t1 = typeof(PersonRecord);
            var t2 = typeof(PersonPositionalRecord);

            var cinfo = t0.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(cinfo);
            Assert.AreEqual(0, cinfo[0].GetParameters().Length);

            cinfo = t1.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(cinfo);
            Assert.AreEqual(0, cinfo[0].GetParameters().Length);

            cinfo = t2.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(cinfo);
            Assert.AreEqual(2, cinfo[0].GetParameters().Length);

            var newobj = (PersonPositionalRecord) System.Activator.CreateInstance(t2, new Object[]{"Hello", "World"});
            Assert.AreEqual("Hello", newobj.FirstName);
            Assert.AreEqual("World", newobj.LastName);
        }
    }
}