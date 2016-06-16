///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.named
{
	/// <summary>
	/// A spin-locking implementation of a latch for use in guaranteeing delivery between
	/// a delta stream produced by a named window and consumable by another statement.
	/// </summary>
	public class NamedWindowConsumerLatchSpin : NamedWindowConsumerLatch
	{
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	    // The earlier latch is the latch generated before this latch
	    private readonly NamedWindowConsumerLatchFactory _factory;
	    private NamedWindowConsumerLatchSpin _earlier;
	    private Thread _currentThread;

	    private volatile bool _isCompleted;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="earlier">the latch before this latch that this latch should be waiting for</param>
	    public NamedWindowConsumerLatchSpin(NamedWindowDeltaData deltaData, IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> dispatchTo, NamedWindowConsumerLatchFactory factory, NamedWindowConsumerLatchSpin earlier)
            : base(deltaData, dispatchTo)
	    {
	        _factory = factory;
	        _earlier = earlier;
	    }

	    /// <summary>
	    /// Ctor - use for the first and unused latch to indicate completion.
	    /// </summary>
	    public NamedWindowConsumerLatchSpin(NamedWindowConsumerLatchFactory factory)
	        : base(null, null)
	    {
	        _factory = factory;
	        _isCompleted = true;
	        _earlier = null;
	    }

	    /// <summary>
	    /// Returns true if the dispatch completed for this future.
	    /// </summary>
	    /// <value>true for completed, false if not</value>
	    public bool IsCompleted
	    {
	        get { return _isCompleted; }
	    }

	    /// <summary>
	    /// Blocking call that returns only when the earlier latch completed.
	    /// </summary>
	    /// <returns>unit of the latch</returns>
	    public override void Await()
	    {
	        var thread = Thread.CurrentThread;
	        if (_earlier._isCompleted) {
	            _currentThread = thread;
	            return;
	        }

	        if (((NamedWindowConsumerLatch) _earlier).CurrentThread == thread) {
	            _currentThread = thread;
	            return;
	        }

	        long spinStartTime = _factory.TimeSourceService.GetTimeMillis();
	        while(!_earlier._isCompleted) {
	            Thread.Yield();
	            long spinDelta = _factory.TimeSourceService.GetTimeMillis() - spinStartTime;
	            if (spinDelta > _factory.MsecWait) {
	                Log.Info("Spin wait timeout exceeded in named window '{0}' consumer dispatch at {1}ms for {0}, consider disabling named window consumer dispatch latching for better performance", _factory.Name, _factory.MsecWait);
	                break;
	            }
	        }
	    }

	    public override Thread CurrentThread
	    {
	        get { return _currentThread; }
	    }

	    /// <summary>
	    /// Called to indicate that the latch completed and a later latch can start.
	    /// </summary>
	    public override void Done()
	    {
	        _isCompleted = true;
	        _earlier = null;
	    }
	}
} // end of namespace
