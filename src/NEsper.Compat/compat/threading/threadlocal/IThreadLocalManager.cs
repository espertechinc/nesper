///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.threading.threadlocal
{
    public interface IThreadLocalManager
    {
        /// <summary>
        /// Creates a thread local instance.
        /// </summary>
        /// <returns></returns>
        IThreadLocal<T> Create<T>(Func<T> factoryDelegate)
            where T : class;

        /// <summary>
        /// Creates the default thread local.
        /// </summary>
        /// <returns></returns>
        IThreadLocal<T> CreateDefaultThreadLocal<T>(Func<T> factoryDelegate)
            where T : class;
    }
}
