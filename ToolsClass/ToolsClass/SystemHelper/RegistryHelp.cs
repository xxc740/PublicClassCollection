using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Reflection;

namespace ToolsClass.SystemHelper
{
    /// <summary>
    /// 可用于注册表快捷保存读取的接口
    /// </summary>
    public interface IRegistry { }

    public static class RegistryExitensionMethods
    {
        private static readonly Type IgnoreAttribute = typeof (RegistryIgnoreAttribute);
        private static readonly Type MemberAttribute = typeof (RegistryMemberAttribute);
        private static readonly Type RegistryInterface = typeof (IRegistry);

        /// <summary>
        /// 读取注册表
        /// </summary>
        /// <param name="rootKey"></param>
        /// <param name="registry"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Read(this RegistryKey rootKey, IRegistry registry, string name = null)
        {
            if(registry == null)
                throw new ArgumentException(nameof(registry));

            if (rootKey == null)
            {
               return false;
            }

            rootKey = rootKey.OpenSubKey(name==null?"": registry.GetRegistryRootName());

            using (rootKey)
            {
                Tuple<PropertyInfo[], FieldInfo[]> members = registry.GetMembers();
                if(members.Item1.Length + members.Item2.Length == 0)
                    return false;

                foreach (PropertyInfo property  in members.Item1)
                {
                    string registryName = property.GetRegistryName();
                    object value;
                    if (RegistryInterface.IsAssignableFrom(property.PropertyType))
                    {
                        if (property.PropertyType == registry.GetType())
                        {
                            throw new Exception("Member type is same with Class type.");
                        }

                        object oldValue = property.GetValue(registry, null);
                        value = oldValue ?? Activator.CreateInstance(property.PropertyType);
                        if (!rootKey.Read((IRegistry) value, registryName)) value = null;
                    }
                    else
                    {
                        value = rootKey.GetValue(registryName);
                    }

                    if(value != null)
                        property.SetValue(registry,ChangeType(value,property.PropertyType),null);
                }

                foreach (FieldInfo fieldInfo in members.Item2)
                {
                    string registryName = fieldInfo.GetRegistryName();
                    object value;
                    if (RegistryInterface.IsAssignableFrom(fieldInfo.FieldType))
                    {
                        if (fieldInfo.FieldType == registry.GetType())
                        {
                            throw new Exception("Member type is same with Class type.");
                        }

                        value = fieldInfo.GetValue(registry) ?? Activator.CreateInstance(fieldInfo.FieldType);
                        rootKey.Read((IRegistry) value, registryName);
                    }
                    else
                    {
                        value = rootKey.GetValue(registryName);
                    }

                    if(value != null)
                        fieldInfo.SetValue(registry,ChangeType(value,fieldInfo.FieldType));
                }
            }

            return true;
        }

        /// <summary>
        /// 保存到注册表
        /// </summary>
        /// <param name="rootKey"></param>
        /// <param name="registry"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Save(this RegistryKey rootKey, IRegistry registry, string name = null)
        {
            if(registry == null)
                throw new ArgumentException(nameof(registry));

            if (rootKey == null)
                return false;

            using (rootKey)
            {
                Tuple<PropertyInfo[], FieldInfo[]> members = registry.GetMembers();
                if(members.Item1.Length + members.Item2.Length == 0)
                    return false;

                foreach (PropertyInfo property in members.Item1)
                {
                    string registryName = property.GetRegistryName();
                    object value = property.GetValue(registry, null);
                    if (RegistryInterface.IsAssignableFrom(property.PropertyType))
                    {
                        if (property.PropertyType == registry.GetType())
                        {
                            throw new Exception("Member type is same with Class type.");
                        }

                        if (value != null)
                            rootKey.Save((IRegistry) value, registryName);
                    }
                    else
                    {
                        value = ChangeType(value, TypeCode.String);
                        if(value != null)
                            rootKey.SetValue(registryName,value,RegistryValueKind.String);
                    }
                }

                foreach (FieldInfo fieldInfo in members.Item2)
                {
                    string registryName = fieldInfo.GetRegistryName();
                    object value = fieldInfo.GetValue(registry);
                    if (RegistryInterface.IsAssignableFrom(fieldInfo.FieldType))
                    {
                        if (fieldInfo.FieldType == registry.GetType())
                        {
                            throw new Exception("Member type is same with Class type.");
                        }

                        if (value != null)
                            rootKey.Save((IRegistry) value, registryName);
                    }
                    else
                    {
                        value = ChangeType(value, TypeCode.String);
                        if(value != null)
                            rootKey.SetValue(registryName,value,RegistryValueKind.String);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 读取注册表
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="rootKey"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Read(this IRegistry registry, RegistryKey rootKey, string name = null)
        {
            return rootKey.Read(registry, name);
        }

        /// <summary>
        /// 保存到注册表
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="rootKey"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Save(this IRegistry registry, RegistryKey rootKey, string name = null)
        {
            return rootKey.Save(registry, name);
        }

        private static object ChangeType(object value, Type conversionType)
        {
            if (conversionType == RegistryInterface)
            {
                return value;
            }

            if (conversionType == typeof (Guid))
            {
                return new Guid((string)value);
            }

            return Convert.ChangeType(value, conversionType);
        }

        private static object ChangeType(object value, TypeCode typeCode)
        {
            if (value is IConvertible) return Convert.ChangeType(value, typeCode);
            return value.ToString();
        }

        private static bool IsDefinedRegistryAttribute(this MemberInfo memberInfo)
        {
            return memberInfo.IsDefined(IgnoreAttribute, false) || memberInfo.IsDefined(MemberAttribute, false);
        }

        private static string GetRegistryRootName(this IRegistry registry)
        {
            Type rootType = registry.GetType();
            return rootType.IsDefined(typeof(RegistryRootAttribute), false) ? rootType.GetCustomAttribute<RegistryRootAttribute>().Name : rootType.Name;
        }

        private static string GetRegistryName(this MemberInfo memberInfo)
        {
            return memberInfo.IsDefined(MemberAttribute, false) ? memberInfo.GetCustomAttribute<RegistryMemberAttribute>().Name : memberInfo.Name;
        }

        private static Tuple<PropertyInfo[], FieldInfo[]> GetMembers(this IRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            Type t = registry.GetType();
            IList<MemberInfo> lst = t.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty).ToList();
            bool isDefinedAttribute = lst.Any(i => i.IsDefinedRegistryAttribute());
            return isDefinedAttribute ? new Tuple<PropertyInfo[], FieldInfo[]>(lst.OfType<PropertyInfo>().ToArray(), lst.OfType<FieldInfo>().ToArray()) : new Tuple<PropertyInfo[], FieldInfo[]>(t.GetProperties(), t.GetFields());
        }
    }

    /// <summary>
    /// RegistryRootAttribute:用于描述注册表对象类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegistryRootAttribute : Attribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    ///  RegistryIgnoreAttribute:忽略注册表成员,使用了该特性，将忽略未定义RegistryMemberAttribute的成员
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RegistryIgnoreAttribute : Attribute { }

    /// <summary>
    ///  RegistryMemberAttribute：用于描述注册表成员,使用了该特性，将忽略未定义RegistryMemberAttribute的成员
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RegistryMemberAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
