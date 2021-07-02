///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esperio.csv
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// An event Adapter that uses a CSV file for a source.
    /// </summary>

    public class CSVInputAdapter
        : AbstractCoordinatedAdapter
        , InputAdapter
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int? _eventsPerSec;
        private CSVReader _reader;
        private string[] _propertyOrder;
        private readonly CSVInputAdapterSpec _adapterSpec;
        private DataMap _propertyTypes;
        private readonly string _eventTypeName;
        private long? _lastTimestamp = 0;
        private long _totalDelay;
        private bool _atEof = false;
        private string[] _firstRow;
        private Type _beanType;
        private int _rowCount = 0;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="runtime">provides the engine runtime and services</param>
        /// <param name="spec">the parameters for this adapter</param>

        public CSVInputAdapter(EPRuntime runtime, CSVInputAdapterSpec spec)
            : base(runtime, spec.IsUsingEngineThread, spec.IsUsingExternalTimer, spec.IsUsingTimeSpanEvents)
        {
            Coercer = new BasicTypeCoercer();
            _adapterSpec = spec;
            _eventTypeName = _adapterSpec.EventTypeName;
            _eventsPerSec = spec.EventsPerSec;

            if (runtime != null)
            {
                FinishInitialization(runtime, spec);
            }
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="runtime">provides the engine runtime and services</param>
        /// <param name="adapterInputSource">the source of the CSV file</param>
        /// <param name="eventTypeName">the name of the Map event to create from the CSV data</param>
        public CSVInputAdapter(EPRuntime runtime, AdapterInputSource adapterInputSource, string eventTypeName)
            : this(runtime, new CSVInputAdapterSpec(adapterInputSource, eventTypeName))
        {
            
        }

        /// <summary>
        /// Ctor for adapters that will be passed to an AdapterCoordinator.
        /// </summary>
        /// <param name="adapterSpec">contains parameters that specify the behavior of the input adapter</param>

        public CSVInputAdapter(CSVInputAdapterSpec adapterSpec)
            : this(null, adapterSpec)
        {
        }

        /// <summary>
        /// Ctor for adapters that will be passed to an AdapterCoordinator.
        /// </summary>
        /// <param name="adapterInputSource">the parameters for this adapter</param>
        /// <param name="eventTypeName">the event type name that the input adapter generates events for</param>

        public CSVInputAdapter(AdapterInputSource adapterInputSource, string eventTypeName)
            : this(null, adapterInputSource, eventTypeName)
        {
        }

        /// <summary>
        /// Gets or sets the coercing provider.
        /// </summary>
        /// <value>The coercer.</value>
        public AbstractTypeCoercer Coercer { get; set; }

        public override SendableEvent Read()
        {
            if (StateManager.State == AdapterState.DESTROYED || _atEof)
            {
                return null;
            }

            try
            {
                if (EventsToSend.IsEmpty())
                {
                    if (_beanType != null)
                    {
                        return new SendableBeanEvent(NewMapEvent(), _beanType, _eventTypeName, _totalDelay, ScheduleSlot);
                    }

                    return new SendableMapEvent(NewMapEvent(), _eventTypeName, _totalDelay, ScheduleSlot);
                }

                var theEvent = EventsToSend.First();
                EventsToSend.Remove(theEvent);
                return theEvent;
            }
            catch (EndOfStreamException)
            {
                if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled))
                {
                    Log.Debug(".read reached end of CSV file");
                } 
                _atEof = true;
                if (StateManager.State == AdapterState.STARTED)
                {
                    Stop();
                }
                else
                {
                    Destroy();
                }
                return null;
            }
        }

        public override EPRuntime Runtime
        {
            set
            {
                base.Runtime = value;
                FinishInitialization(value, _adapterSpec);
            }
        }

        /// <summary>
        /// Close the CSVReader.
        /// </summary>

        protected override void Close()
        {
            _reader.Close();
        }

        /// <summary>
        /// Remove the first member of eventsToSend. If there is another record in the CSV file, 
        /// insert the event created from it into eventsToSend.
        /// </summary>

        protected override void ReplaceFirstEventToSend()
        {
            EventsToSend.Remove(EventsToSend.First());
            var _event = Read();
            if (_event != null)
            {
                EventsToSend.Add(_event);
            }
        }

        /// <summary>
        /// Reset all the changeable state of this ReadableAdapter, as if it were just created.
        /// </summary>

        protected override void Reset()
        {
            _lastTimestamp = 0;
            _totalDelay = 0;
            _atEof = false;
            if (_reader.IsResettable)
            {
                _reader.Reset();
            }
        }

        private void FinishInitialization(EPRuntime runtime, CSVInputAdapterSpec spec)
        {
            AssertValidParameters(runtime, spec);

            var spi = (EPRuntimeSPI)runtime;

            ScheduleSlot = new ScheduleBucket(-1).AllocateSlot();

            _reader = new CSVReader(spec.AdapterInputSource);
            _reader.Looping = spec.IsLooping;

            var firstRow = FirstRow;

            var givenPropertyTypes = ConstructPropertyTypes(
                spec.EventTypeName,
                spec.PropertyTypes,
                spi.ServicesContext.EventTypeRepositoryBus);

            _propertyOrder =
                spec.PropertyOrder ??
                CSVPropertyOrderHelper.ResolvePropertyOrder(firstRow, givenPropertyTypes);

            _reader.IsUsingTitleRow = IsUsingTitleRow(firstRow, _propertyOrder);
            if (!IsUsingTitleRow(firstRow, _propertyOrder))
            {
                _firstRow = firstRow;
            }

            _propertyTypes = ResolvePropertyTypes(givenPropertyTypes);
            if (givenPropertyTypes == null)
            {
                throw new EPException("CSV adapter requires a predefined event type name, the event type named '" + spec.EventTypeName + "' could not be found");
            }

            Coercer.SetPropertyTypes(_propertyTypes);
        }

        private DataMap NewMapEvent()
        {
            ++_rowCount;
            var row = _firstRow ?? _reader.GetNextRecord();
            _firstRow = null;
            var map = CreateMapFromRow(row);
            UpdateTotalDelay(map, _reader.GetAndClearIsReset());
            return map;
        }

        /// <summary>
        /// Simplified object construction through type parameters.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static object ProxyParser(string input)
        {
            return input;
        }

        /// <summary>
        /// Simplified object construction through type parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static object EasyParser<T>( string input )
        {
            var result = Convert.ChangeType(input, typeof (T));
            return result;
        }

        /// <summary>
        /// Retrieves the object factory for a given type. 
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static ObjectFactory<string> ObjectFactoryFor(Type type)
        {
            ObjectFactory<string> factoryObj;

            lock (((ICollection)StaticTypeTable).SyncRoot)
            {
                if (!StaticTypeTable.TryGetValue(type, out factoryObj))
                {
                    var constructor = type.GetConstructor(ParameterTypes);
                    if ( constructor == null )
                    {
                        throw new EPException("unable to find a usable constructor for " + type.FullName);
                    }

                    factoryObj = (input => constructor.Invoke(new object[] {input}));

                    StaticTypeTable[type] = factoryObj;
                }
            }

            return factoryObj;
        }

        private static IDictionary<string, ObjectFactory<string>> CreatePropertyConstructors(IDictionary<string, Type> propertyTypes)
        {
            var factories = new NullableDictionary<string, ObjectFactory<string>>();
            
            foreach( var entry in propertyTypes )
            {
            	var property = entry.Key;
            	var propertyType = entry.Value;

                Log.Debug(".CreatePropertyConstructors property==" + property + ", type==" + propertyType);

                factories.Put(property, ObjectFactoryFor(propertyType));
            }

            return factories;
        }

        private IDictionary<string, object> CreateMapFromRow(string[] row)
        {
            var map = new Dictionary<string, object>();

            var count = 0;

            try
            {
                foreach (var property in _propertyOrder)
                {
                    // Skip properties that are in the title row but not
                    // part of the map to send
                    if ((_propertyTypes != null) &&
                        (!_propertyTypes.ContainsKey(property)) &&
                        (!property.Equals(_adapterSpec.TimestampColumn)))
                    {
                        count++;
                        continue;
                    }

                    var value = Coercer.Coerce(property, row[count++]);
                    map.Put(property, value);
                }
            }
            catch (Exception e)
            {
                throw new EPException(e);
            }

            return map;
        }

        private DataMap ConstructPropertyTypes(string eventTypeName,
                                               DataMap propertyTypesGiven,
                                               EventTypeRepository eventAdapterService)
        {
            var propertyTypes = new Dictionary<string, object>();
            var eventType = eventAdapterService.NameToTypeMap.Get(eventTypeName);
            if (eventType == null)
            {
                if (propertyTypesGiven != null)
                {
                    throw new EPException("Failed to find event type named '" + eventTypeName + "'");
                }
                return propertyTypesGiven;
            }
            if (eventType.UnderlyingType != typeof(DataMap))
            {
                _beanType = eventType.UnderlyingType;
            }
            if (propertyTypesGiven != null && eventType.PropertyNames.Length != propertyTypesGiven.Count)
            {
                // allow this scenario for beans as we may want to bring in a subset of properties
                if (_beanType != null)
                {
                    return propertyTypesGiven;
                }

                throw new EPException("Event type " + eventTypeName + " has already been declared with a different number of parameters");
            }

            foreach (var property in eventType.PropertyNames) {
                Type type;
                try {
                    type = eventType.GetPropertyType(property);
                }
                catch (PropertyAccessException e) {
                    // thrown if trying to access an invalid property on an EventBean
                    throw new EPException(e);
                }

                if (propertyTypesGiven != null && propertyTypesGiven.Get(property) == null) {
                    throw new EPException("Event type " + eventTypeName + " " +
                                          "has already been declared with different parameters");
                }
                if (propertyTypesGiven != null && !Equals(propertyTypesGiven.Get(property), type)) {
                    throw new EPException("Event type " + eventTypeName + " " +
                                          "has already been declared with a different type for property " + property);
                }
                // we can't set read-only properties for bean
                if (eventType.UnderlyingType != typeof (DataMap)) {
                    var magicType = MagicType.GetCachedType(_beanType);
                    var magicProperty = magicType.ResolveProperty(property, PropertyResolutionStyle.CASE_SENSITIVE);
                    if (magicProperty == null) continue;
                    if (!magicProperty.CanWrite)
                        if (propertyTypesGiven == null) {
                            continue;
                        }
                        else {
                            throw new EPException("Event type " + eventTypeName + "property " + property +
                                                  " is read only");
                        }
                }
            
                propertyTypes[property] = type;
            }

            // flatten nested types
            var flattenPropertyTypes = new Dictionary<string, object>();
            foreach (var prop in propertyTypes)
            {
                var name = prop.Key;
                var type = prop.Value;
                var asType = type as Type;

                if ((asType != null) && 
                    (asType.IsGenericStringDictionary()) && 
                    (eventType is MapEventType))
                {
                    var mapEventType = (MapEventType) eventType;
                    var nested = (DataMap) mapEventType.Types.Get(name);
                    foreach (var nestedProperty in nested.Keys) {
                        flattenPropertyTypes.Put(name + "." + nestedProperty, nested.Get(nestedProperty));
                    }
                }
                else if (asType != null)
                {
                    if (asType.IsNullable()) {
                        asType = Nullable.GetUnderlyingType(asType);
                    }

                    if ((!asType.IsPrimitive) && (asType != typeof(string)))
                    {
                        var magicType = MagicType.GetCachedType(asType);
                        foreach(var magicProperty in magicType.GetAllProperties(false)) {
                            if (magicProperty.CanWrite) {
                                flattenPropertyTypes[name + '.' + magicProperty.Name] = magicProperty.PropertyType;
                            }
                        }
                    }
                    else {
                        flattenPropertyTypes[name] = type;
                    }
                }
                else {
                    flattenPropertyTypes[name] = type;
                }
            }

            return flattenPropertyTypes;
        }

        private void UpdateTotalDelay(IDictionary<string, object> map, bool isFirstRow)
        {
            if (_eventsPerSec != null)
            {
                var msecPerEvent = 1000 / _eventsPerSec.Value;
                _totalDelay += msecPerEvent;
            }
            else if (_adapterSpec.TimestampColumn != null)
            {
                var timestamp = ResolveTimestamp(map);
                if (timestamp == null) {
                    throw new EPException("Couldn't resolve the timestamp for record " + map.RenderAny());
                }
                else if (timestamp < 0)
                {
                    throw new EPException("Encountered negative timestamp for CSV record : " + map.RenderAny());
                }
                else
                {
                    long? timestampDifference;
                    if (timestamp < _lastTimestamp)
                    {
                        if (!isFirstRow)
                        {
                            throw new EPException("Subsequent timestamp " + timestamp + " is smaller than previous timestamp " + _lastTimestamp);
                        }

                        timestampDifference = timestamp;
                    }
                    else
                    {
                        timestampDifference = timestamp - _lastTimestamp;
                    }
                    _lastTimestamp = timestamp;
                    _totalDelay += timestampDifference.Value;
                }
            }
        }

        private long? ResolveTimestamp(IDictionary<string, object> map)
        {
            if (_adapterSpec.TimestampColumn != null)
            {
                var value = map.Get(_adapterSpec.TimestampColumn);
                return long.Parse(value.ToString());
            }
            
            return null;
        }

        private DataMap ResolvePropertyTypes(DataMap propertyTypes)
        {
            if (propertyTypes != null)
            {
                return propertyTypes;
            }

            var regex = new Regex(@"\s");

            var result = new Dictionary<string, object>();
            for (var i = 0; i < _propertyOrder.Length; i++) {
                var name = _propertyOrder[i];
                var type = typeof (string);
                if (name.Contains(" ")) {
                    var typeAndName = regex.Split(name);
                    try {
                        name = typeAndName[1];
                        type = TypeHelper.ResolveType(typeAndName[0]).GetBoxedType();
                        _propertyOrder[i] = name;
                    } catch (Exception e) {
                        Log.Warn("Unable to use given type for property, will default to String: " + _propertyOrder[i], e);
                    }
                }
                result.Put(name, type);
            }
            return result;
        }

        private static bool IsUsingTitleRow(string[] firstRow, string[] propertyOrder)
        {
            if (firstRow == null)
            {
                return false;
            }
            ISet<string> firstRowSet = new SortedSet<string>(firstRow);
            ISet<string> propertyOrderSet = new SortedSet<string>(propertyOrder);
            return firstRowSet.SetEquals(propertyOrderSet);
        }

        /// <summary>
        /// Gets the first row.
        /// </summary>
        /// <value>The first row.</value>
        
        private string[] FirstRow
        {
            get
            {
                string[] firstRow;
                try
                {
                	firstRow = _reader.GetNextRecord();
                }
                catch (EndOfStreamException)
                {
                    _atEof = true;
                    firstRow = null;
                }
                return firstRow;
            }
        }

        private static void AssertValidEventsPerSec(int? eventsPerSec)
        {
            if (eventsPerSec != null)
            {
                if (eventsPerSec < 1 || eventsPerSec > 1000)
                {
                    throw new ArgumentException("Illegal value of eventsPerSec:" + eventsPerSec);
                }
            }
        }

        private static void AssertValidParameters(EPRuntime runtime, CSVInputAdapterSpec adapterSpec)
        {
            if (!(runtime is EPRuntimeSPI))
            {
                throw new ArgumentException("Invalid type of EPServiceProvider");
            }

            if (adapterSpec.EventTypeName == null)
            {
                throw new ArgumentException("eventTypeName cannot be null");
            }

            if (adapterSpec.AdapterInputSource == null)
            {
                throw new ArgumentException("adapterInputSource cannot be null");
            }

            AssertValidEventsPerSec(adapterSpec.EventsPerSec);

            if (adapterSpec.IsLooping && !adapterSpec.AdapterInputSource.IsResettable)
            {
                throw new EPException("Cannot loop on a non-resettable input source");
            }
        }

        private static readonly Type[] ParameterTypes = new [] { typeof(string) };
        private static readonly Dictionary<Type, ObjectFactory<string>> StaticTypeTable;

        static CSVInputAdapter()
        {
            StaticTypeTable = new Dictionary<Type, ObjectFactory<string>>();
            StaticTypeTable[typeof(bool)] = EasyParser<bool>;
            StaticTypeTable[typeof(sbyte)] = EasyParser<sbyte>;
            StaticTypeTable[typeof(short)] = EasyParser<short>;
            StaticTypeTable[typeof(int)] = EasyParser<int>;
            StaticTypeTable[typeof(long)] = EasyParser<long>;
            StaticTypeTable[typeof(byte)] = EasyParser<byte>;
            StaticTypeTable[typeof(ushort)] = EasyParser<ushort>;
            StaticTypeTable[typeof(uint)] = EasyParser<uint>;
            StaticTypeTable[typeof(ulong)] = EasyParser<ulong>;
            StaticTypeTable[typeof(float)] = EasyParser<float>;
            StaticTypeTable[typeof(double)] = EasyParser<double>;
            StaticTypeTable[typeof(decimal)] = EasyParser<decimal>;

            StaticTypeTable[typeof(bool?)] = EasyParser<bool>;
            StaticTypeTable[typeof(sbyte?)] = EasyParser<sbyte>;
            StaticTypeTable[typeof(short?)] = EasyParser<short>;
            StaticTypeTable[typeof(int?)] = EasyParser<int>;
            StaticTypeTable[typeof(long?)] = EasyParser<long>;
            StaticTypeTable[typeof(byte?)] = EasyParser<byte>;
            StaticTypeTable[typeof(ushort?)] = EasyParser<ushort>;
            StaticTypeTable[typeof(uint?)] = EasyParser<uint>;
            StaticTypeTable[typeof(ulong?)] = EasyParser<ulong>;
            StaticTypeTable[typeof(float?)] = EasyParser<float>;
            StaticTypeTable[typeof(double?)] = EasyParser<double>;
            StaticTypeTable[typeof(decimal?)] = EasyParser<decimal>;

            StaticTypeTable[typeof(string)] = ProxyParser;
        }

        private static IContainer GetContainer(EPRuntime runtime)
        {
            if (runtime is EPRuntimeSPI spi)
                return spi.Container;
            throw new ArgumentException("Container is missing");
        }
    }
}
