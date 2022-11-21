using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;
using System.Data;
using System.Reflection;
using Microsoft.Tools.TestClient.Variables;

namespace Microsoft.Tools.TestClient
{
    class DataContractAnalyzer
    {
        private static Type[] memberAttributes = new Type[] { typeof(DataMemberAttribute), typeof(MessageBodyMemberAttribute), typeof(MessageHeaderAttribute), typeof(MessageHeaderArrayAttribute), typeof(XmlAttributeAttribute), typeof(XmlElementAttribute), typeof(XmlArrayAttribute), typeof(XmlTextAttribute) };
        private static Type[] typeAttributes = new Type[] { typeof(DataContractAttribute), typeof(XmlTypeAttribute), typeof(MessageContractAttribute) };
        internal static IDictionary<string, Type> TypesCache = new Dictionary<string, Type>();
        private static IDictionary<string, SerializableType> serviceTypeInfoPool = new Dictionary<string, SerializableType>();

        internal ClientEndpointInfo AnalyzeDataContract(ClientEndpointInfo endpoint)
        {
            Type contractType = ClientSettings.ClientAssembly.GetType(endpoint.OperationContractTypeName);
            if (contractType == null)
            {
                endpoint.Valid = false;
                return endpoint;
            }
            
            object[] customAttributes = contractType.GetCustomAttributes(typeof(ServiceContractAttribute), true);
            
            if (((customAttributes != null) && (customAttributes.Length == 1)) && (((ServiceContractAttribute)customAttributes[0]).CallbackContract != null))
            {
                endpoint.Valid = false;
            }
            else
            {
                endpoint.Valid = true;
            }

            endpoint.ClientTypeName = GetContractTypeName(contractType);

            foreach (MethodInfo info in contractType.GetMethods())
            {
                bool isOneWay = false;
                object[] objArray2 = info.GetCustomAttributes(typeof(OperationContractAttribute), false);
                if ((objArray2.Length == 1) && ((OperationContractAttribute)objArray2[0]).IsOneWay)
                {
                    isOneWay = true;
                }
                ServiceMethodInfo item = new ServiceMethodInfo(info.Name, isOneWay);
                endpoint.Methods.Add(item);
                foreach (ParameterInfo info3 in info.GetParameters())
                {
                    TypeMemberInfo info4;
                    string name = info3.Name;
                    if (info3.ParameterType.IsByRef)
                    {
                        info4 = new TypeMemberInfo(name, CreateServiceTypeInfo(info3.ParameterType.GetElementType()));
                    }
                    else
                    {
                        info4 = new TypeMemberInfo(name, CreateServiceTypeInfo(info3.ParameterType));
                    }
                    if (info3.IsIn || !info3.IsOut)
                    {
                        item.InputParameters.Add(info4);
                    }
                    else
                    {
                        item.OtherParameters.Add(info4);
                    }
                }
                if ((info.ReturnType != null) && !info.ReturnType.Equals(typeof(void)))
                {
                    TypeMemberInfo info5 = new TypeMemberInfo("(return)", CreateServiceTypeInfo(info.ReturnParameter.ParameterType));
                    item.OtherParameters.Add(info5);
                }
            }
            return endpoint;
        }

        private static string GetContractTypeName(Type contractType)
        {
            foreach (Type type in ClientSettings.ClientAssembly.GetTypes())
            {
                if (contractType.IsAssignableFrom(type) && !type.IsInterface)
                {
                    return type.FullName;
                }
            }
            return null;
        }

        internal static Variable BuildVariableInfo(string name, object value)
        {
            SerializableType type2;
            if (value == null)
            {
                value = new NullObject();
            }
            Type type = value.GetType();
            string fullName = type.FullName;
            if (!serviceTypeInfoPool.TryGetValue(fullName, out type2))
            {
                type2 = CreateServiceTypeInfo(type);
            }
            TypeMemberInfo memberInfo = new TypeMemberInfo(name, type2);
            Variable variable = VariableFactory.CreateAssociateVariable(memberInfo, value);
            if (memberInfo.Members != null)
            {
                Variable[] variableArray;
                if (type.IsArray)
                {
                    Array array = (Array)value;
                    variableArray = new Variable[array.Length];
                    for (int i = 0; i < array.Length; i++)
                    {
                        object obj2 = array.GetValue(i);
                        variableArray[i] = BuildVariableInfo("[" + i + "]", obj2);
                    }
                }
                else if (IsCollectionType(type))
                {
                    ICollection is2 = (ICollection)value;
                    variableArray = new Variable[is2.Count];
                    int num2 = 0;
                    foreach (object obj3 in is2)
                    {
                        variableArray[num2++] = BuildVariableInfo("[" + num2 + "]", obj3);
                    }
                }
                else if (IsDictionaryType(type))
                {
                    IDictionary dictionary = (IDictionary)value;
                    variableArray = new Variable[dictionary.Count];
                    int num3 = 0;
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        variableArray[num3++] = BuildVariableInfo("[" + num3 + "]", entry);
                    }
                }
                else
                {
                    variableArray = new Variable[memberInfo.Members.Count];
                    int num4 = 0;
                    foreach (PropertyInfo info2 in type.GetProperties())
                    {
                        if ((IsSupportedMember(info2) || (value is DictionaryEntry)) || IsKeyValuePairType(type))
                        {
                            object obj4 = info2.GetValue(value, null);
                            variableArray[num4++] = BuildVariableInfo(info2.Name, obj4);
                        }
                    }
                    foreach (FieldInfo info3 in type.GetFields())
                    {
                        if (IsSupportedMember(info3))
                        {
                            object obj5 = info3.GetValue(value);
                            variableArray[num4++] = BuildVariableInfo(info3.Name, obj5);
                        }
                    }
                }
                variable.SetChildVariables(variableArray);
            }
            return variable;
        }

