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

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.compat.util;

namespace com.espertech.esper.compat.magic
{
    public class MagicMarker
    {
        public static MagicMarker SingletonInstance { get; } = new MagicMarker(
            new DefaultGenericTypeCasterFactory());

        private GenericTypeCasterFactory _typeCasterFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MagicMarker"/> class.
        /// </summary>
        /// <param name="typeCasterFactory">The type caster factory.</param>
        public MagicMarker(GenericTypeCasterFactory typeCasterFactory) {
            _typeCasterFactory = typeCasterFactory;
        }

        /// <summary>
        /// Gets or sets the type caster factory.
        /// </summary>
        public GenericTypeCasterFactory TypeCasterFactory {
            get => _typeCasterFactory;
            set => _typeCasterFactory = value;
        }

        #region Collection Factory
        private readonly ILockable _collectionFactoryTableLock = new MonitorSpinLock(60000);
        private readonly IDictionary<Type, Func<object, ICollection<object>>> _collectionFactoryTable =
            new Dictionary<Type, Func<object, ICollection<object>>>();

        /// <summary>
        /// Creates a factory object that produces wrappers (MagicCollection) instances for a
        /// given object.  This method is designed for those who know early on that they are
        /// going to be dealing with an opaque object that will have a true generic definition
        /// once they receive it.  However, to make life easier (relatively speaking), we would
        /// prefer to operate with a clean object rather than the type specific detail.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public Func<object, ICollection<object>> GetCollectionFactory(Type t)
        {
            using(_collectionFactoryTableLock.Acquire()) {
                if (! _collectionFactoryTable.TryGetValue(t, out var collectionFactory)) {
                    var intermediate = NewCollectionFactory(t);
                    collectionFactory = o => intermediate.Invoke(o, _typeCasterFactory);
                    _collectionFactoryTable[t] = collectionFactory;
                }

                return collectionFactory;
            }
        }

        public ICollection<object> GetCollection(Object o)
        {
            if (o == null)
                return null;

            return GetCollectionFactory(o.GetType())?.Invoke(o);
        }

        public static ICollection<object> NewMagicCollection<TV>(object o, GenericTypeCasterFactory typeCasterFactory)
        {
            return o == null ? null : new MagicCollection<TV>(o, typeCasterFactory.Get<TV>());
        }

        /// <summary>
        /// Constructs the factory method for the given type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        
        private Func<object, GenericTypeCasterFactory, ICollection<object>> NewCollectionFactory(Type t)
        {
            if (t == typeof(ICollection<object>))
                return (o, factory) => (ICollection<object>)o;

            var genericType = t.FindGenericInterface(typeof (ICollection<>));
            if (genericType == null)
                return null;

            var genericArg = genericType.GetGenericArguments()[0];
            var magicActivator = typeof (MagicMarker)
                .GetMethod("NewMagicCollection", new[] { typeof (object), typeof(GenericTypeCasterFactory) })?
                .MakeGenericMethod(genericArg);

            // GetInstance create a function that will create the wrapper type when
            // the generic object is presented.
            var eParam1 = Expression.Parameter(typeof (object), "o");
            var eParam2 = Expression.Parameter(typeof(GenericTypeCasterFactory), "gv");
            var eBuild = Expression.Call(magicActivator, eParam1, eParam2);
            var eLambda = Expression.Lambda<Func<object, GenericTypeCasterFactory, ICollection<object>>>(eBuild, eParam1, eParam2);

            // Return the compiled lambda method
            return eLambda.Compile();
        }
        #endregion

        #region List Factory
        private readonly ILockable _listFactoryTableLock = new MonitorSpinLock(60000);
        private readonly IDictionary<Type, Func<object, IList<object>>> _listFactoryTable =
            new Dictionary<Type, Func<object, IList<object>>>();

        public Func<object, IList<object>> GetListFactory(Type t)
        {
            using (_listFactoryTableLock.Acquire())
            {
                if (!_listFactoryTable.TryGetValue(t, out var listFactory))
                {
                    var intermediate = NewListFactory(t);
                    listFactory = o => intermediate.Invoke(o, _typeCasterFactory);
                    _listFactoryTable[t] = listFactory;
                }

                return listFactory;
            }
        }

        public static IList<object> NewMagicList<TV>(object o, GenericTypeCasterFactory typeCasterFactory)
        {
            return o == null ? null : new MagicList<TV>(o, typeCasterFactory.Get<TV>());
        }
        
        public IList<object> GetList(Object o)
        {
            if (o == null)
                return null;

            return GetListFactory(o.GetType())?.Invoke(o);
        }

        /// <summary>
        /// Constructs the factory method for the given type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>

        private Func<object, GenericTypeCasterFactory, IList<object>> NewListFactory(Type t)
        {
            if (t == typeof(IList<object>))
                return (o, factory) => (IList<object>)o;

            var genericType = t.FindGenericInterface(typeof(IList<>));
            if (genericType == null)
                return null;

            var genericArg = genericType.GetGenericArguments()[0];
            var magicActivator = typeof(MagicMarker)
                .GetMethod("NewMagicList", new[] { typeof(object), typeof(GenericTypeCasterFactory) })?
                .MakeGenericMethod(genericArg);

            // GetInstance create a function that will create the wrapper type when
            // the generic object is presented.
            var eParam1 = Expression.Parameter(typeof(object), "o");
            var eParam2 = Expression.Parameter(typeof(GenericTypeCasterFactory), "gv");
            var eBuild = Expression.Call(magicActivator, eParam1, eParam2);
            var eLambda = Expression.Lambda<Func<object, GenericTypeCasterFactory, IList<object>>>(eBuild, eParam1, eParam2);

            // Return the compiled lambda method
            return eLambda.Compile();
        }
        #endregion

