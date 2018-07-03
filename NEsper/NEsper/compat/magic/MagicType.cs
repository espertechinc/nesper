///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.events;

namespace com.espertech.esper.compat.magic
{
    public class MagicType
    {
        /// <summary>
        /// Underlying type managed by magic type
        /// </summary>
        private readonly Type _type;
        /// <summary>
        /// MagicType for parent type
        /// </summary>
        private readonly MagicType _parent;
        /// <summary>
        /// Case insensitive property table
        /// </summary>
        private readonly IDictionary<string, SimpleMagicPropertyInfo> _ciPropertyTable =
            new Dictionary<string, SimpleMagicPropertyInfo>();
        /// <summary>
        /// Case sensitive property table
        /// </summary>
        private readonly IDictionary<string, SimpleMagicPropertyInfo> _csPropertyTable =
            new Dictionary<string, SimpleMagicPropertyInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MagicType"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public MagicType(Type type)
        {
            _type = type;

            var baseType = type.BaseType;
            if (baseType != null) {
                _parent = GetCachedType(baseType);
            }

            IndexIndexedProperties();
            IndexMappedProperties();
            IndexSimpleProperties();
        }

        /// <summary>
        /// Gets the type that magic type reflects.
        /// </summary>
        public Type TargetType
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the magic type for the base type.
        /// </summary>
        public MagicType BaseType
        {
            get { return _parent; }
        }

        /// <summary>
        /// Creates a new instance of the object.  Assumes a default constructor.
        /// </summary>
        /// <returns></returns>
        public Object New()
        {
            return Activator.CreateInstance(_type);
        }

        /// <summary>
        /// Returns true if this type is an extension of the provided
        /// baseType.  This method will recurse the type tree to determine
        /// if the extension occurs anywhere in the tree.  If not found,
        /// the method returns false.
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <returns></returns>
        public bool ExtendsType(Type baseType)
        {
            if (_type == baseType)
                return true;
            if (_parent == null)
                return false;
            return _parent.ExtendsType(baseType);
        }

        private static string GetPropertyName( string assumedName, ICustomAttributeProvider member )
        {
            var attributes = member.GetCustomAttributes(typeof(PropertyNameAttribute), true);
            if (attributes != null)
            {
                foreach (PropertyNameAttribute attribute in attributes)
                {
                    return attribute.Name;
                }
            }

            return assumedName;
        }

        private void AddProperty(string csName, string ciName, SimpleMagicPropertyInfo prop)
        {
            var ciProp = _ciPropertyTable.Get(ciName);
            if (ciProp != null) {
                ciProp.Next = prop;
            }
            else {
                _ciPropertyTable[ciName] = prop;
            }

            // It is possible for a property that is case sensitive to
            // exist in csPropertyTable, but not ciPropertyTable.  That's
            // the nature of case sensitive versus case insensitive.
            var csProp = _csPropertyTable.Get(csName);
            if (csProp != null) {
                csProp.Next = prop;
            }
            else {
                _csPropertyTable[csName] = prop;
            }
        }

        /// <summary>
        /// Indexes the simple properties.
        /// </summary>
        private void IndexSimpleProperties()
        {
            foreach (PropertyInfo propertyInfo in FetchSimpleProperties())
            {
                var csName = GetPropertyName(propertyInfo.Name, propertyInfo);
                var ciName = csName.ToUpper();
                var prop = new SimpleMagicPropertyInfo(
                    csName,
                    propertyInfo,
                    propertyInfo.GetGetMethod(),
                    propertyInfo.GetSetMethod(),
                    EventPropertyType.SIMPLE);

                AddProperty(csName, ciName, prop);
            }

            foreach (MethodInfo methodInfo in FetchSimpleAccessors())
            {
                var csName = GetAccessorPropertyName(methodInfo);
                var ciName = csName.ToUpper();
                var setter = GetSimpleMutator(csName, methodInfo.ReturnType);
                var prop = new SimpleMagicPropertyInfo(
                    csName,
                    methodInfo,
                    methodInfo,
                    setter,
                    EventPropertyType.SIMPLE);

                AddProperty(csName, ciName, prop);
            }
        }


