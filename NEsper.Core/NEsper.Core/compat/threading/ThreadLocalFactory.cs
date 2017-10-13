///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.compat.threading
{
    /// <summary>
    /// Creator and manufacturer of thread local objects.
    /// </summary>
    public interface ThreadLocalFactory
    {
        /// <summary>
        /// Create a thread local object of the specified type param.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        IThreadLocal<T> CreateThreadLocal<T>(FactoryDelegate<T> factory)
            where T : class;
    }
}
