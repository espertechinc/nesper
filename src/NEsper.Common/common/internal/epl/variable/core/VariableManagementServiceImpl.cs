///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.variable;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;

using static com.espertech.esper.common.@internal.context.util.StatementCPCacheService;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    /// Variables service for reading and writing variables, and for setting a version number for the current thread to
    /// consider variables for.
    /// <para>
    /// Consider a statement as follows: select * from MyEvent as A where A.val &gt; var1 and A.val2 &gt; var1 and A.val3 &gt; var2
    /// </para>
    /// <para>
    /// Upon statement execution we need to guarantee that the same atomic value for all variables is applied for all
    /// variable reads (by expressions typically) within the statement.
    /// </para>
    /// <para>
    /// Designed to support:
    /// however writes are very fast (entry to collection plus increment an int) and therefore blocking should not be an issue
    /// </para>
    /// <para>
    /// As an alternative to a version-based design, a read-lock for the variable space could also be used, with the following
    /// disadvantages: The write lock may just not be granted unless fair locks are used which are more expensive; And
    /// a read-lock is more expensive to acquire for multiple CPUs; A thread-local is still need to deal with
    /// "set var1=3, var2=var1+1" assignments where the new uncommitted value must be visible in the local evaluation.
    /// </para>
    /// <para>
    /// Every new write to a variable creates a new version. Thus when reading variables, readers can ignore newer versions
    /// and a read lock is not required in most circumstances.
    /// </para>
    ///
    /// <para>This algorithm works as follows:
    /// <para>A thread processing an event into the runtimevia sendEvent() calls the "setLocalVersion" method once
    /// before processing a statement that has variables.
    /// This places into a threadlocal variable the current version number, say version 570.
    /// </para>
    /// <para>
    /// A statement that reads a variable has an variable node that has a VariableReader handle
    /// obtained during validation (example).
    /// </para>
    /// <para>
    /// The VariableReader takes the version from the threadlocal (570) and compares the version number with the
    /// version numbers held for the variable.
    /// If the current version is same or lower (520, as old or older) then the threadlocal version,
    /// then use the current value.
    /// If the current version is higher (571, newer) then the threadlocal version, then go to the prior value.
    /// Use the prior value until a version is found that as old or older then the threadlocal version.
    /// </para>
    /// <para>
    /// If no version can be found that is old enough, output a warning and return the newest version.
    /// This should not happen, unless a thread is executing for very long within a single statement such that
    /// lifetime-old-version times period passed before the thread asks for variable values.
    /// </para>
    /// <para>
    /// As version numbers are counted up they may reach a boundary. Any write transaction after the boundary
    /// is reached performs a roll-over. In a roll-over, all variables version lists are
    /// newly created and any existing threads that read versions go against a (old) high-collection,
    /// while new threads reading the reset version go against a new low-collection.
    /// </para>
    /// <para>
    /// The class also allows an optional state handler to be plugged in to handle persistence for variable state.
    /// The state handler gets invoked when a variable changes value, and when a variable gets created
    /// to obtain the current value from persistence, if any.
    /// </para>
    /// </para>
    /// </summary>
    public class VariableManagementServiceImpl : VariableManagementService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Sets the boundary above which a reader considers the high-version list of variable values.
        /// For use in roll-over when the current version number overflows the ROLLOVER_WRITER_BOUNDARY.
        /// </summary>
        internal const int ROLLOVER_READER_BOUNDARY = int.MaxValue - 100000;

        /// <summary>
        /// Applicable for each variable if more then the number of versions accumulated, check
        /// timestamps to determine if a version can be expired.
        /// </summary>
        internal const int HIGH_WATERMARK_VERSIONS = 50;

        // Each variable has an index number, a context-partition id, a current version and a list of values
        private readonly IList<IDictionary<int, VariableReader>> variableVersionsPerCP;

        // Each variable and a context-partition id may have a set of callbacks to invoke when the variable changes
        private readonly IList<IDictionary<int, ICollection<VariableChangeCallback>>> changeCallbacksPerCP;

        // Keep the variable list
        private readonly IDictionary<string, VariableDeployment> deploymentsWithVariables;

        // Write lock taken on write of any variable; and on read of older versions
        private readonly IReaderWriterLock readWriteLock;

        // Thread-local for the visible version per thread
        private VariableVersionThreadLocal versionThreadLocal = new VariableVersionThreadLocal();

        // Number of milliseconds that old versions of a variable are allowed to live
        private readonly long millisecondLifetimeOldVersions;
        private readonly TimeProvider timeProvider;
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly VariableStateNonConstHandler optionalStateHandler;
        private volatile int currentVersionNumber;
        private int currentVariableNumber;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "millisecondLifetimeOldVersions">number of milliseconds a version may hang around before expiry</param>
        /// <param name = "timeProvider">provides the current time</param>
        /// <param name = "optionalStateHandler">a optional plug-in that may store variable state and retrieve state upon creation</param>
        /// <param name = "eventBeanTypedEventFactory">event adapters</param>
        public VariableManagementServiceImpl(
            IReaderWriterLockManager readerWriterLockManager,
            long millisecondLifetimeOldVersions,
            TimeProvider timeProvider,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            VariableStateNonConstHandler optionalStateHandler)
            : this(
                readerWriterLockManager,
                0,
                millisecondLifetimeOldVersions,
                timeProvider,
                eventBeanTypedEventFactory,
                optionalStateHandler)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "startVersion">the first version number to start from</param>
        /// <param name = "millisecondLifetimeOldVersions">number of milliseconds a version may hang around before expiry</param>
        /// <param name = "timeProvider">provides the current time</param>
        /// <param name = "optionalStateHandler">a optional plug-in that may store variable state and retrieve state upon creation</param>
        /// <param name = "eventBeanTypedEventFactory">for finding event types</param>
        protected VariableManagementServiceImpl(
            IReaderWriterLockManager readerWriterLockManager,
            int startVersion,
            long millisecondLifetimeOldVersions,
            TimeProvider timeProvider,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            VariableStateNonConstHandler optionalStateHandler)
        {
            this.millisecondLifetimeOldVersions = millisecondLifetimeOldVersions;
            this.timeProvider = timeProvider;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this.optionalStateHandler = optionalStateHandler;
            deploymentsWithVariables = new Dictionary<string, VariableDeployment>().WithNullKeySupport();
            readWriteLock = readerWriterLockManager.CreateLock(GetType());
            variableVersionsPerCP = new List<IDictionary<int, VariableReader>>();
            changeCallbacksPerCP = new List<IDictionary<int, ICollection<VariableChangeCallback>>>();
            currentVersionNumber = startVersion;
        }

        public void Destroy()
        {
            versionThreadLocal = new VariableVersionThreadLocal();
        }

        public void RemoveVariableIfFound(
            string deploymentId,
            string variableName)
        {
            lock (this) {
                var entry = deploymentsWithVariables.Get(deploymentId);
                if (entry == null) {
                    return;
                }

                var variable = entry.GetVariable(variableName);
                if (variable == null) {
                    return;
                }

                if (Log.IsDebugEnabled) {
                    Log.Debug("Removing variable '" + variableName + "'");
                }

                entry.Remove(variableName);
                if (optionalStateHandler != null && !variable.MetaData.IsConstant) {
                    var readers = variableVersionsPerCP[variable.VariableNumber];
                    ICollection<int> cps = EmptySet<int>.Instance;
                    if (readers != null) {
                        cps = readers.Keys;
                    }

                    optionalStateHandler.RemoveVariable(variable, deploymentId, cps);
                }

                var number = variable.VariableNumber;
                variableVersionsPerCP[number] = null;
                changeCallbacksPerCP[number] = null;
            }
        }

        public void SetLocalVersion()
        {
            versionThreadLocal.CurrentThread.Version = currentVersionNumber;
        }

        public void RegisterCallback(
            string deploymentId,
            string variableName,
            int agentInstanceId,
            VariableChangeCallback variableChangeCallback)
        {
            var entry = deploymentsWithVariables.Get(deploymentId);
            if (entry == null) {
                return;
            }

            var variable = entry.GetVariable(variableName);
            if (variable == null) {
                return;
            }

            var cps = changeCallbacksPerCP[variable.VariableNumber];
            if (cps == null) {
                cps = new Dictionary<int, ICollection<VariableChangeCallback>>();
                changeCallbacksPerCP[variable.VariableNumber] = cps;
            }

            if (variable.MetaData.OptionalContextName == null) {
                agentInstanceId = DEFAULT_AGENT_INSTANCE_ID;
            }

            var callbacks = cps.Get(agentInstanceId);
            if (callbacks == null) {
                callbacks = new CopyOnWriteArraySet<VariableChangeCallback>();
                cps.Put(agentInstanceId, callbacks);
            }

            callbacks.Add(variableChangeCallback);
        }

        public void UnregisterCallback(
            string deploymentId,
            string variableName,
            int agentInstanceId,
            VariableChangeCallback variableChangeCallback)
        {
            var entry = deploymentsWithVariables.Get(deploymentId);
            if (entry == null) {
                return;
            }

            var variable = entry.GetVariable(variableName);
            if (variable == null) {
                return;
            }

            var cps = changeCallbacksPerCP[variable.VariableNumber];
            if (cps == null) {
                return;
            }

            if (variable.MetaData.OptionalContextName == null) {
                agentInstanceId = 0;
            }

            var callbacks = cps.Get(agentInstanceId);
            if (callbacks != null) {
                callbacks.Remove(variableChangeCallback);
            }
        }

        public void AddVariable(
            string deploymentId,
            VariableMetaData metaData,
            string optionalDeploymentIdContext,
            DataInputOutputSerde optionalSerde)
        {
            lock (this) {
                // check if already exists
                var deploymentEntry = deploymentsWithVariables.Get(deploymentId);
                if (deploymentEntry != null) {
                    var variable = deploymentEntry.GetVariable(metaData.VariableName);
                    if (variable != null) {
                        throw new ArgumentException(
                            "Variable already exists by name '" +
                            metaData.VariableName +
                            "' and deployment '" +
                            deploymentId +
                            "'");
                    }
                }
                else {
                    deploymentEntry = new VariableDeployment();
                    deploymentsWithVariables.Put(deploymentId, deploymentEntry);
                }

                // find empty spot
                var emptySpot = -1;
                var count = 0;
                foreach (var entry in variableVersionsPerCP) {
                    if (entry == null) {
                        emptySpot = count;
                        break;
                    }

                    count++;
                }

                int variableNumber;
                if (emptySpot != -1) {
                    variableNumber = emptySpot;
                    variableVersionsPerCP[emptySpot] = new ConcurrentDictionary<int, VariableReader>();
                    changeCallbacksPerCP[emptySpot] = null;
                }
                else {
                    variableNumber = currentVariableNumber;
                    variableVersionsPerCP.Add(new ConcurrentDictionary<int, VariableReader>());
                    changeCallbacksPerCP.Add(null);
                    currentVariableNumber++;
                }

                var variableX = new Variable(variableNumber, deploymentId, metaData, optionalDeploymentIdContext);
                deploymentEntry.AddVariable(metaData.VariableName, variableX);
                if (optionalStateHandler != null && !metaData.IsConstant) {
                    optionalStateHandler.AddVariable(deploymentId, metaData.VariableName, variableX, optionalSerde);
                }
            }
        }

        public void AllocateVariableState(
            string deploymentId,
            string variableName,
            int agentInstanceId,
            bool recovery,
            NullableObject<object> initialValue,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            var entry = deploymentsWithVariables.Get(deploymentId);
            if (entry == null) {
                throw new ArgumentException("Failed to find variable deployment id '" + deploymentId + "'");
            }

            var variable = entry.GetVariable(variableName);
            if (variable == null) {
                throw new ArgumentException("Failed to find variable '" + variableName + "'");
            }

            // Check current state - see if the variable exists in the state handler
            object initialState;
            if (initialValue != null) {
                initialState = initialValue.Value;
            }
            else {
                initialState = variable.MetaData.ValueWhenAvailable;
            }

            if (variable.MetaData.EventType != null && initialState != null && !(initialState is EventBean)) {
                initialState = eventBeanTypedEventFactory.AdapterForTypedObject(
                    initialState,
                    variable.MetaData.EventType);
            }

            if (optionalStateHandler != null && !variable.MetaData.IsConstant) {
                var priorValue = optionalStateHandler.GetHasState(variable, agentInstanceId);
                if (recovery) {
                    if (priorValue != null) {
                        initialState = priorValue.Value;
                    }
                }
                else {
                    if (priorValue == null) { // we do not already have a value
                        optionalStateHandler.SetState(variable, agentInstanceId, initialState);
                    }
                    else {
                        initialState = priorValue.Value;
                    }
                }
            }

            // create new holder for versions
            var timestamp = timeProvider.Time;
            var valuePerVersion = new VersionedValueList<object>(
                variableName,
                currentVersionNumber,
                initialState,
                timestamp,
                millisecondLifetimeOldVersions,
                readWriteLock.ReadLock,
                HIGH_WATERMARK_VERSIONS,
                false);
            var cps = variableVersionsPerCP[variable.VariableNumber];
            var reader = new VariableReader(variable, versionThreadLocal, valuePerVersion);
            cps.Put(agentInstanceId, reader);
        }

        public void DeallocateVariableState(
            string deploymentId,
            string variableName,
            int agentInstanceId)
        {
            var entry = deploymentsWithVariables.Get(deploymentId);
            if (entry == null) {
                throw new ArgumentException("Failed to find variable deployment id '" + deploymentId + "'");
            }

            var variable = entry.GetVariable(variableName);
            if (variable == null) {
                throw new ArgumentException("Failed to find variable '" + variableName + "'");
            }

            var cps = variableVersionsPerCP[variable.VariableNumber];
            cps.Remove(agentInstanceId);
            if (optionalStateHandler != null && !variable.MetaData.IsConstant) {
                optionalStateHandler.RemoveState(variable, agentInstanceId);
            }
        }

        public Variable GetVariableMetaData(
            string deploymentId,
            string variableName)
        {
            var entry = deploymentsWithVariables.Get(deploymentId);
            if (entry == null) {
                return null;
            }

            return entry.GetVariable(variableName);
        }

        public VariableReader GetReader(
            string deploymentId,
            string variableName,
            int agentInstanceIdAccessor)
        {
            var entry = deploymentsWithVariables.Get(deploymentId);
            if (entry == null) {
                return null;
            }

            var variable = entry.GetVariable(variableName);
            if (variable == null) {
                return null;
            }

            var cps = variableVersionsPerCP[variable.VariableNumber];
            if (variable.MetaData.OptionalContextName == null) {
                return cps.Get(DEFAULT_AGENT_INSTANCE_ID);
            }

            return cps.Get(agentInstanceIdAccessor);
        }

        public void Write(
            int variableNumber,
            int agentInstanceId,
            object newValue)
        {
            var entry = versionThreadLocal.CurrentThread;
            if (entry.Uncommitted == null) {
                entry.Uncommitted = new Dictionary<int, Pair<int, object>>();
            }

            entry.Uncommitted.Put(variableNumber, new Pair<int, object>(agentInstanceId, newValue));
        }

        public void Commit()
        {
            var entry = versionThreadLocal.CurrentThread;
            if (entry.Uncommitted == null) {
                return;
            }

            // get new version for adding the new values (1 or many new values)
            var newVersion = currentVersionNumber + 1;
            if (currentVersionNumber == ROLLOVER_READER_BOUNDARY) {
                // Roll over to new collections;
                // This honors existing threads that will now use the "high" collection in the reader for high version requests
                // and low collection (new and updated) for low version requests
                RollOver();
                newVersion = 2;
            }

            var timestamp = timeProvider.Time;
            // apply all uncommitted changes
            foreach (var uncommittedEntry in entry.Uncommitted) {
                var cps = variableVersionsPerCP[uncommittedEntry.Key];
                var reader = cps.Get(uncommittedEntry.Value.First);
                var versions = reader.VersionsLow;
                // add new value as a new version
                var newValue = uncommittedEntry.Value.Second;
                var oldValue = versions.AddValue(newVersion, newValue, timestamp);
                // make a callback that the value changed
                var cpsCallback = changeCallbacksPerCP[uncommittedEntry.Key];
                if (cpsCallback != null) {
                    var callbacks = cpsCallback.Get(uncommittedEntry.Value.First);
                    if (callbacks != null) {
                        foreach (var callback in callbacks) {
                            callback.Update(newValue, oldValue);
                        }
                    }
                }

                // Check current state - see if the variable exists in the state handler
                if (optionalStateHandler != null) {
                    var metaData = reader.MetaData;
                    if (!metaData.IsConstant) {
                        var agentInstanceId = metaData.OptionalContextName == null
                            ? DEFAULT_AGENT_INSTANCE_ID
                            : uncommittedEntry.Value.First;
                        optionalStateHandler.SetState(reader.Variable, agentInstanceId, newValue);
                    }
                }
            }

            // this makes the new values visible to other threads (not this thread unless set-version called again)
            currentVersionNumber = newVersion;
            entry.Uncommitted = null; // clean out uncommitted variables
        }

        public void Rollback()
        {
            var entry = versionThreadLocal.CurrentThread;
            entry.Uncommitted = null;
        }

        /// <summary>
        /// Rollover includes creating a new
        /// </summary>
        private void RollOver()
        {
            foreach (var entryCP in variableVersionsPerCP) {
                foreach (var entry in entryCP) {
                    var name = entry.Value.MetaData.VariableName;
                    var timestamp = timeProvider.Time;
                    // Construct a new collection, forgetting the history
                    var versionsOld = entry.Value.VersionsLow;
                    var currentValue = versionsOld.CurrentAndPriorValue.CurrentVersion.Value;
                    var versionsNew = new VersionedValueList<object>(
                        name,
                        1,
                        currentValue,
                        timestamp,
                        millisecondLifetimeOldVersions,
                        readWriteLock.ReadLock,
                        HIGH_WATERMARK_VERSIONS,
                        false);
                    // Tell the reader to use the high collection for old requests
                    entry.Value.VersionsHigh = versionsOld;
                    entry.Value.VersionsLow = versionsNew;
                }
            }
        }

        public void CheckAndWrite(
            string deploymentId,
            string variableName,
            int agentInstanceId,
            object newValue)
        {
            var entry = deploymentsWithVariables.Get(deploymentId);
            if (entry == null) {
                throw new ArgumentException("Failed to find variable deployment id '" + deploymentId + "'");
            }

            var variable = entry.GetVariable(variableName);
            var variableNumber = variable.VariableNumber;
            if (newValue == null) {
                Write(variableNumber, agentInstanceId, null);
                return;
            }

            var valueType = newValue.GetType();
            if (variable.MetaData.EventType != null) {
                if (!TypeHelper.IsSubclassOrImplementsInterface(
                        newValue.GetType(),
                        variable.MetaData.EventType.UnderlyingType)) {
                    throw new VariableValueException(
                        "Variable '" +
                        variableName +
                        "' of declared event type '" +
                        variable.MetaData.EventType.Name +
                        "' underlying type '" +
                        variable.MetaData.EventType.UnderlyingType.Name +
                        "' cannot be assigned a value of type '" +
                        valueType.CleanName() +
                        "'");
                }

                var eventBean = eventBeanTypedEventFactory.AdapterForTypedObject(newValue, variable.MetaData.EventType);
                Write(variableNumber, agentInstanceId, eventBean);
                return;
            }

            var variableType = variable.MetaData.Type;
            if (valueType.Equals(variableType) || variableType == typeof(object)) {
                Write(variableNumber, agentInstanceId, newValue);
                return;
            }

            if (TypeHelper.IsSubclassOrImplementsInterface(valueType, variableType)) {
                Write(variableNumber, agentInstanceId, newValue);
                return;
            }

            if (!variableType.IsTypeNumeric() || !valueType.IsTypeNumeric()) {
                throw new VariableValueException(
                    VariableUtil.GetAssigmentExMessage(variableName, variableType, valueType));
            }

            // determine if the expression type can be assigned
            if (!valueType.CanCoerce(variableType)) {
                throw new VariableValueException(
                    VariableUtil.GetAssigmentExMessage(variableName, variableType, valueType));
            }

            var valueCoerced = TypeHelper.CoerceBoxed(newValue, variableType);
            Write(variableNumber, agentInstanceId, valueCoerced);
        }

        public IDictionary<int, VariableReader> GetReadersPerCP(
            string deploymentId,
            string variableName)
        {
            var entry = deploymentsWithVariables.Get(deploymentId);
            if (entry == null) {
                throw new ArgumentException("Failed to find variable deployment id '" + deploymentId + "'");
            }

            var variable = entry.GetVariable(variableName);
            return variableVersionsPerCP[variable.VariableNumber];
        }

        public IDictionary<DeploymentIdNamePair, VariableReader> VariableReadersNonCP {
            get {
                IDictionary<DeploymentIdNamePair, VariableReader> result = new Dictionary<DeploymentIdNamePair, VariableReader>();
                foreach (var deployment in deploymentsWithVariables) {
                    foreach (var variable in deployment.Value.Variables) {
                        var variableNum = variable.Value.VariableNumber;
                        if (variable.Value.MetaData.OptionalContextName == null) {
                            foreach (var entry in
                                     variableVersionsPerCP[variableNum]) {
                                result.Put(new DeploymentIdNamePair(deployment.Key, variable.Key), entry.Value);
                            }
                        }
                    }
                }

                return result;
            }
        }

        public VariableStateNonConstHandler OptionalStateHandler => optionalStateHandler;
        public IDictionary<string, VariableDeployment> DeploymentsWithVariables => deploymentsWithVariables;

        public void TraverseVariables(BiConsumer<string, Variable> consumer)
        {
            foreach (var entry in deploymentsWithVariables) {
                foreach (var variable in entry.Value.Variables) {
                    consumer.Invoke(entry.Key, variable.Value);
                }
            }
        }

        public IReaderWriterLock ReadWriteLock => readWriteLock;
    }
} // end of namespace