///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    ///     Reads and writes variable values.
    ///     <para />
    ///     Works closely with <seealso cref="VariableManagementService" /> in determining the version to read.
    /// </summary>
    public class VariableReader
    {
        private readonly VariableVersionThreadLocal versionThreadLocal;
        private volatile VersionedValueList<object> versionsHigh;
        private volatile VersionedValueList<object> versionsLow;

        public VariableReader(
            Variable variable,
            VariableVersionThreadLocal versionThreadLocal,
            VersionedValueList<object> versionsLow)
        {
            Variable = variable;
            this.versionThreadLocal = versionThreadLocal;
            this.versionsLow = versionsLow;
            versionsHigh = null;
        }

        /// <summary>
        ///     For roll-over (overflow) in version numbers, sets a new collection of versioned-values for the variable
        ///     to use when requests over the version rollover boundary are made.
        /// </summary>
        /// <value>the list of versions for roll-over</value>
        public VersionedValueList<object> VersionsHigh {
            set => versionsHigh = value;
        }

        /// <summary>
        ///     Returns the value of a variable.
        ///     <para />
        ///     Considers the version set via thread-local for the thread's atomic read of variable values.
        /// </summary>
        /// <value>value of variable at the version applicable for the thead</value>
        public object Value {
            get {
                var entry = versionThreadLocal.CurrentThread;
                if (entry.Uncommitted != null) {
                    // Check existance as null values are allowed
                    if (entry.Uncommitted.ContainsKey(Variable.VariableNumber)) {
                        return entry.Uncommitted.Get(Variable.VariableNumber).Second;
                    }
                }

                var myVersion = entry.Version;
                var versions = versionsLow;
                if (myVersion >= VariableManagementServiceImpl.ROLLOVER_READER_BOUNDARY) {
                    if (versionsHigh != null) {
                        versions = versionsHigh;
                    }
                }

                return versions.GetVersion(myVersion);
            }
        }

        public VariableMetaData MetaData => Variable.MetaData;

        /// <summary>
        ///     Sets a new list of versioned-values to inquire against, for use when version numbers roll-over.
        /// </summary>
        /// <value>the list of versions for read</value>
        public VersionedValueList<object> VersionsLow {
            get => versionsLow;
            set => versionsLow = value;
        }

        public Variable Variable { get; }
    }
} // end of namespace