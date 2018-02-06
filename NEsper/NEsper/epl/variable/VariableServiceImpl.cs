///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.variable
{
    /// <summary>
    /// Variables service for reading and writing variables, and for setting a version number for the current thread to
    /// consider variables for.
    /// <para />
    /// Consider a statement as follows: select * from MyEvent as A where A.val &gt; var1 and A.val2 &gt; var1 and A.val3 &gt; var2
    /// <para />
    /// Upon statement execution we need to guarantee that the same atomic value for all variables is applied for all
    /// variable reads (by expressions typically) within the statement.
    /// <para />
    /// Designed to support:
    /// however writes are very fast (entry to collection plus increment an int) and therefore blocking should not be an issue
    /// <para />
    /// As an alternative to a version-based design, a read-lock for the variable space could also be used, with the following
    /// disadvantages: The write lock may just not be granted unless fair locks are used which are more expensive; And
    /// a read-lock is more expensive to acquire for multiple CPUs; A thread-local is still need to deal with
    /// "set var1=3, var2=var1+1" assignments where the new uncommitted value must be visible in the local evaluation.
    /// <para />
    /// Every new write to a variable creates a new version. Thus when reading variables, readers can ignore newer versions
    /// and a read lock is not required in most circumstances.
    /// <para />
    /// This algorithm works as follows:
    /// <para />
    /// A thread processing an event into the engine via sendEvent() calls the "setLocalVersion" method once
    /// before processing a statement that has variables.
    /// This places into a threadlocal variable the current version number, say version 570.
    /// <para />
    /// A statement that reads a variable has an <seealso cref="ExprVariableNode" /> that has a <seealso cref="com.espertech.esper.epl.variable.VariableReader" /> handle
    /// obtained during validation (example).
    /// <para />
    /// The <seealso cref="com.espertech.esper.epl.variable.VariableReader" /> takes the version from the threadlocal (570) and compares the version number with the
    /// version numbers held for the variable.
    /// If the current version is same or lower (520, as old or older) then the threadlocal version,
    /// then use the current value.
    /// If the current version is higher (571, newer) then the threadlocal version, then go to the prior value.
    /// Use the prior value until a version is found that as old or older then the threadlocal version.
    /// <para />
    /// If no version can be found that is old enough, output a warning and return the newest version.
    /// This should not happen, unless a thread is executing for very long within a single statement such that
    /// lifetime-old-version time speriod passed before the thread asks for variable values.
    /// <para />
    /// As version numbers are counted up they may reach a boundary. Any write transaction after the boundary
    /// is reached performs a roll-over. In a roll-over, all variables version lists are
    /// newly created and any existing threads that read versions go against a (old) high-collection,
    /// while new threads reading the reset version go against a new low-collection.
    /// <para />
    /// The class also allows an optional state handler to be plugged in to handle persistence for variable state.
    /// The state handler gets invoked when a variable changes value, and when a variable gets created
    /// to obtain the current value from persistence, if any.
    /// </summary>
    public class VariableServiceImpl : VariableService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Sets the boundary above which a reader considers the high-version list of variable values.
        /// For use in roll-over when the current version number overflows the ROLLOVER_WRITER_BOUNDARY.
        /// </summary>
        public const int ROLLOVER_READER_BOUNDARY = int.MaxValue - 100000;

        /// <summary>
        /// Applicable for each variable if more then the number of versions accumulated, check
        /// timestamps to determine if a version can be expired.
        /// </summary>
        public const int HIGH_WATERMARK_VERSIONS = 50;

        // Each variable has an index number, a context-partition id, a current version and a list of values
        private readonly List<ConcurrentDictionary<int, VariableReader>> _variableVersionsPerCP;

        // Each variable and a context-partition id may have a set of callbacks to invoke when the variable changes
        private readonly List<IDictionary<int, ICollection<VariableChangeCallback>>> _changeCallbacksPerCP;

        // Keep the variable list
        private readonly IDictionary<String, VariableMetaData> _variables;

        // Write lock taken on write of any variable; and on read of older versions
        private readonly IReaderWriterLock _readWriteLock;

        // Thread-local for the visible version per thread
        private VariableVersionThreadLocal _versionThreadLocal;

        // Number of milliseconds that old versions of a variable are allowed to live
        private readonly long _millisecondLifetimeOldVersions;
        private readonly TimeProvider _timeProvider;
        private readonly EventAdapterService _eventAdapterService;
        private readonly VariableStateHandler _optionalStateHandler;

        private volatile int _currentVersionNumber;
        private int _currentVariableNumber;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="millisecondLifetimeOldVersions">number of milliseconds a version may hang around before expiry</param>
        /// <param name="timeProvider">provides the current time</param>
        /// <param name="eventAdapterService">event adapters</param>
        /// <param name="optionalStateHandler">a optional plug-in that may store variable state and retrieve state upon creation</param>
        public VariableServiceImpl(
            IContainer container,
            long millisecondLifetimeOldVersions, 
            TimeProvider timeProvider, 
            EventAdapterService eventAdapterService,
            VariableStateHandler optionalStateHandler)
            : this(container, 0, millisecondLifetimeOldVersions, timeProvider, eventAdapterService, optionalStateHandler)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="startVersion">the first version number to start from</param>
        /// <param name="millisecondLifetimeOldVersions">number of milliseconds a version may hang around before expiry</param>
        /// <param name="timeProvider">provides the current time</param>
        /// <param name="eventAdapterService">for finding event types</param>
        /// <param name="optionalStateHandler">a optional plug-in that may store variable state and retrieve state upon creation</param>
        public VariableServiceImpl(
            IContainer container,
            int startVersion, 
            long millisecondLifetimeOldVersions, 
            TimeProvider timeProvider,
            EventAdapterService eventAdapterService,
            VariableStateHandler optionalStateHandler)
        {
            _versionThreadLocal = new VariableVersionThreadLocal(container.ThreadLocalManager());
            _millisecondLifetimeOldVersions = millisecondLifetimeOldVersions;
            _timeProvider = timeProvider;
            _eventAdapterService = eventAdapterService;
            _optionalStateHandler = optionalStateHandler;
            _variables = new Dictionary<String, VariableMetaData>().WithNullSupport();
            _readWriteLock = container.RWLockManager().CreateLock(MethodBase.GetCurrentMethod().DeclaringType);
            _variableVersionsPerCP = new List<ConcurrentDictionary<int, VariableReader>>();
            _changeCallbacksPerCP = new List<IDictionary<int, ICollection<VariableChangeCallback>>>();
            _currentVersionNumber = startVersion;
        }

        public void Dispose()
        {
            _versionThreadLocal = new VariableVersionThreadLocal(null);
        }

        public void RemoveVariableIfFound(String name)
        {
            lock (this)
            {
                var metaData = _variables.Get(name);
                if (metaData == null)
                {
                    return;
                }

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Removing variable '" + name + "'");
                }
                _variables.Remove(name);

                if (_optionalStateHandler != null)
                {
                    ConcurrentDictionary<int, VariableReader> readers = _variableVersionsPerCP[metaData.VariableNumber];
                    IEnumerable<int> cps = Collections.GetEmptySet<int>();
                    if (readers != null)
                    {
                        cps = readers.Keys;
                    }
                    _optionalStateHandler.RemoveVariable(name, cps);
                }

                var number = metaData.VariableNumber;
                _variableVersionsPerCP[number] = null;
                _changeCallbacksPerCP[number] = null;
            }
        }

        public void SetLocalVersion()
        {
            _versionThreadLocal.CurrentThread.Version = _currentVersionNumber;
        }

        public void RegisterCallback(String variableName, int agentInstanceId, VariableChangeCallback variableChangeCallback)
        {
            var metaData = _variables.Get(variableName);
            if (metaData == null)
            {
                return;
            }

            IDictionary<int, ICollection<VariableChangeCallback>> cps = _changeCallbacksPerCP[metaData.VariableNumber];
            if (cps == null)
            {
                cps = new Dictionary<int, ICollection<VariableChangeCallback>>();
                _changeCallbacksPerCP[metaData.VariableNumber] = cps;
            }

            if (metaData.ContextPartitionName == null)
            {
                agentInstanceId = EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID;
            }

            ICollection<VariableChangeCallback> callbacks = cps.Get(agentInstanceId);
            if (callbacks == null)
            {
                callbacks = new CopyOnWriteArraySet<VariableChangeCallback>();
                cps.Put(agentInstanceId, callbacks);
            }
            callbacks.Add(variableChangeCallback);
        }

        public void UnregisterCallback(String variableName, int agentInstanceId, VariableChangeCallback variableChangeCallback)
        {
            var metaData = _variables.Get(variableName);
            if (metaData == null)
            {
                return;
            }

            var cps = _changeCallbacksPerCP[metaData.VariableNumber];
            if (cps == null)
            {
                return;
            }

            if (metaData.ContextPartitionName == null)
            {
                agentInstanceId = 0;
            }

            ICollection<VariableChangeCallback> callbacks = cps.Get(agentInstanceId);
            if (callbacks != null)
            {
                callbacks.Remove(variableChangeCallback);
            }
        }

        public void CreateNewVariable<T>(
            string optionalContextName,
            string variableName,
            bool constant,
            T value,
            EngineImportService engineImportService)
        {
            CreateNewVariable(
                optionalContextName,
                variableName,
                typeof(T).FullName,
                constant,
                typeof(T).IsArray, false,
                value,
                engineImportService);
        }

        public void CreateNewVariable(string optionalContextName, string variableName, string variableType, bool constant, bool array, bool arrayOfPrimitive, object value, EngineImportService engineImportService)
        {
            // Determime the variable type
            var primitiveType = TypeHelper.GetPrimitiveTypeForName(variableType);
            var type = TypeHelper.GetTypeForSimpleName(variableType);
            Type arrayType = null;
            EventType eventType = null;
            if (type == null)
            {
                if (variableType.ToLower() == "object")
                {
                    type = typeof(Object);
                }
                if (type == null)
                {
                    eventType = _eventAdapterService.GetEventTypeByName(variableType);
                    if (eventType != null)
                    {
                        type = eventType.UnderlyingType;
                    }
                }
                if (type == null)
                {
                    try
                    {
                        type = engineImportService.ResolveType(variableType, false);
                        if (array)
                        {
                            arrayType = TypeHelper.GetArrayType(type.GetBoxedType());
                        }
                    }
                    catch (EngineImportException e)
                    {
                        Log.Debug("Not found '" + type + "': " + e.Message, e);
                        // expected
                    }
                }
                if (type == null)
                {
                    throw new VariableTypeException("Cannot create variable '" + variableName + "', type '" +
                        variableType + "' is not a recognized type");
                }
                if (array && eventType != null)
                {
                    throw new VariableTypeException("Cannot create variable '" + variableName + "', type '" +
                            variableType + "' cannot be declared as an array type");
                }
            }
            else
            {
                if (array)
                {
                    if (arrayOfPrimitive)
                    {
                        if (primitiveType == null)
                        {
                            throw new VariableTypeException("Cannot create variable '" + variableName + "', type '" +
                                    variableType + "' is not a primitive type");
                        }
                        arrayType = TypeHelper.GetArrayType(primitiveType);
                    }
                    else
                    {
                        arrayType = TypeHelper.GetArrayType(type.GetBoxedType());
                    }
                }
            }

            if ((eventType == null) && (!type.IsBuiltinDataType()) && (type != typeof(object)) && !type.IsArray && !type.IsEnum)
            {
                if (array)
                {
                    throw new VariableTypeException("Cannot create variable '" + variableName + "', type '" +
                        variableType + "' cannot be declared as an array, only scalar types can be array");
                }

                eventType = _eventAdapterService.AddBeanType(type.GetDefaultTypeName(), type, false, false, false);
            }

            if (arrayType != null)
            {
                type = arrayType;
            }

            CreateNewVariable(variableName, optionalContextName, type, eventType, constant, value);
        }

        private void CreateNewVariable(String variableName, String optionalContextName, Type type, EventType eventType, bool constant, Object value)
        {
            lock (this)
            {
                // check type
                var variableType = type.GetBoxedType();

                // check if it exists
                var metaData = _variables.Get(variableName);
                if (metaData != null)
                {
                    throw new VariableExistsException(VariableServiceUtil.GetAlreadyDeclaredEx(variableName, false));
                }

                // find empty spot
                var emptySpot = -1;
                var count = 0;
                foreach (var entry in _variableVersionsPerCP)
                {
                    if (entry == null)
                    {
                        emptySpot = count;
                        break;
                    }
                    count++;
                }

                int variableNumber;
                if (emptySpot != -1)
                {
                    variableNumber = emptySpot;
                    _variableVersionsPerCP[emptySpot] = new ConcurrentDictionary<int, VariableReader>();
                    _changeCallbacksPerCP[emptySpot] = null;
                }
                else
                {
                    variableNumber = _currentVariableNumber;
                    _variableVersionsPerCP.Add(new ConcurrentDictionary<int, VariableReader>());
                    _changeCallbacksPerCP.Add(null);
                    _currentVariableNumber++;
                }

                // check coercion
                var coercedValue = value;
                if (eventType != null)
                {
                    if ((value != null) && (!TypeHelper.IsSubclassOrImplementsInterface(value.GetType(), eventType.UnderlyingType)))
                    {
                        throw new VariableTypeException("Variable '" + variableName
                                + "' of declared event type '" + eventType.Name + "' underlying type '" + eventType.UnderlyingType.GetCleanName() +
                                "' cannot be assigned a value of type '" + value.GetType().GetCleanName() + "'");
                    }
                    coercedValue = _eventAdapterService.AdapterForType(value, eventType);
                }
                else if (variableType == typeof(object))
                {
                    // no validation
                }
                else
                {
                    // allow string assignments to non-string variables
                    if ((coercedValue != null) && (coercedValue is String))
                    {
                        try
                        {
                            coercedValue = TypeHelper.Parse(variableType, (String)coercedValue);
                        }
                        catch (Exception ex)
                        {
                            throw new VariableTypeException(
                                string.Format(
                                    "Variable '{0}' of declared type {1} cannot be initialized by value '{2}': {3}: {4}",
                                    variableName,
                                    variableType.GetCleanName(),
                                    coercedValue,
                                    ex.GetType().FullName,
                                    ex.Message));
                        }
                    }

                    if ((coercedValue != null) && (!TypeHelper.IsSubclassOrImplementsInterface(coercedValue.GetType(), variableType)))
                    {
                        // if the declared type is not numeric or the init value is not numeric, fail
                        if ((!variableType.IsNumeric()) || (!(coercedValue.IsNumber())))
                        {
                            throw GetVariableTypeException(variableName, variableType, coercedValue.GetType());
                        }

                        if (!(coercedValue.GetType().CanCoerce(variableType)))
                        {
                            throw GetVariableTypeException(variableName, variableType, coercedValue.GetType());
                        }
                        // coerce
                        coercedValue = CoercerFactory.CoerceBoxed(coercedValue, variableType);
                    }
                }

                var initialState = coercedValue;
                VariableStateFactory stateFactory = new VariableStateFactoryConst(initialState);

                metaData = new VariableMetaData(variableName, optionalContextName, variableNumber, variableType, eventType, constant, stateFactory);
                _variables.Put(variableName, metaData);
            }
        }

        public void AllocateVariableState(String variableName, int agentInstanceId, StatementExtensionSvcContext extensionServicesContext, bool isRecoveringResilient)
        {
            var metaData = _variables.Get(variableName);
            if (metaData == null)
            {
                throw new ArgumentException("Failed to find variable '" + variableName + "'");
            }

            // Check current state - see if the variable exists in the state handler
            var initialState = metaData.VariableStateFactory.InitialState;
            if (_optionalStateHandler != null)
            {
                var priorValue = _optionalStateHandler.GetHasState(
                    variableName,
                    metaData.VariableNumber, agentInstanceId,
                    metaData.VariableType,
                    metaData.EventType, extensionServicesContext,
                    metaData.IsConstant);
                if (isRecoveringResilient)
                {
                    if (priorValue.First)
                    {
                        initialState = priorValue.Second;
                    }
                }
                else
                {
                    _optionalStateHandler.SetState(variableName, metaData.VariableNumber, agentInstanceId, initialState);
                }
            }

            // create new holder for versions
            var timestamp = _timeProvider.Time;
            var valuePerVersion = new VersionedValueList<Object>(variableName, _currentVersionNumber, initialState, timestamp, _millisecondLifetimeOldVersions, _readWriteLock.ReadLock, HIGH_WATERMARK_VERSIONS, false);
            var cps = _variableVersionsPerCP[metaData.VariableNumber];
            var reader = new VariableReader(metaData, _versionThreadLocal, valuePerVersion);
            cps.Put(agentInstanceId, reader);
        }

        public void DeallocateVariableState(String variableName, int agentInstanceId)
        {
            var metaData = _variables.Get(variableName);
            if (metaData == null)
            {
                throw new ArgumentException("Failed to find variable '" + variableName + "'");
            }

            VariableReader tempVariableReader;
            var cps = _variableVersionsPerCP[metaData.VariableNumber];
            cps.TryRemove(agentInstanceId, out tempVariableReader);

            if (_optionalStateHandler != null)
            {
                _optionalStateHandler.RemoveState(variableName, metaData.VariableNumber, agentInstanceId);
            }
        }

        public VariableMetaData GetVariableMetaData(String variableName)
        {
            return _variables.Get(variableName);
        }

        public VariableReader GetReader(String variableName, int agentInstanceIdAccessor)
        {
            var metaData = _variables.Get(variableName);
            if (metaData == null)
            {
                return null;
            }
            var cps = _variableVersionsPerCP[metaData.VariableNumber];
            if (metaData.ContextPartitionName == null)
            {
                return cps.Get(EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID);
            }
            return cps.Get(agentInstanceIdAccessor);
        }

        public String IsContextVariable(String variableName)
        {
            var metaData = _variables.Get(variableName);
            if (metaData == null)
            {
                return null;
            }
            return metaData.ContextPartitionName;
        }

        public void Write(int variableNumber, int agentInstanceId, Object newValue)
        {
            var entry = _versionThreadLocal.CurrentThread;
            if (entry.Uncommitted == null)
            {
                entry.Uncommitted = new Dictionary<int, Pair<int, Object>>();
            }
            entry.Uncommitted.Put(variableNumber, new Pair<int, Object>(agentInstanceId, newValue));
        }

        public IReaderWriterLock ReadWriteLock
        {
            get { return _readWriteLock; }
        }

        public void Commit()
        {
            var entry = _versionThreadLocal.CurrentThread;
            if (entry.Uncommitted == null)
            {
                return;
            }

            // get new version for adding the new values (1 or many new values)
            var newVersion = _currentVersionNumber + 1;

            if (_currentVersionNumber == ROLLOVER_READER_BOUNDARY)
            {
                // Roll over to new collections;
                // This honors existing threads that will now use the "high" collection in the reader for high version requests
                // and low collection (new and updated) for low version requests
                RollOver();
                newVersion = 2;
            }
            var timestamp = _timeProvider.Time;

            // apply all uncommitted changes
            foreach (KeyValuePair<int, Pair<int, object>> uncommittedEntry in entry.Uncommitted.ToList())
            {
                var cps = _variableVersionsPerCP[uncommittedEntry.Key];
                var reader = cps[uncommittedEntry.Value.First];
                var versions = reader.VersionsLow;

                // add new value as a new version
                var newValue = uncommittedEntry.Value.Second;
                var oldValue = versions.AddValue(newVersion, newValue, timestamp);

                // make a callback that the value changed
                var cpsCallback = _changeCallbacksPerCP[uncommittedEntry.Key];
                if (cpsCallback != null)
                {
                    var callbacks = cpsCallback.Get(uncommittedEntry.Value.First);
                    if (callbacks != null)
                    {
                        foreach (var callback in callbacks)
                        {
                            callback.Invoke(newValue, oldValue);
                        }
                    }
                }

                // Check current state - see if the variable exists in the state handler
                if (_optionalStateHandler != null)
                {
                    var name = versions.Name;
                    int agentInstanceId = reader.VariableMetaData.ContextPartitionName == null
                        ? EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID
                        : uncommittedEntry.Value.First;
                    _optionalStateHandler.SetState(name, uncommittedEntry.Key, agentInstanceId, newValue);
                }
            }

            // this makes the new values visible to other threads (not this thread unless set-version called again)
            _currentVersionNumber = newVersion;
            entry.Uncommitted = null;    // clean out uncommitted variables
        }

        public void Rollback()
        {
            var entry = _versionThreadLocal.CurrentThread;
            entry.Uncommitted = null;
        }

        /// <summary>
        /// Rollover includes creating a new
        /// </summary>
        private void RollOver()
        {
            foreach (var entryCP in _variableVersionsPerCP)
            {
                foreach (KeyValuePair<int, VariableReader> entry in entryCP)
                {
                    String name = entry.Value.VariableMetaData.VariableName;
                    var timestamp = _timeProvider.Time;

                    // Construct a new collection, forgetting the history
                    var versionsOld = entry.Value.VersionsLow;
                    var currentValue = versionsOld.CurrentAndPriorValue.CurrentVersion.Value;
                    var versionsNew = new VersionedValueList<Object>(name, 1, currentValue, timestamp, _millisecondLifetimeOldVersions, _readWriteLock.ReadLock, HIGH_WATERMARK_VERSIONS, false);

                    // Tell the reader to use the high collection for old requests
                    entry.Value.VersionsHigh = versionsOld;
                    entry.Value.VersionsLow = versionsNew;
                }
            }
        }

        public void CheckAndWrite(String variableName, int agentInstanceId, Object newValue)
        {
            var metaData = _variables.Get(variableName);
            var variableNumber = metaData.VariableNumber;

            if (newValue == null)
            {
                Write(variableNumber, agentInstanceId, null);
                return;
            }

            var valueType = newValue.GetType();

            if (metaData.EventType != null)
            {
                if ((!TypeHelper.IsSubclassOrImplementsInterface(newValue.GetType(), metaData.EventType.UnderlyingType)))
                {
                    throw new VariableValueException("Variable '" + variableName
                        + "' of declared event type '" + metaData.EventType.Name + "' underlying type '" + metaData.EventType.UnderlyingType.GetCleanName() +
                            "' cannot be assigned a value of type '" + valueType.GetCleanName() + "'");
                }
                var eventBean = _eventAdapterService.AdapterForType(newValue, metaData.EventType);
                Write(variableNumber, agentInstanceId, eventBean);
                return;
            }

            var variableType = metaData.VariableType;
            if ((valueType == variableType) || (variableType == typeof(object)))
            {
                Write(variableNumber, agentInstanceId, newValue);
                return;
            }

            // Look for simple boxing rules
            var valueTypeBoxed = valueType.GetBoxedType();
            var variableTypeBoxed = variableType.GetBoxedType();
            if (((valueType != valueTypeBoxed) || (variableType != variableTypeBoxed)) && ((valueTypeBoxed == variableTypeBoxed)))
            {
                Write(variableNumber, agentInstanceId, newValue);
                return;
            }

            if ((!variableType.IsNumeric()) || (!valueType.IsNumeric()))
            {
                throw new VariableValueException(VariableServiceUtil.GetAssigmentExMessage(variableName, variableType, valueType));
            }

            // determine if the expression type can be assigned
            if (!(TypeHelper.CanCoerce(valueType, variableType)))
            {
                throw new VariableValueException(VariableServiceUtil.GetAssigmentExMessage(variableName, variableType, valueType));
            }

            object valueCoerced = CoercerFactory.CoerceBoxed(newValue, variableType);
            Write(variableNumber, agentInstanceId, valueCoerced);
        }

        public override String ToString()
        {
            var writer = new StringWriter();
            foreach (var entryMeta in _variables)
            {
                var variableNum = entryMeta.Value.VariableNumber;
                foreach (KeyValuePair<int, VariableReader> entry in _variableVersionsPerCP[variableNum])
                {
                    var list = entry.Value.VersionsLow;
                    writer.Write("Variable '" + entry.Key + "' : " + list + "\n");
                }
            }
            return writer.ToString();
        }

        public IDictionary<string, VariableReader> VariableReadersNonCP
        {
            get
            {
                IDictionary<String, VariableReader> result = new Dictionary<String, VariableReader>();
                foreach (var entryMeta in _variables)
                {
                    var variableNum = entryMeta.Value.VariableNumber;
                    if (entryMeta.Value.ContextPartitionName == null)
                    {
                        foreach (KeyValuePair<int, VariableReader> entry in _variableVersionsPerCP[variableNum])
                        {
                            result.Put(entryMeta.Key, entry.Value);
                        }
                    }
                }
                return result;
            }
        }

        public ConcurrentDictionary<int, VariableReader> GetReadersPerCP(String variableName)
        {
            var metaData = _variables.Get(variableName);
            return _variableVersionsPerCP[metaData.VariableNumber];
        }

        private static VariableTypeException GetVariableTypeException(String variableName, Type variableType, Type initValueClass)
        {
            return new VariableTypeException("Variable '" + variableName
                    + "' of declared type " + variableType.GetCleanName() +
                    " cannot be initialized by a value of type " + initValueClass.GetCleanName());
        }
    }
}
