///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esperio.csv
{
    /// <summary>Coercer for type conversion. </summary>
    public abstract class AbstractTypeCoercer
    {
        /// <summary>For logging. </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>Constructors for coercion. </summary>
        protected IDictionary<string, Func<string, object>> propertyFactories;

        /// <summary>Ctor. </summary>
        /// <param name="propertyTypes">the type conversion to be done</param>
        public void SetPropertyTypes(IDictionary<string, object> propertyTypes)
        {
            propertyFactories = CreatePropertyFactories(propertyTypes);
        }

        /// <summary>Convert a value. </summary>
        /// <param name="property">property name</param>
        /// <param name="source">value to convert</param>
        /// <returns>object value</returns>
        /// <throws>Exception if coercion failed</throws>
        public abstract object Coerce(string property, string source);

        private static readonly IDictionary<Type, Func<string, object>> _typeFactoryTable =
            new Dictionary<Type, Func<string, object>>();

        private static Func<string, object> CreateTypeFactory(Type t)
        {
            if (t == typeof(string))
                return s => s;
            if (t == typeof(int?))
                return s => s == null ? (int?) null : int.Parse(s);
            if (t == typeof(long?))
                return s => s == null ? (long?)null : long.Parse(s);
            if (t == typeof(short?))
                return s => s == null ? (short?)null : short.Parse(s);
            if (t == typeof(float?))
                return s => s == null ? (float?)null : float.Parse(s);
            if (t == typeof(double?))
                return s => s == null ? (double?)null : double.Parse(s);
            if (t == typeof(decimal?))
                return s => s == null ? (decimal?)null : decimal.Parse(s);
            if (t == typeof(DateTime?))
                return s => s == null ? (DateTime?)null : DateTime.Parse(s);
            if (t == typeof(Guid?))
                return s => s == null ? (Guid?)null : new Guid(s);

            var constructor = t.GetConstructor(new[] {typeof (string)});
            if (constructor != null) {
                var eParam = Expression.Parameter(typeof(string), "arg");
                var eConstructor = Expression.New(constructor, eParam);
                var eLambda = Expression.Lambda<Func<string, object>>(eConstructor, eParam);
                return eLambda.Compile();
            }

            var parser = t.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new []{ typeof(string)}, null);
            if (parser != null) {
                var eParam = Expression.Parameter(typeof(string), "arg");
                var eMethod = Expression.Call(parser, eParam);
                var eLambda = Expression.Lambda<Func<string, object>>(eMethod, eParam);
                return eLambda.Compile();
            }

            return null;
        }

        private static IDictionary<string, Func<string, object>> CreatePropertyFactories(IDictionary<string, object> propertyTypes)
        {
            var factories = new NullableDictionary<string, Func<string, object>>();

            foreach (var property in propertyTypes.Keys)
            {
                Log.Debug(".CreatePropertyFactories property==" + property + ", type==" + propertyTypes.Get(property));

                var propertyType = ((Type) propertyTypes.Get(property)).GetBoxedType();

                lock (_typeFactoryTable)
                {
                    var factory = _typeFactoryTable.Get(propertyType);
                    if (factory == null) {
                        _typeFactoryTable[propertyType] = factory = CreateTypeFactory(propertyType);
                    }
                 
                    factories[property] = factory;
                }
            }

            return factories;
        }
    }
}