        internal static Variable[] BuildVariableInfos(object result, IDictionary<string, object> outValues)
        {
            Variable[] variableArray = new Variable[outValues.Count + 1];
            variableArray[0] = BuildVariableInfo("(return)", result);
            int num = 1;
            foreach (KeyValuePair<string, object> pair in outValues)
            {
                variableArray[num++] = BuildVariableInfo(pair.Key, pair.Value);
            }
            return variableArray;
        }

        private static SerializableType CreateServiceTypeInfo(Type type)
        {
            string fullName = type.FullName;
            if (serviceTypeInfoPool.ContainsKey(type.FullName))
            {
                return serviceTypeInfoPool[fullName];
            }
            bool isInvalid = false;
            SerializableType type2 = new SerializableType(type);
            serviceTypeInfoPool.Add(fullName, type2);
            if (type.IsArray)
            {
                SerializableType type3 = CreateServiceTypeInfo(type.GetElementType());
                isInvalid = type3.IsInvalid;
                type2.Members.Add(new TypeMemberInfo("[0]", type3));
            }
            else if (IsNullableType(type))
            {
                SerializableType type4 = CreateServiceTypeInfo(type.GetGenericArguments()[0]);
                isInvalid = type4.IsInvalid;
                type2.Members.Add(new TypeMemberInfo("Value", type4));
            }
            else if (IsCollectionType(type))
            {
                Type baseType = type.BaseType;
                if (baseType.IsGenericType)
                {
                    SerializableType type6 = CreateServiceTypeInfo(baseType.GetGenericArguments()[0]);
                    isInvalid = type6.IsInvalid;
                    type2.Members.Add(new TypeMemberInfo("[0]", type6));
                }
            }
            else if (IsDictionaryType(type))
            {
                Type[] genericArguments = type.GetGenericArguments();
                Type type7 = typeof(KeyValuePair<,>);
                SerializableType type9 = CreateServiceTypeInfo(type7.MakeGenericType(new Type[] { genericArguments[0], genericArguments[1] }));
                isInvalid = type9.IsInvalid;
                type2.Members.Add(new TypeMemberInfo("[0]", type9));
                if (!TypesCache.ContainsKey(fullName))
                {
                    TypesCache.Add(fullName, type);
                }
            }
            else if (IsKeyValuePairType(type))
            {
                Type[] typeArray4 = type.GetGenericArguments();
                SerializableType type10 = CreateServiceTypeInfo(typeArray4[0]);
                SerializableType type11 = CreateServiceTypeInfo(typeArray4[1]);
                isInvalid = type10.IsInvalid || type11.IsInvalid;
                type2.Members.Add(new TypeMemberInfo("Key", type10));
                type2.Members.Add(new TypeMemberInfo("Value", type11));
                if (!TypesCache.ContainsKey(fullName))
                {
                    TypesCache.Add(fullName, type);
                }
            }
            else if (IsSupportedType(type))
            {
                foreach (PropertyInfo info in type.GetProperties())
                {
                    if (IsSupportedMember(info) || (type == typeof(DictionaryEntry)))
                    {
                        SerializableType type12 = CreateServiceTypeInfo(info.PropertyType);
                        if (type12.IsInvalid)
                        {
                            isInvalid = true;
                        }
                        type2.Members.Add(new TypeMemberInfo(info.Name, type12));
                    }
                }
                foreach (FieldInfo info2 in type.GetFields())
                {
                    if (IsSupportedMember(info2))
                    {
                        SerializableType type13 = CreateServiceTypeInfo(info2.FieldType);
                        if (type13.IsInvalid)
                        {
                            isInvalid = true;
                        }
                        type2.Members.Add(new TypeMemberInfo(info2.Name, type13));
                    }
                }
            }
            if (isInvalid)
            {
                type2.MarkAsInvalid();
            }
            foreach (object obj2 in type.GetCustomAttributes(typeof(KnownTypeAttribute), false))
            {
                KnownTypeAttribute attribute = (KnownTypeAttribute)obj2;
                type2.SubTypes.Add(CreateServiceTypeInfo(attribute.Type));
            }
            foreach (object obj3 in type.GetCustomAttributes(typeof(XmlIncludeAttribute), false))
            {
                XmlIncludeAttribute attribute2 = (XmlIncludeAttribute)obj3;
                type2.SubTypes.Add(CreateServiceTypeInfo(attribute2.Type));
            }
            return type2;
        }

        private static bool HasAttribute(MemberInfo member, Type type)
        {
            return (member.GetCustomAttributes(type, true).Length > 0);
        }

        internal static bool IsCollectionType(Type currentType)
        {
            return (currentType.GetCustomAttributes(typeof(CollectionDataContractAttribute), true).Length > 0);
        }

        internal static bool IsDataSet(Type type)
        {
            Type o = typeof(DataSet);
            if (!type.Equals(o))
            {
                return o.IsAssignableFrom(type);
            }
            return true;
        }

        internal static bool IsDictionaryType(Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>)));
        }

        internal static bool IsKeyValuePairType(Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)));
        }

        internal static bool IsNullableType(Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        private static bool IsSupportedMember(MemberInfo member)
        {
            foreach (Type type in memberAttributes)
            {
                if (HasAttribute(member, type))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsSupportedType(Type currentType)
        {
            if (currentType == typeof(DictionaryEntry))
            {
                return true;
            }
            foreach (Type type in typeAttributes)
            {
                if (HasAttribute(currentType, type))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
