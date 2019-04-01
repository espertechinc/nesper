///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.filter
{
    public interface FilterServiceGranularLockFactory
    {
        IReaderWriterLock ObtainNew();
    }

    public class ProxyFilterServiceGranularLockFactory : FilterServiceGranularLockFactory
    {
        public Func<IReaderWriterLock> ProcObtainNew { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyFilterServiceGranularLockFactory"/> class.
        /// </summary>
        public ProxyFilterServiceGranularLockFactory()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyFilterServiceGranularLockFactory"/> class.
        /// </summary>
        /// <param name="procObtainNew">The proc obtain new.</param>
        public ProxyFilterServiceGranularLockFactory(Func<IReaderWriterLock> procObtainNew)
        {
            ProcObtainNew = procObtainNew;
        }

        /// <summary>
        /// Obtains the new.
        /// </summary>
        /// <returns></returns>
        public IReaderWriterLock ObtainNew()
        {
            return ProcObtainNew.Invoke();
        }
    }
}