        #region Dictionary Factory
        private readonly ILockable _dictionaryFactoryTableLock = new MonitorSpinLock(60000);
        private readonly IDictionary<Type, Func<object, IDictionary<object, object>>> _dictionaryFactoryTable =
            new Dictionary<Type, Func<object, IDictionary<object, object>>>();

        /// <summary>
        /// Creates a factory object that produces wrappers (MagicStringDictionary) instances for
        /// a given object.  This method is designed for those who know early on that they are
        /// going to be dealing with an opaque object that will have a true generic definition
        /// once they receive it.  However, to make life easier (relatively speaking), we would
        /// prefer to operate with a clean object rather than the type specific detail.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public Func<object, IDictionary<object, object>> GetDictionaryFactory(Type t)
        {
            using(_dictionaryFactoryTableLock.Acquire())
            {
                if (!_dictionaryFactoryTable.TryGetValue(t, out var dictionaryFactory))
                {
                    var intermediate = NewDictionaryFactory(t);
                    dictionaryFactory = o => intermediate.Invoke(o, _typeCasterFactory);
                    _dictionaryFactoryTable[t] = dictionaryFactory;
                }

                return dictionaryFactory;
            }
        }

        public static IDictionary<object, object> NewMagicDictionary<TK, TV>(object o, GenericTypeCasterFactory typeCasterFactory)
        {
            return o == null ? null : new MagicDictionary<TK, TV>(o, typeCasterFactory.Get<TK>());
        }

        /// <summary>
        /// Constructs the factory method for the given type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>

        private Func<object, GenericTypeCasterFactory, IDictionary<object, object>> NewDictionaryFactory(Type t)
        {
            if (t == typeof(IDictionary<object, object>))
                return (o,factory) => (IDictionary<object, object>)o;

            var genericType = t.FindGenericInterface(typeof(IDictionary<,>));
            if (genericType == null)
                return null;

            var genericKey = genericType.GetGenericArguments()[0];
            var genericArg = genericType.GetGenericArguments()[1];
            var magicActivator = typeof (MagicMarker)
                .GetMethod("NewMagicDictionary", new[] { typeof (object), typeof(GenericTypeCasterFactory) })?
                .MakeGenericMethod(genericKey, genericArg);

            // GetInstance create a function that will create the wrapper type when
            // the generic object is presented.
            var eParam1 = Expression.Parameter(typeof(object), "o");
            var eParam2 = Expression.Parameter(typeof(GenericTypeCasterFactory), "gv");
            var eBuild = Expression.Call(magicActivator, eParam1, eParam2);
            var eLambda = Expression.Lambda<Func<object, GenericTypeCasterFactory, IDictionary<object, object>>>(eBuild, eParam1, eParam2);

            // Return the compiled lambda method
            return eLambda.Compile();
        }
        #endregion

        #region String Dictionary Factory
        private readonly ILockable _stringDictionaryFactoryTableLock = new MonitorSpinLock(60000);
        private readonly IDictionary<Type, Func<object, IDictionary<string, object>>> _stringDictionaryFactoryTable =
            new Dictionary<Type, Func<object, IDictionary<string, object>>>();

        public IDictionary<string,object> GetStringDictionary(Object o)
        {
            if ( o == null )
                return null;

            return GetStringDictionaryFactory(o.GetType())?.Invoke(o);
        }

        /// <summary>
        /// Creates a factory object that produces wrappers (MagicStringDictionary) instances for
        /// a given object.  This method is designed for those who know early on that they are
        /// going to be dealing with an opaque object that will have a true generic definition
        /// once they receive it.  However, to make life easier (relatively speaking), we would
        /// prefer to operate with a clean object rather than the type specific detail.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public Func<object, IDictionary<string, object>> GetStringDictionaryFactory(Type t)
        {
            using (_stringDictionaryFactoryTableLock.Acquire())
            {
                if (!_stringDictionaryFactoryTable.TryGetValue(t, out var stringDictionaryFactory))
                {
                    stringDictionaryFactory = NewStringDictionaryFactory(t);
                    _stringDictionaryFactoryTable[t] = stringDictionaryFactory;
                }

                return stringDictionaryFactory;
            }
        }

        public static IDictionary<string, object> NewMagicStringDictionary<TV>(object o)
        {
            return o == null ? null : new MagicStringDictionary<TV>(o);
        }

        /// <summary>
        /// Constructs the factory method for the given type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>

        private Func<object, IDictionary<string, object>> NewStringDictionaryFactory(Type t)
        {
            if (t == typeof(IDictionary<string, object>))
                return o => (IDictionary<string, object>)o;

            var genericType = t.FindGenericInterface(typeof(IDictionary<,>));
            if (genericType == null)
                return null;

            var genericKey = genericType.GetGenericArguments()[0];
            if (genericKey != typeof(string))
                return null;

            var genericArg = genericType.GetGenericArguments()[1];
            var magicActivator = typeof(MagicMarker)
                .GetMethod("NewMagicStringDictionary", new[] { typeof(object) })?
                .MakeGenericMethod(genericArg);

            // GetInstance create a function that will create the wrapper type when
            // the generic object is presented.
            var eParam = Expression.Parameter(typeof(object), "o");
            var eBuild = Expression.Call(magicActivator, eParam);
            var eLambda = Expression.Lambda<Func<object, IDictionary<string, object>>>(eBuild, eParam);

            // Return the compiled lambda method
            return eLambda.Compile();
        }
        #endregion
    }

    public class DefaultGenericTypeCasterFactory : GenericTypeCasterFactory
    {
        public GenericTypeCaster<T> Get<T>()
        {
            return CastHelper.GetCastConverter<T>();
        }
    }
}
