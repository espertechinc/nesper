///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.named
{
	/// <summary>
	/// A suspend-and-notify implementation of a latch for use in guaranteeing delivery between
	/// a named window delta result and consumable by another statement.
	/// </summary>
	public class NamedWindowConsumerLatchWait : NamedWindowConsumerLatch
	{
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	    // The earlier latch is the latch generated before this latch
	    private readonly NamedWindowConsumerLatchFactory _factory;
	    private NamedWindowConsumerLatchWait _earlier;

	    // The later latch is the latch generated after this latch
	    private NamedWindowConsumerLatchWait _later;
	    private volatile bool _isCompleted;
	    private Thread _currentThread;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="earlier">the latch before this latch that this latch should be waiting for</param>
	    public NamedWindowConsumerLatchWait(NamedWindowDeltaData deltaData, IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> dispatchTo, NamedWindowConsumerLatchFactory factory, NamedWindowConsumerLatchWait earlier)
            : base(deltaData, dispatchTo)
	    {
	        _factory = factory;
	        _earlier = earlier;
	    }

	    /// <summary>
	    /// Ctor - use for the first and unused latch to indicate completion.
	    /// </summary>
	    public NamedWindowConsumerLatchWait(NamedWindowConsumerLatchFactory factory)
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
	    /// Hand a later latch to use for indicating completion via notify.
	    /// </summary>
	    /// <value>is the later latch</value>
	    public NamedWindowConsumerLatchWait Later
	    {
	        set { _later = value; }
	    }

	    /// <summary>
	    /// Blcking call that returns only when the earlier latch completed.
	    /// </summary>
	    /// <returns>payload of the latch</returns>
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

	        lock(this)
	        {
	            if (!_earlier._isCompleted)
	            {
	                try
	                {
	                    Monitor.Wait(this, (int) _factory.MsecWait);
	                }
	                catch (ThreadAbortException e)
	                {
                        Log.Error("thread aborted", e);
                    }
	                catch (ThreadInterruptedException e)
	                {
	                    Log.Error("thread interrupted", e);
	                }
	            }
	        }

	        if (!_earlier._isCompleted) {
	            Log.Info("Wait timeout exceeded for named window '" + "' consumer dispatch with notify");
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
	        if (_later != null)
	        {
	            lock(_later)
	            {
	                Monitor.Pulse(_later);
	            }
	        }
	        _earlier = null;
	        _later = null;
	    }
	}
} // end of namespace