        /// <summary>
        /// Indexes the mapped properties.
        /// </summary>
        private void IndexMappedProperties()
        {
#if false
            foreach (PropertyInfo propertyInfo in FetchMappedProperties())
            {
                var csName = GetPropertyName(propertyInfo.Name, propertyInfo);
                var ciName = csName.ToUpper();
                var prop = new SimpleMagicPropertyInfo(
                    csName,
                    propertyInfo,
                    propertyInfo.GetGetMethod(),
                    propertyInfo.GetSetMethod(),
                    EventPropertyType.MAPPED);

                AddProperty(csName, ciName, prop);
            }
#endif

            // Mapped properties exposed through accessors may be exposed with both
            // a GetXXX accessor and a SetXXX mutator.  We need to merge both thought
            // processes.


            foreach (var accessorInfo in FetchMappedAccessors())
            {
                var csName = GetAccessorPropertyName(accessorInfo);
                var ciName = csName.ToUpper();
                var prop = new SimpleMagicPropertyInfo(
                    csName,
                    accessorInfo,
                    accessorInfo,
                    null,
                    EventPropertyType.MAPPED);

                AddProperty(csName, ciName, prop);
            }
        }

        /// <summary>
        /// Indexes the indexed properties.
        /// </summary>
        private void IndexIndexedProperties()
        {
            foreach (PropertyInfo propertyInfo in FetchIndexedProperties())
            {
                var csName = GetPropertyName(propertyInfo.Name, propertyInfo);
                var ciName = csName.ToUpper();
                var prop = new SimpleMagicPropertyInfo(
                    csName,
                    propertyInfo,
                    propertyInfo.GetGetMethod(),
                    propertyInfo.GetSetMethod(),
                    EventPropertyType.INDEXED);

                AddProperty(csName, ciName, prop);
            }

            foreach( MethodInfo methodInfo in FetchIndexedAccessors()) {
                var csName = GetAccessorPropertyName(methodInfo);
                var ciName = csName.ToUpper();
                var prop = new SimpleMagicPropertyInfo(
                    csName,
                    methodInfo,
                    methodInfo,
                    null,
                    EventPropertyType.INDEXED);

                AddProperty(csName, ciName, prop);
            }
        }

