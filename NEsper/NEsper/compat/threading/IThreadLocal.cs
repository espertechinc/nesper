///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.compat.threading
{
    /// <summary>
    /// IThreadLocal provides the engine with a way to store information that
    /// is local to the instance and a the thread.  While the CLR provides the
    /// ThreadStatic attribute, it can only be applied to static variables;
    /// some usage patterns in esper (such as statement-specific thread-specific
    /// processing data) require that data be associated by instance and thread.
    /// The CLR provides a solution to this known as LocalDataStoreSlot.  It
    /// has been documented that this method is slower than its ThreadStatic
    /// counterpart, but it allows for instance-based allocation.
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public interface IThreadLocal<T> : IDisposable
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        T Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the data or creates it if not found.
        /// </summary>
        /// <returns></returns>
        T GetOrCreate();
    }
}
