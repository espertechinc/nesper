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
    public class DefaultThreadLocalManager : IThreadLocalManager
    {
        public IThreadLocalFactory DefaultThreadLocalFactory { get; set; }

        /// <summary>
        /// Initializes the <see cref="DefaultThreadLocalManager"/> class.
        /// </summary>
        public DefaultThreadLocalManager(IThreadLocalFactory threadLocalFactory)
        {
            DefaultThreadLocalFactory = threadLocalFactory;
        }

        /// <summary>
        /// Creates a thread local instance.
        /// </summary>
        /// <returns></returns>
        public IThreadLocal<T> Create<T>(Func<T> factoryDelegate)
            where T : class
        {
            return CreateDefaultThreadLocal(factoryDelegate);
        }

        /// <summary>
        /// Creates the default thread local.
        /// </summary>
        /// <returns></returns>
        public IThreadLocal<T> CreateDefaultThreadLocal<T>(Func<T> factoryDelegate)
            where T : class
        {
            var localFactory = DefaultThreadLocalFactory;
            if (localFactory == null) {
                throw new ApplicationException("default thread local factory is not set");
            }

            return localFactory.CreateThreadLocal(factoryDelegate);
        }
    }
}
