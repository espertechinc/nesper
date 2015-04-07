///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.filter
{
	/// <summary>
	/// Static factory for implementations of the <see cref="FilterService"/> interface.
	/// </summary>
	public sealed class FilterServiceProvider
	{
		/// <summary> Creates an implementation of the FilterEvaluationService interface.</summary>
		/// <returns> implementation
		/// </returns>
        public static FilterServiceSPI NewService(ConfigurationEngineDefaults.FilterServiceProfile filterServiceProfile)
        {
            if (filterServiceProfile == ConfigurationEngineDefaults.FilterServiceProfile.READMOSTLY)
            {
                return new FilterServiceLockCoarse();
            }
            else
            {
                return new FilterServiceLockFine();
            }
        }
	}
}