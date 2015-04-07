///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.pattern.observer
{
	/// <summary>
	/// Abstract class for applications to extend to implement a pattern observer.
	/// </summary>
	public abstract class EventObserverSupport : EventObserver
	{
	    public abstract void StartObserve();
	    public abstract void StopObserve();
	    public abstract void Accept(EventObserverVisitor visitor);
	    public abstract MatchedEventMap BeginState { get; }
	}
} // End of namespace
