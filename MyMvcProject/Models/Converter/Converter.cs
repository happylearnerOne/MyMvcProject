using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace MyMvcProject.Models.Converter
{
    public class Converter<T> where T : new()
    {
        public static bool Compare(T obj1, T obj2)
        {
            PropertyInfo[] ps = typeof(T).GetProperties();

            foreach (PropertyInfo p in ps)
            {
                if (null == p.GetValue(obj1))
                {
                    if (null != p.GetValue(obj2))
                        return false;
                }
                else if (!p.GetValue(obj2).Equals(p.GetValue(obj2)))
                    return false;
            }

            return true;
        }

        public static T ConvertToObject(object obj)
        {
            T t = new T();

            PropertyInfo[] targetProperties = typeof(T).GetProperties();
            PropertyInfo[] sourceProperties = obj.GetType().GetProperties();

            foreach (PropertyInfo i in targetProperties)
            {
                var sp = (from p in sourceProperties
                          where p.Name == i.Name
                          select p).FirstOrDefault();

                if (null == sp)
                    continue; // property not found

                if (i.PropertyType.Equals(sp.PropertyType))
                {
                    i.SetValue(t, sp.GetValue(obj), null);
                }
                else if (i.PropertyType.ToString().Contains("System.Nullable"))
                {
                    // 取得原始變數類型
                    string propertyType = i.PropertyType.ToString().Substring(i.PropertyType.ToString().IndexOf('[') + 1);
                    propertyType = propertyType.Remove(propertyType.LastIndexOf(']'));
                    i.SetValue(t, Convert.ChangeType(sp.GetValue(obj), TypeDelegator.GetType(propertyType)), null);
                }
                else
                {
                    i.SetValue(t, Convert.ChangeType(sp.GetValue(obj), i.PropertyType), null);
                }
            }

            return t;
        }

        public static T ConvertToObject(DataTable dt)
        {
            if (null != dt && dt.Rows.Count > 0)
                return ConvertToObject(dt.Rows[0]);
            else
                throw new Exception("DataTable is empty.");
        }

        public static T ConvertToObject(DataRow dr, string prefix = "")
        {
            T t = new T();

            PropertyInfo[] targetProperties = typeof(T).GetProperties();
            DataColumnCollection columns = dr.Table.Columns;

            foreach (PropertyInfo i in targetProperties)
            {
                var aa = i.PropertyType;
                Console.WriteLine(aa);
                string name = prefix + i.Name;
                if (columns.Contains(name))
                {
                    object value = dr[name];
                    if (value != DBNull.Value)
                    {
                        if (i.PropertyType == typeof(bool))
                        {
                            i.SetValue(t, (true.Equals(value) || "1".Equals(value) || "Y".Equals(value)), null);
                        }
                        else if (i.PropertyType.IsEnum)
                        {
                            i.SetValue(t, Enum.ToObject(i.PropertyType, value));
                        }
                        else
                        {
                            if (i.PropertyType.ToString().Contains("System.Nullable"))
                            {
                                string propertyType = i.PropertyType.ToString().Substring(i.PropertyType.ToString().IndexOf('[') + 1);
                                propertyType = propertyType.Remove(propertyType.LastIndexOf(']'));
                                i.SetValue(t, Convert.ChangeType(value, TypeDelegator.GetType(propertyType)), null);
                            }
                            else
                                i.SetValue(t, Convert.ChangeType(value, i.PropertyType), null);
                        }
                    }
                }
            }

            return t;
        }

        public static IEnumerable<T> ConvertToList(DataTable dt, string prefix = "")
        {
            List<T> list = new List<T>();

            if (null != dt && dt.Rows.Count > 0)
            {
                PropertyInfo[] targetProperties = typeof(T).GetProperties();
                DataColumnCollection columns = dt.Columns;
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(ConvertToObject(dr, prefix));
                }
            }

            return list;
        }

        public static Hashtable ConvertToTable(DataTable dt, string column, string sort_key)
        {
            Hashtable hashtable = new Hashtable();

            foreach (DataRow row in dt.Rows)
            {
                if (hashtable.ContainsKey(row[column]))
                {
                    if (hashtable[row[column]].GetType() == typeof(List<T>))
                    {
                        ((List<T>)hashtable[row[column]]).Add(ConvertToObject(row));
                    }
                    else
                    {
                        List<T> list = new List<T>();
                        list.Add((T)hashtable[row[column]]);
                        list.Add(ConvertToObject(row));
                        hashtable[row[column]] = list;
                    }
                }
                else
                    hashtable[row[column]] = ConvertToObject(row);
            }

            foreach (DictionaryEntry pair in hashtable)
            {
                if (pair.Value.GetType() == typeof(List<T>))
                {
                    var property = typeof(T).GetProperty(sort_key);
                    ((List<T>)pair.Value).Sort((x, y) =>
                    {
                        var col1 = property.GetValue(x);
                        var col2 = property.GetValue(y);
                        return Comparer<object>.Default.Compare(col1, col2);
                    });
                }
            }
            return hashtable;
        }

        public static T ConvertToAttribute(CustomAttributeData customAttributeData)
        {
            T t = new T();

            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo i in properties)
            {
                var argument = customAttributeData.NamedArguments.Where(p => p.MemberName == i.Name).FirstOrDefault();
                if (argument != null && argument.MemberInfo != null)
                {
                    i.SetValue(t, argument.TypedValue.Value);
                }
            }
            return t;
        }

        public static T Copy(T obj)
        {
            return ConvertToObject(obj);
        }

        public static List<T> ConvertJsonList(object obj)
        {
            string[] strings = obj as string[];
            List<T> list = new List<T>();
            foreach (string s in strings)
            {
                list.Add(JsonConvert.DeserializeObject<T>(s));
            }
            return list;
        }
    }
}
