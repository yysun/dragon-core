using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace Dragon
{
    public class Database
    {
        public static Database Open(string name)
        {
            //return new Database(ConnectionStringHelper.GetConnectionString(name));
            return new Database(name);
        }

        #region Database Connection String and Command
        protected string connectionString;

        public Database(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public SqlCommand CreateCommand(string name, object parameters, SqlConnection connection)
        {
            var cmd = new SqlCommand(name, connection);
            if (parameters != null)
            {
                Type type = parameters.GetType();
                //var properties = parameters.GetType().GetProperties();
                var properties = propertyInfoCache[type];
                foreach (var property in properties)
                {
                    //FastPropertyInfo needs update to support anonymous class
                    //cmd.Parameters.AddWithValue("@" + property.Name,
                    //    property.GetValue(parameters, null) ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@" + property.Name,
                        type.GetProperty(property.Name).GetValue(parameters, null) ?? DBNull.Value);
                }
            }
            return cmd;
        }
        #endregion

        #region Execute and Query
        static PropertyInfoCache propertyInfoCache = new PropertyInfoCache();

        private IEnumerable<T> CreateObjects<T>(SqlDataReader reader)
        {
            IList<T> al = new List<T>();
            while (reader.Read())
            {
                Type EntityType = typeof(T);
                //var properties = EntityType.GetProperties();
                var properties = propertyInfoCache[typeof(T)];
                T newobj = (T)System.Activator.CreateInstance(EntityType);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string fieldName = reader.GetName(i);
                    object propertyObject = reader[fieldName];
                    if (propertyObject is System.DBNull) continue;

                    //if (properties.Any(p => p.Name == fieldName))
                    //{
                    //    object[] parameter = new object[1] { propertyObject };
                    //    EntityType.InvokeMember(fieldName, BindingFlags.Default | BindingFlags.SetProperty,
                    //        null, newobj, parameter,
                    //        System.Globalization.CultureInfo.InvariantCulture);
                    //}

                    var pinfo = properties.Where(p=>p.Name == fieldName).FirstOrDefault();
                    if(pinfo!=null) pinfo.SetValue(newobj, propertyObject, null);
                }
                al.Add(newobj);
            }
            return al;
        }

        public int Execute(string name, object parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var command = CreateCommand(name, parameters, conn);
                return command.ExecuteNonQuery();
            }
        }

        public object QueryValue(string name, object parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var command = CreateCommand(name, parameters, conn);
                return command.ExecuteScalar();
            }
        }

        public IEnumerable<T> Query<T>(string name, object parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var command = CreateCommand(name, parameters, conn);
                SqlDataReader reader = command.ExecuteReader();
                return CreateObjects<T>(reader);
            }
        }

        public Tuple<IEnumerable<T>, IEnumerable<U>> Query<T, U>(string name, object parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var command = CreateCommand(name, parameters, conn);
                SqlDataReader reader = command.ExecuteReader();
                var item1 = CreateObjects<T>(reader);
                reader.NextResult();
                var item2 = CreateObjects<U>(reader);
                return Tuple.Create<IEnumerable<T>, IEnumerable<U>>(item1, item2);
            }
        }

        public Tuple<IEnumerable<T>, IEnumerable<U>, IEnumerable<V>> Query<T, U, V>(string name, object parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var command = CreateCommand(name, parameters, conn);
                SqlDataReader reader = command.ExecuteReader();
                var item1 = CreateObjects<T>(reader);
                reader.NextResult();
                var item2 = CreateObjects<U>(reader);
                reader.NextResult();
                var item3 = CreateObjects<V>(reader);
                return Tuple.Create<IEnumerable<T>, IEnumerable<U>, IEnumerable<V>>(item1, item2, item3);
            }
        }
        #endregion

        #region Property Info Cache
        class PropertyInfoCache
        {
            private Dictionary<Type, IList<FastPropertyInfo>> cache =
                       new Dictionary<Type, IList<FastPropertyInfo>>();

            private object lock_obj = new object();

            public IList<FastPropertyInfo> this[Type key]
            {
                get
                {
                    if (!cache.ContainsKey(key))
                    {
                        lock (lock_obj)
                        {
                            var properties = key.GetProperties().Select(p => new FastPropertyInfo(p));
                            cache[key] = properties.ToList();
                        }
                    }
                    return cache[key];
                }
            }
        }
        #endregion
    }
}
