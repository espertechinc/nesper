///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.timer;

namespace com.espertech.esper.epl.named
{
	/// <summary>
	/// Class to hold a current latch per named window.
	/// </summary>
	public class NamedWindowConsumerLatchFactory
	{
	    private readonly bool _useSpin;
	    private readonly bool _enabled;

	    private NamedWindowConsumerLatchSpin _currentLatchSpin;
	    private NamedWindowConsumerLatchWait _currentLatchWait;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="name">the factory name</param>
	    /// <param name="msecWait">the number of milliseconds latches will await maximually</param>
	    /// <param name="locking">the blocking strategy to employ</param>
	    /// <param name="timeSourceService">time source provider</param>
	    public NamedWindowConsumerLatchFactory(string name, bool enabled, long msecWait, ConfigurationEngineDefaults.Threading.Locking locking, TimeSourceService timeSourceService)
	    {
	        Name = name;
	        _enabled = enabled;
	        MsecWait = msecWait;
	        TimeSourceService = timeSourceService;

	        _useSpin = enabled && (locking == ConfigurationEngineDefaults.Threading.Locking.SPIN);

	        // construct a completed latch as an initial root latch
	        if (_useSpin) {
	            _currentLatchSpin = new NamedWindowConsumerLatchSpin(this);
	        }
	        else if (enabled) {
	            _currentLatchWait = new NamedWindowConsumerLatchWait(this);
	        }
	    }

	    /// <summary>
	    /// Returns a new latch.
	    /// <para />Need not be synchronized as there is one per statement and execution is during statement lock.
	    /// </summary>
	    /// <returns>latch</returns>
	    public NamedWindowConsumerLatch NewLatch(NamedWindowDeltaData delta, IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> consumers)
	    {
	        if (_useSpin) {
	            var nextLatch = new NamedWindowConsumerLatchSpin(delta, consumers, this, _currentLatchSpin);
	            _currentLatchSpin = nextLatch;
	            return nextLatch;
	        }
	        else {
	            if (_enabled) {
	                var nextLatch = new NamedWindowConsumerLatchWait(delta, consumers, this, _currentLatchWait);
	                _currentLatchWait.Later = nextLatch;
	                _currentLatchWait = nextLatch;
	                return nextLatch;
	            }
	            return new NamedWindowConsumerLatchNone(delta, consumers);
	        }
	    }

	    public TimeSourceService TimeSourceService { get; private set; }

	    public string Name { get; private set; }

	    public long MsecWait { get; private set; }
	}
} // end of namespace
