///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern.guard;
using com.espertech.esper.pattern.observer;

namespace com.espertech.esper.pattern
{
	/// <summary>
	/// Factory service for resolving pattern objects such as guards and observers.
	/// </summary>
	public interface PatternObjectResolutionService
	{
	    /// <summary>
	    /// Creates an observer factory considering configured plugged-in resources.
	    /// </summary>
	    /// <param name="spec">is the namespace, name and parameters for the observer</param>
	    /// <returns>observer factory</returns>
	    /// <throws>PatternObjectException if the observer cannot be resolved</throws>
	    ObserverFactory Create(PatternObserverSpec spec);

	    /// <summary>
	    /// Creates a guard factory considering configured plugged-in resources.
	    /// </summary>
	    /// <param name="spec">is the namespace, name and parameters for the guard</param>
	    /// <returns>guard factory</returns>
	    /// <throws>PatternObjectException if the guard cannot be resolved</throws>
	   GuardFactory Create(PatternGuardSpec spec);
	}
}
