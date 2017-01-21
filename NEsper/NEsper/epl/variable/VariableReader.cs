///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.variable
{
    /// <summary>
    /// Reads and writes variable values.
    /// <para />Works closely with <seealso cref="VariableService" /> in determining the version to read.
    /// </summary>
    public class VariableReader
    {
        private readonly VariableMetaData _variableMetaData;
        private readonly VariableVersionThreadLocal _versionThreadLocal;
        private volatile VersionedValueList<Object> _versionsHigh;
        private volatile VersionedValueList<Object> _versionsLow;
    
        public VariableReader(VariableMetaData variableMetaData, VariableVersionThreadLocal versionThreadLocal, VersionedValueList<Object> versionsLow)
        {
            _variableMetaData = variableMetaData;
            _versionThreadLocal = versionThreadLocal;
            _versionsLow = versionsLow;
            _versionsHigh = null;
        }

        /// <summary>
        /// For roll-over (overflow) in version numbers, sets a new collection of versioned-values for the variable
        /// to use when requests over the version rollover boundary are made.
        /// </summary>
        /// <value>the list of versions for roll-over</value>
        public VersionedValueList<object> VersionsHigh
        {
            set { _versionsHigh = value; }
            get { return _versionsHigh; }
        }

        /// <summary>
        /// Returns the value of a variable.
        /// <para />Considers the version set via thread-local for the thread's atomic read of variable values.
        /// </summary>
        /// <value>value of variable at the version applicable for the thead</value>
        public object Value
        {
            get
            {
                var entry = _versionThreadLocal.CurrentThread;
                if (entry.Uncommitted != null)
                {
                    // Check existance as null values are allowed
                    if (entry.Uncommitted.ContainsKey(_variableMetaData.VariableNumber))
                    {
                        return entry.Uncommitted.Get(_variableMetaData.VariableNumber).Second;
                    }
                }

                var myVersion = entry.Version;
                var versions = _versionsLow;
                if (myVersion >= VariableServiceImpl.ROLLOVER_READER_BOUNDARY)
                {
                    if (_versionsHigh != null)
                    {
                        versions = _versionsHigh;
                    }
                }
                return versions.GetVersion(myVersion);
            }
        }

        public VariableMetaData VariableMetaData
        {
            get { return _variableMetaData; }
        }

        /// <summary>
        /// Sets a new list of versioned-values to inquire against, for use when version numbers roll-over.
        /// </summary>
        /// <value>the list of versions for read</value>
        public VersionedValueList<object> VersionsLow
        {
            get { return _versionsLow; }
            set { _versionsLow = value; }
        }
    }
}
