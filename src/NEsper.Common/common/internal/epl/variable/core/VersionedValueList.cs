///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    ///     A self-cleaning list of versioned-values.
    ///     <para />
    ///     The current and prior version are held for lock-less read access in a transient variable.
    ///     <para />
    ///     The list relies on transient as well as a read-lock to guard against concurrent modification. However a read lock
    ///     is only
    ///     taken when a list of old versions must be updated.
    ///     <para />
    ///     When a high watermark is reached, the list on write access removes old versions up to the
    ///     number of milliseconds compared to current write timestamp.
    ///     <para />
    ///     If an older version is requested then held by the list, the list can either throw an exception
    ///     or return the current value.
    /// </summary>
    public class VersionedValueList<T>
        where T : class
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly bool _errorWhenNotFound;
        private readonly int _highWatermark; // used for removing older versions
        private readonly long _millisecondLifetimeOldVersions;

        // Variables name and read lock; read lock used when older version then the prior version is requested

        // Holds the older versions
        private readonly List<VersionedValue<T>> _olderVersions;
        private readonly ILockable _readLock;

        // Hold the current and prior version for no-lock reading
        private volatile CurrentValue<T> _currentAndPriorValue;

        /// <summary>Ctor.</summary>
        /// <param name="name">variable name</param>
        /// <param name="initialVersion">first version number</param>
        /// <param name="initialValue">first value</param>
        /// <param name="timestamp">timestamp of first version</param>
        /// <param name="millisecondLifetimeOldVersions">
        ///     number of milliseconds after which older versions get expired and removed
        /// </param>
        /// <param name="readLock">for coordinating Update to old versions</param>
        /// <param name="highWatermark">
        ///     when the number of old versions reached high watermark, the list inspects size on every write
        /// </param>
        /// <param name="errorWhenNotFound">
        ///     true if an exception should be throw if the requested version cannot be found,
        ///     or false if the engine should log a warning
        /// </param>
        public VersionedValueList(
            string name,
            int initialVersion,
            T initialValue,
            long timestamp,
            long millisecondLifetimeOldVersions,
            ILockable readLock,
            int highWatermark,
            bool errorWhenNotFound)
        {
            Name = name;
            _readLock = readLock;
            _highWatermark = highWatermark;
            _olderVersions = new List<VersionedValue<T>>();
            _errorWhenNotFound = errorWhenNotFound;
            _millisecondLifetimeOldVersions = millisecondLifetimeOldVersions;
            _currentAndPriorValue = new CurrentValue<T>(
                new VersionedValue<T>(initialVersion, initialValue, timestamp),
                new VersionedValue<T>(-1, null, timestamp));
        }

        /// <summary>Returns the name of the value stored.</summary>
        /// <returns>value name</returns>
        public string Name { get; }

        /// <summary>Returns the current and prior version.</summary>
        /// <returns>value</returns>
        public CurrentValue<T> CurrentAndPriorValue => _currentAndPriorValue;

        /// <summary>Returns the list of old versions, for testing purposes.</summary>
        /// <returns>list of versions older then current and prior version</returns>
        public IList<VersionedValue<T>> OlderVersions => _olderVersions;

        /// <summary>
        ///     Retrieve a value for the given version or older then then given version.
        ///     <para />
        ///     The implementaton only locks the read lock if an older version the the prior version is requested.
        /// </summary>
        /// <param name="versionAndOlder">the version we are looking for</param>
        /// <returns>
        ///     value for the version or the next older version, ignoring newer versions
        /// </returns>
        public T GetVersion(int versionAndOlder)
        {
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(
                    ".GetVersion Thread " +
                    Thread.CurrentThread.ManagedThreadId +
                    " for '" +
                    Name +
                    "' retrieving version " +
                    versionAndOlder +
                    " or older");
            }

            T resultValue = null;
            var current = _currentAndPriorValue;

            if (current.CurrentVersion.Version <= versionAndOlder) {
                resultValue = current.CurrentVersion.Value;
            }
            else if (current.PriorVersion.Version != -1 &&
                     current.PriorVersion.Version <= versionAndOlder) {
                resultValue = current.PriorVersion.Value;
            }
            else {
                using (_readLock.Acquire()) {
                    current = _currentAndPriorValue;

                    if (current.CurrentVersion.Version <= versionAndOlder) {
                        resultValue = current.CurrentVersion.Value;
                    }
                    else if (current.PriorVersion.Version != -1 &&
                             current.PriorVersion.Version <= versionAndOlder) {
                        resultValue = current.PriorVersion.Value;
                    }
                    else {
                        var found = false;
                        for (var i = _olderVersions.Count - 1; i >= 0; i--) {
                            var entry = _olderVersions[i];
                            if (entry.Version <= versionAndOlder) {
                                resultValue = entry.Value;
                                found = true;
                                break;
                            }
                        }

                        if (!found) {
                            var currentVersion = current.CurrentVersion.Version;
                            var priorVersion = current.PriorVersion.Version;
                            int? oldestVersion = null;
                            if (_olderVersions.Count > 0) {
                                oldestVersion = _olderVersions[0].Version;
                            }

                            var oldestValue = _olderVersions.Count > 0 ? _olderVersions[0].Value : null;

                            var text = "Variables value for version '" +
                                       versionAndOlder +
                                       "' and older could not be found" +
                                       " (currentVersion=" +
                                       currentVersion +
                                       " priorVersion=" +
                                       priorVersion +
                                       " oldestVersion=" +
                                       oldestVersion +
                                       " numOldVersions=" +
                                       _olderVersions.Count +
                                       " oldestValue=" +
                                       oldestValue +
                                       ")";
                            if (_errorWhenNotFound) {
                                throw new IllegalStateException(text);
                            }

                            Log.Warn(text);
                            return current.CurrentVersion.Value;
                        }
                    }
                }
            }

            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(
                    ".getVersion Thread " +
                    Thread.CurrentThread.ManagedThreadId +
                    " for '" +
                    Name +
                    " version " +
                    versionAndOlder +
                    " or older result is " +
                    resultValue);
            }

            return resultValue;
        }

        /// <summary>
        ///     Add a value and version to the list, returning the prior value of the variable.
        /// </summary>
        /// <param name="version">for the value to add</param>
        /// <param name="value">to add</param>
        /// <param name="timestamp">the time associated with the version</param>
        /// <returns>prior value</returns>
        public object AddValue(
            int version,
            T value,
            long timestamp)
        {
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(
                    ".addValue Thread " +
                    Thread.CurrentThread.ManagedThreadId +
                    " for '" +
                    Name +
                    "' adding version " +
                    version +
                    " at value " +
                    value);
            }

            // push to prior if not already used
            if (_currentAndPriorValue.PriorVersion.Version == -1) {
                _currentAndPriorValue = new CurrentValue<T>(
                    new VersionedValue<T>(version, value, timestamp),
                    _currentAndPriorValue.CurrentVersion);
                return _currentAndPriorValue.PriorVersion.Value;
            }

            // add to list
            var priorVersion = _currentAndPriorValue.PriorVersion;
            _olderVersions.Add(priorVersion);

            // check watermarks
            if (_olderVersions.Count >= _highWatermark) {
                var expireBefore = timestamp - _millisecondLifetimeOldVersions;
                while (_olderVersions.Count > 0) {
                    var oldestVersion = _olderVersions[0];
                    if (oldestVersion.Timestamp <= expireBefore) {
                        _olderVersions.RemoveAt(0);
                    }
                    else {
                        break;
                    }
                }
            }

            _currentAndPriorValue = new CurrentValue<T>(
                new VersionedValue<T>(version, value, timestamp),
                _currentAndPriorValue.CurrentVersion);
            return _currentAndPriorValue.PriorVersion.Value;
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("Variable '").Append(Name).Append("' ");
            buffer.Append(" current=").Append(_currentAndPriorValue.CurrentVersion);
            buffer.Append(" prior=").Append(_currentAndPriorValue.CurrentVersion);

            var count = 0;
            foreach (var old in _olderVersions) {
                buffer.Append(" Old(").Append(count).Append(")=").Append(old).Append("\n");
                count++;
            }

            return buffer.ToString();
        }
    }
} // End of namespace