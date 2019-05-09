///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    ///     A wrapper for a thread-local to hold the current version for variables visible
    ///     for a thread, as well as uncommitted values of variables for a thread.
    /// </summary>
    public class VariableVersionThreadLocal
    {
        private readonly IThreadLocal<VariableVersionThreadEntry> _vThreadLocal;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableVersionThreadLocal"/> class.
        /// </summary>
        /// <param name="threadLocalManager">The thread local manager.</param>
        public VariableVersionThreadLocal(IThreadLocalManager threadLocalManager)
        {
            if (threadLocalManager != null) {
                _vThreadLocal = threadLocalManager.Create(CreateEntry);
            }
            else {
                _vThreadLocal = null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableVersionThreadLocal"/> class.
        /// </summary>
        public VariableVersionThreadLocal()
            : this(new DefaultThreadLocalManager(new SlimThreadLocalFactory()))
        {
        }

        /// <summary>
        ///     Returns the version and uncommitted values for the current thread.
        /// </summary>
        /// <returns>entry for current thread</returns>
        public VariableVersionThreadEntry CurrentThread => _vThreadLocal.GetOrCreate();

        /// <summary>
        ///     Creates a new variable version thread entry.
        /// </summary>
        /// <returns></returns>
        private static VariableVersionThreadEntry CreateEntry()
        {
            return new VariableVersionThreadEntry(0, null);
        }
    }
} // End of namespace