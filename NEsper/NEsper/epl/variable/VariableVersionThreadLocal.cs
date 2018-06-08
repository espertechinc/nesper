///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.epl.variable
{
    /// <summary>
    /// A wrapper for a thread-local to hold the current version for variables visible
    /// for a thread, as well as uncommitted values of variables for a thread.
    /// </summary>
	public class VariableVersionThreadLocal
	{
        private readonly IThreadLocal<VariableVersionThreadEntry> _vThreadLocal;

        /// <summary>
        /// Creates a new variable version thread entry.
        /// </summary>
        /// <returns></returns>
        private static VariableVersionThreadEntry CreateEntry()
        {
            return new VariableVersionThreadEntry(0, null);
        }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    public VariableVersionThreadLocal(IThreadLocalManager threadLocalManager)
	    {
	        if (threadLocalManager != null) {
	            _vThreadLocal = threadLocalManager.Create<VariableVersionThreadEntry>(CreateEntry);
	        }
	        else {
	            _vThreadLocal = null;
	        }
	    }

	    /// <summary>
        /// Returns the version and uncommitted values for the current thread.
        /// </summary>
        /// <returns>entry for current thread</returns>
	    public VariableVersionThreadEntry CurrentThread
	    {
            get
            {
                VariableVersionThreadEntry entry = _vThreadLocal.GetOrCreate();
                return entry;
            }
	    }
	}
} // End of namespace