        /// <summary>
        /// Gets all properties.
        /// </summary>
        /// <param name="isCaseSensitive">if set to <c>true</c> [is case sensitive].</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public IEnumerable<SimpleMagicPropertyInfo> GetAllProperties(bool isCaseSensitive, Predicate<MagicPropertyInfo> filter)
        {
            var table = isCaseSensitive ? _csPropertyTable : _ciPropertyTable;
            foreach (var entry in table)
            {
                for (var temp = entry.Value; temp != null; temp = temp.Next )
                {
                    if (filter.Invoke(temp))
                    {
                        yield return temp;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all properties.
        /// </summary>
        /// <param name="isCaseSensitive">if set to <c>true</c> [case sensitive].</param>
        /// <returns></returns>
        public IEnumerable<SimpleMagicPropertyInfo> GetAllProperties( bool isCaseSensitive )
        {
            return GetAllProperties(isCaseSensitive, magicProperty => true);
        }

        /// <summary>
        /// Gets all simple properties.
        /// </summary>
        /// <param name="isCaseSensitive">if set to <c>true</c> [case sensitive].</param>
        /// <returns></returns>
        public IEnumerable<SimpleMagicPropertyInfo> GetSimpleProperties(bool isCaseSensitive)
        {
            return GetAllProperties(isCaseSensitive, magicProperty => magicProperty.EventPropertyType == EventPropertyType.SIMPLE);
        }

        /// <summary>
        /// Gets all mapped properties.
        /// </summary>
        /// <param name="isCaseSensitive">if set to <c>true</c> [case sensitive].</param>
        /// <returns></returns>
        public IEnumerable<SimpleMagicPropertyInfo> GetMappedProperties(bool isCaseSensitive)
        {
            return GetAllProperties(isCaseSensitive, magicProperty => magicProperty.EventPropertyType == EventPropertyType.MAPPED);
        }

        /// <summary>
        /// Gets all indexed properties.
        /// </summary>
        /// <param name="isCaseSensitive">if set to <c>true</c> [case sensitive].</param>
        /// <returns></returns>
        public IEnumerable<SimpleMagicPropertyInfo> GetIndexedProperties(bool isCaseSensitive)
        {
            return GetAllProperties(isCaseSensitive, magicProperty => magicProperty.EventPropertyType == EventPropertyType.INDEXED);
        }

        /// <summary>
        /// Resolves the complex property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="resolutionStyle">The resolution style.</param>
        /// <returns></returns>
        public MagicPropertyInfo ResolveComplexProperty(string propertyName, PropertyResolutionStyle resolutionStyle)
        {
            int indexOfDot = propertyName.IndexOf('.');
            if (indexOfDot != -1) {
                var head = propertyName.Substring(0, indexOfDot);
                var tail = propertyName.Substring(indexOfDot + 1);
                var rootProperty = ResolveProperty(head, resolutionStyle);
                var rootPropertyType = GetCachedType(rootProperty.PropertyType);
                if (rootPropertyType == null) {
                    return null;
                }

                var tailProperty = rootPropertyType.ResolveProperty(tail, resolutionStyle);
                return new DynamicMagicPropertyInfo(rootProperty, tailProperty);
            }

            return ResolveProperty(propertyName, resolutionStyle);
        }

        /// <summary>
        /// Finds the property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="resolutionStyle">if set to <c>true</c> [is case sensitive].</param>
        /// <returns></returns>
        public SimpleMagicPropertyInfo ResolveProperty(string propertyName, PropertyResolutionStyle resolutionStyle )
        {
            switch (resolutionStyle) {
                case PropertyResolutionStyle.CASE_SENSITIVE:
                    do {
                        var property = _csPropertyTable.Get(propertyName);
                        if (property != null)
                            return property;
                        if (_parent != null)
                            return _parent.ResolveProperty(propertyName, resolutionStyle);
                        return null;
                    } while (false);

                case PropertyResolutionStyle.CASE_INSENSITIVE:
                    do {
                        var property = _ciPropertyTable.Get(propertyName.ToUpper());
                        if (property != null)
                            return property;
                        if (_parent != null)
                            return _parent.ResolveProperty(propertyName, resolutionStyle);
                        return null;
                    } while (false);

                case PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE:
                    do {
                        var property = _ciPropertyTable.Get(propertyName.ToUpper());
                        if (property != null) {
                            if (property.IsUnique) {
                                return property;
                            }

                            throw new EPException("Unable to determine which property to use for \"" + propertyName +
                                                  "\" because more than one property matched");
                        }

                        if (_parent != null)
                            return _parent.ResolveProperty(propertyName, resolutionStyle);
                        return null;
                    } while (false);
            }

            return null;
        }

        /// <summary>
        /// Search of the type to find a property.
        /// </summary>
        /// <param name="propertyName">The name.</param>
        /// <param name="resolutionStyle">The resolution style.</param>
        /// <returns></returns>
        public MethodInfo ResolvePropertyMethod( string propertyName, PropertyResolutionStyle resolutionStyle)
        {
            var property = ResolveProperty(propertyName, resolutionStyle) as SimpleMagicPropertyInfo;
            return property?.GetMethod;
        }

        /// <summary>
        /// Gets the name of the mutator property.
        /// </summary>
        /// <param name="mutatorMethod">The mutator method.</param>
        /// <returns></returns>
        public string GetMutatorPropertyName(MethodInfo mutatorMethod)
        {
            var attributes = mutatorMethod.GetCustomAttributes(typeof(PropertyNameAttribute), true);
            if (attributes != null)
            {
                foreach (PropertyNameAttribute attribute in attributes)
                {
                    return attribute.Name;
                }
            }

            // Start by removing the "Insert" from the front of the mutatorMethod name
            String inferredName = mutatorMethod.Name.Substring(3);
            String newInferredName = null;
            // Leave uppercase inferred names such as URL
            if (inferredName.Length >= 2)
            {
                if (Char.IsUpper(inferredName[0]) &&
                    Char.IsUpper(inferredName[1]))
                {
                    newInferredName = inferredName;
                }
            }
            // camelCase the inferred name
            if (newInferredName == null)
            {
                newInferredName = Char.ToString(Char.ToUpper(inferredName[0]));
                if (inferredName.Length > 1)
                {
                    newInferredName += inferredName.Substring(1);
                }
            }

            return newInferredName;
        }

        /// <summary>
        /// Gets the name that should be assigned to the property bound to the accessorMethod
        /// </summary>
        /// <param name="accessorMethod"></param>
        /// <returns></returns>

        public string GetAccessorPropertyName(MethodInfo accessorMethod)
        {
            var attributes = accessorMethod.GetCustomAttributes(typeof(PropertyNameAttribute), true);
            if (attributes != null)
            {
                foreach (PropertyNameAttribute attribute in attributes)
                {
                    return attribute.Name;
                }
            }

            // Start by removing the "get" from the front of the accessorMethod name
            String inferredName = accessorMethod.Name.Substring(3);
            String newInferredName = null;
            // Leave uppercase inferred names such as URL
            if (inferredName.Length >= 2)
            {
                if (Char.IsUpper(inferredName[0]) &&
                    Char.IsUpper(inferredName[1]))
                {
                    newInferredName = inferredName;
                }
            }
            // camelCase the inferred name
            if (newInferredName == null)
            {
                newInferredName = Char.ToString(Char.ToUpper(inferredName[0]));
                if (inferredName.Length > 1)
                {
                    newInferredName += inferredName.Substring(1);
                }
            }

            return newInferredName;
        }

        /// <summary>
        /// Returns all simple properties
        /// </summary>
        /// <returns></returns>

        public IEnumerable<PropertyInfo> FetchSimpleProperties()
        {
            foreach (PropertyInfo propInfo in _type.GetProperties())
            {
                var getMethod = propInfo.GetGetMethod();
                if ((getMethod != null) &&
                    (getMethod.IsStatic == false) &&
                    (getMethod.GetParameters().Length == 0))
                {
                    yield return propInfo;
                }
            }
        }

        /// <summary>
        /// Returns an enumerable that provides all accessors that take no
        /// parameters.
        /// </summary>
        /// <returns></returns>

        public IEnumerable<MethodInfo> FetchSimpleAccessors()
        {
            foreach (var methodInfo in GetAccessors())
            {
                var methodParams = methodInfo.GetParameters();
                if (methodParams.Length == 0)
                {
                    yield return methodInfo;
                }
            }
        }

        public IEnumerable<PropertyInfo> FetchIndexedProperties()
        {
            foreach (PropertyInfo propInfo in _type.GetProperties())
            {
                var getMethod = propInfo.GetGetMethod();
                if (getMethod == null)
                    continue;
                if (getMethod.IsStatic)
                    continue;

                var parameters = getMethod.GetParameters();
                if ((parameters.Length != 1) || (parameters[0].ParameterType != typeof(int)))
                    continue;

                yield return propInfo;
            }
        }

        /// <summary>
        /// Returns an enumerable that provides all accessors that take one
        /// parameter of type int.
        /// </summary>
        /// <returns></returns>

        public IEnumerable<MethodInfo> FetchIndexedAccessors()
        {
            foreach (MethodInfo methodInfo in GetAccessors())
            {
                ParameterInfo[] methodParams = methodInfo.GetParameters();
                if ((methodParams.Length == 1) &&
                    (methodParams[0].ParameterType == typeof(int)))
                {
                    yield return methodInfo;
                }
            }
        }

        public IEnumerable<PropertyInfo> FetchMappedProperties()
        {
            foreach (PropertyInfo propInfo in _type.GetProperties())
            {
                var getMethod = propInfo.GetGetMethod();
                if (getMethod == null)
                    continue;
                if (getMethod.IsStatic)
                    continue;

                var returnType = propInfo.PropertyType;
                if (returnType.IsGenericStringDictionary())
                    yield return propInfo;
            }
        }

        /// <summary>
        /// Returns an enumerable that provides all accessors that take one
        /// parameter of type string.
        /// </summary>
        /// <returns></returns>

        public IEnumerable<MethodInfo> FetchMappedAccessors()
        {
            foreach (var methodInfo in GetAccessors())
            {
                var methodParams = methodInfo.GetParameters();
                if ((methodParams.Length == 1) &&
                    (methodParams[0].ParameterType == typeof(string)))
                {
                    yield return methodInfo;
                }
            }
        }

        /// <summary>
        /// Enumerates all accessor methods for a type
        /// </summary>
        /// <returns></returns>

        public IEnumerable<MethodInfo> GetAccessors()
        {
            foreach (var methodInfo in _type.GetMethods())
            {
                var methodName = methodInfo.Name;

                if ((methodInfo.IsSpecialName == false) &&
                    (methodName.StartsWith("Get")) &&
                    (methodName != "Get"))
                {
                    // We don't need any of the pseudo accessors from System.Object
                    if ((methodName == "GetHashCode") ||
                        (methodName == "GetType")) {
                        continue;
                    }

                    yield return methodInfo;
                }
            }
        }

        public IEnumerable<MethodInfo> GetMutators()
        {
            foreach (var methodInfo in _type.GetMethods())
            {
                var methodName = methodInfo.Name;

                if ((methodInfo.IsSpecialName == false) &&
                    (methodInfo.GetParameters().Length == 1) &&
                    (methodName.StartsWith("Set")) &&
                    (methodName != "Set"))
                {
                    yield return methodInfo;
                }
            }
        }

        public IEnumerable<MethodInfo> GetMappableMutators()
        {
            foreach (var methodInfo in _type.GetMethods())
            {
                var methodName = methodInfo.Name;

                if ((methodInfo.IsSpecialName == false) &&
                    (methodInfo.GetParameters().Length == 2) &&
                    (methodInfo.GetParameters()[0].ParameterType == typeof(string)) &&
                    (methodName.StartsWith("Set")) &&
                    (methodName != "Set"))
                {
                    yield return methodInfo;
                }
            }
        }

        private MethodInfo GetSimpleMutator(string propertyName, Type propertyType)
        {
            var methodName = "Set" + propertyName;
            try
            {
                var methodInfo = _type.GetMethod(methodName);
                if ((methodInfo != null) && methodInfo.IsPublic && !methodInfo.IsStatic)
                {
                    var parameters = methodInfo.GetParameters();
                    if ((parameters.Length == 1) && (parameters[0].ParameterType == propertyType))
                    {
                        return methodInfo;
                    }
                }
            } 
            catch(AmbiguousMatchException)
            {
                var methods = _type
                    .GetMethods()
                    .Where(method => method.Name == methodName && method.IsPublic && !method.IsStatic);
                foreach(var methodInfo in methods)
                {
                    var parameters = methodInfo.GetParameters();
                    if ((parameters.Length == 1) && (parameters[0].ParameterType == propertyType))
                    {
                        return methodInfo;
                    }
                }

            }

            return null;
        }

        private static readonly ILockable TypeCacheLock = new MonitorSpinLock(60000);
        private static readonly Dictionary<Type, MagicType> TypeCacheTable =
            new Dictionary<Type, MagicType>();

        public static MagicType GetCachedType(Type t)
        {
            using (TypeCacheLock.Acquire()) {
                MagicType magicType;
                if (!TypeCacheTable.TryGetValue(t, out magicType)) {
                    TypeCacheTable[t] = magicType = new MagicType(t);
                }

                return magicType;
            }
        }

        private static readonly ILockable AccessorCacheLock = new MonitorSpinLock(60000);
        private static readonly Dictionary<MethodInfo, Func<object, object>> AccessorCacheTable =
            new Dictionary<MethodInfo, Func<object, object>>();

        public static Func<object, object> GetLambdaAccessor(MethodInfo methodInfo)
        {
            using (AccessorCacheLock.Acquire()) {
                Func<object, object> lambdaAccessor;
                if (!AccessorCacheTable.TryGetValue(methodInfo, out lambdaAccessor)) {
                    var eParam = Expression.Parameter(typeof (object), "arg");
                    var eCast1 = methodInfo.DeclaringType.IsValueType
                        ? Expression.Unbox(eParam, methodInfo.DeclaringType)
                        : Expression.Convert(eParam, methodInfo.DeclaringType);
                    var eMethod = Expression.Call(methodInfo.IsStatic ? null : eCast1, methodInfo);
                    var eCast2 = methodInfo.ReturnType.IsValueType
                                     ? (Expression) Expression.Convert(eMethod, typeof(object))
                                     : eMethod;
                    var eLambda = Expression.Lambda<Func<object, object>>(eCast2, eParam);
                    AccessorCacheTable[methodInfo] = lambdaAccessor = eLambda.Compile();
                }

                return lambdaAccessor;
            }
        }
    }

    #region MagicPropertyInfo
    public abstract class MagicPropertyInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        public Type PropertyType { get; protected set; }

        /// <summary>
        /// Gets or sets the event type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        public EventPropertyType EventPropertyType { get; protected set; }

        /// <summary>
        /// Returns a function that can be used to obtain the value of the
        /// property from an object instance.
        /// </summary>
        /// <value>The get function.</value>
        abstract public Func<object, object> GetFunction { get; }

        /// <summary>
        /// Returns a function that can be used to set the value of the
        /// property in an object instance.
        /// </summary>
        /// <value>The set function.</value>
        abstract public Action<object, object> SetFunction { get; }

        /// <summary>
        /// Gets a value indicating whether this property can be set.
        /// </summary>
        /// <value><c>true</c> if this instance can write; otherwise, <c>false</c>.</value>
        abstract public bool CanWrite { get; }

        /// <summary>
        /// Static cast method used in assignment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static T CastTo<T>(Object value)
        {
            if (value is T)
                return (T)value;

            // Arrays need to be converted by looking at the internal elements
            // within the array.  Since value is more than likely going to be
            // an array of System.Object, the conversion is basically a recursive
            // call to this method.
            if (typeof(T).IsArray)
            {
                var valueArray = value as Object[];
                if (valueArray == null)
                {
                    return default(T); // null
                }

                var subType = typeof(T).GetElementType();
                var subCast = typeof(MagicPropertyInfo)
                    .GetMethod("CastTo")
                    .MakeGenericMethod(subType);

                var returnArray = Array.CreateInstance(subType, valueArray.Length);
                for (int ii = 0; ii < valueArray.Length; ii++)
                {
                    returnArray.SetValue(subCast.Invoke(null, new[] { valueArray[ii] }), ii);
                }

                return (T)((Object)returnArray);
            }

            var genericTypeCaster = CastHelper.GetCastConverter<T>();
            return genericTypeCaster(value);
        }
    }

    public class SimpleMagicPropertyInfo : MagicPropertyInfo
    {
        /// <summary>
        /// Gets or sets the member.
        /// </summary>
        /// <value>The member.</value>
        public MemberInfo Member { get; protected set; }

        /// <summary>
        /// Gets or sets the get method.
        /// </summary>
        /// <value>The get method.</value>
        public MethodInfo GetMethod { get; protected set; }

        /// <summary>
        /// Gets or sets the set method.
        /// </summary>
        /// <value>The set method.</value>
        public MethodInfo SetMethod { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this property can be set.
        /// </summary>
        /// <value><c>true</c> if this instance can write; otherwise, <c>false</c>.</value>
        public override bool CanWrite
        {
            get { return SetMethod != null; }
        }

        /// <summary>
        /// Gets or sets the next magic property INFO that shares the same
        /// name.
        /// </summary>
        /// <value>The next.</value>
        public SimpleMagicPropertyInfo Next { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this property is unique.  If a property is
        /// not unique then it shares the same name as another property but a different
        /// implementation.
        /// </summary>
        /// <value><c>true</c> if this instance is unique; otherwise, <c>false</c>.</value>
        public bool IsUnique
        {
            get { return Next == null; }
        }

        private Func<object, object> _getFunction;

        /// <summary>
        /// Returns a function that can be used to obtain the value of the
        /// property from an object instance.
        /// </summary>
        /// <value>The get function.</value>
        public override Func<object, object> GetFunction
        {
            get
            {
                if (_getFunction == null) {
                    var eParam1 = Expression.Parameter(typeof(object), "obj");
                    var eCast1 = Expression.ConvertChecked(eParam1, Member.DeclaringType);
                    var eMethod = Expression.Call(eCast1, GetMethod);
                    var eLambda = Expression.Lambda<Func<object, object>>(eMethod, eParam1);
                    _getFunction = eLambda.Compile();
                }

                return _getFunction;
            }
        }

        private Action<object, object> _setFunction;

        /// <summary>
        /// Returns a function that can be used to set the value of the
        /// property in an object instance.
        /// </summary>
        /// <value>The set function.</value>
        public override Action<object, object> SetFunction
        {
            get
            {
                if ((SetMethod != null) && (_setFunction == null)) {
                    var castTo = typeof(MagicPropertyInfo)
                        .GetMethod("CastTo")
                        .MakeGenericMethod(GetMethod.ReturnType);

                    var eParam1 = Expression.Parameter(typeof (object), "obj");
                    var eParam2 = Expression.Parameter(typeof(object), "value");
                    var eCast1 = Expression.ConvertChecked(eParam1, Member.DeclaringType);
                    var eCast2 = Expression.Call(castTo, eParam2);
                    var eMethod = Expression.Call(eCast1, SetMethod, eCast2);
                    var eLambda = Expression.Lambda<Action<object, object>>(eMethod, eParam1, eParam2);
                    _setFunction = eLambda.Compile();
                }

                return _setFunction;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMagicPropertyInfo"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="member">The member.</param>
        /// <param name="getMethod">The get method.</param>
        /// <param name="setMethod">The set method.</param>
        /// <param name="propertyType">Type of the property.</param>
        public SimpleMagicPropertyInfo(string name, MemberInfo member, MethodInfo getMethod, MethodInfo setMethod, EventPropertyType propertyType)
        {
            Name = name;
            Member = member;
            
            if ( member is PropertyInfo )
                PropertyType = ((PropertyInfo) member).PropertyType;
            else if ( member is MethodInfo )
                PropertyType = ((MethodInfo) member).ReturnType;

            GetMethod = getMethod;
            SetMethod = setMethod;
            EventPropertyType = propertyType;
        }
    }

    public class DynamicMagicPropertyInfo : MagicPropertyInfo
    {
        public MagicPropertyInfo Parent { get; private set; }
        public MagicPropertyInfo Child { get; private set; }

        /// <summary>
        /// Gets the value from an instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        private Object GetValue(Object instance)
        {
            var parentInstance = Parent.GetFunction.Invoke(instance);
            var childInstance = Child.GetFunction.Invoke(parentInstance);
            return childInstance;
        }

        /// <summary>
        /// Sets the value within an instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="value">The value.</param>
        private void SetValue(Object instance, Object value)
        {
            var parentInstance = Parent.GetFunction.Invoke(instance);
            Child.SetFunction.Invoke(parentInstance, value);
        }

        /// <summary>
        /// Returns a function that can be used to obtain the value of the
        /// property from an object instance.
        /// </summary>
        /// <value>The get function.</value>
        public override Func<object, object> GetFunction
        {
            get { return GetValue; }
        }

        /// <summary>
        /// Returns a function that can be used to set the value of the
        /// property in an object instance.
        /// </summary>
        /// <value>The set function.</value>
        public override Action<object, object> SetFunction
        {
            get { return SetValue; }
        }

        /// <summary>
        /// Gets a value indicating whether this property can be set.
        /// </summary>
        /// <value><c>true</c> if this instance can write; otherwise, <c>false</c>.</value>
        public override bool CanWrite
        {
            get { return Child.CanWrite; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicMagicPropertyInfo"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="child">The child.</param>
        public DynamicMagicPropertyInfo(MagicPropertyInfo parent, MagicPropertyInfo child)
        {
            Parent = parent;
            Child = child;
        }
    }

    #endregion
}
