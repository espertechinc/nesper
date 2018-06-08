///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.filter
{
	/// <summary>
	/// Static factory for implementations of the <see cref="FilterService"/> interface.
	/// </summary>
	public sealed class FilterServiceProvider
	{
	    public static FilterServiceSPI NewService(
            IContainer container,
	        ConfigurationEngineDefaults.FilterServiceProfile filterServiceProfile,
	        bool allowIsolation)
	    {
	        return NewService(
	            container.LockManager(),
	            container.RWLockManager(),
	            filterServiceProfile,
	            allowIsolation);
	    }

        /// <summary> Creates an implementation of the FilterEvaluationService interface.</summary>
        /// <returns> implementation
        /// </returns>
        public static FilterServiceSPI NewService(
            ILockManager lockManager,
            IReaderWriterLockManager rwLockManager,
		    ConfigurationEngineDefaults.FilterServiceProfile filterServiceProfile, 
		    bool allowIsolation)
        {
            if (filterServiceProfile == ConfigurationEngineDefaults.FilterServiceProfile.READMOSTLY)
            {
                return new FilterServiceLockCoarse(lockManager, rwLockManager, allowIsolation);
            }
            else
            {
                return new FilterServiceLockFine(lockManager, rwLockManager, allowIsolation);
            }
        }
	}
}
