///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Static factory for implementations of the <seealso cref="FilterService" /> interface.
    /// </summary>
    public class FilterServiceProvider
    {
        /// <summary>
        ///     Creates an implementation of the FilterEvaluationService interface.
        /// </summary>
        /// <param name="filterServiceProfile">config</param>
        /// <param name="allowIsolation">whether isolation is supported</param>
        /// <param name="rwLockManager"></param>
        /// <returns>implementation</returns>
        public static FilterServiceSPI NewService(
            FilterServiceProfile filterServiceProfile,
            bool allowIsolation,
            IReaderWriterLockManager rwLockManager)
        {
            if (filterServiceProfile == FilterServiceProfile.READMOSTLY) {
                return new FilterServiceLockCoarse(rwLockManager, allowIsolation);
            }

            return new FilterServiceLockFine(rwLockManager, allowIsolation);
        }
    }
} // end of namespace