///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.util
{
	public class SupportMTUpdateListener : UpdateListener {
	    private readonly IList<EventBean[]> newDataList;
	    private readonly IList<EventBean[]> oldDataList;
	    private EventBean[] lastNewData;
	    private EventBean[] lastOldData;
	    private bool isInvoked;

	    public SupportMTUpdateListener() {
	        newDataList = new List<EventBean[]>();
	        oldDataList = new List<EventBean[]>();
	    }

	    public void Update(
		    object sender,
		    UpdateEventArgs eventArgs)
	    {
		    var oldData = eventArgs.OldEvents;
		    var newData = eventArgs.NewEvents;
		    
		    lock (this) {
			    oldDataList.Add(oldData);
			    newDataList.Add(newData);

			    lastNewData = newData;
			    lastOldData = oldData;

			    isInvoked = true;
		    }
	    }

	    public void Reset() {
		    lock (this) {
			    oldDataList.Clear();
			    newDataList.Clear();
			    lastNewData = null;
			    lastOldData = null;
			    isInvoked = false;
		    }
	    }

	    public EventBean[] LastNewData => lastNewData;

	    public EventBean[] GetAndResetLastNewData()
	    {
		    lock (this) {
			    var lastNew = lastNewData;
			    Reset();
			    return lastNew;
		    }
	    }

	    public EventBean AssertOneGetNewAndReset() {
		    lock (this) {

			    ClassicAssert.IsTrue(isInvoked);

			    ClassicAssert.AreEqual(1, newDataList.Count);
			    ClassicAssert.AreEqual(1, oldDataList.Count);

			    ClassicAssert.AreEqual(1, lastNewData.Length);
			    ClassicAssert.IsNull(lastOldData);

			    var lastNew = lastNewData[0];
			    Reset();
			    return lastNew;
		    }
	    }

	    public EventBean AssertOneGetOldAndReset() {
		    lock (this) {

			    ClassicAssert.IsTrue(isInvoked);

			    ClassicAssert.AreEqual(1, newDataList.Count);
			    ClassicAssert.AreEqual(1, oldDataList.Count);

			    ClassicAssert.AreEqual(1, lastOldData.Length);
			    ClassicAssert.IsNull(lastNewData);

			    var lastNew = lastOldData[0];
			    Reset();
			    return lastNew;
		    }
	    }

	    public EventBean[] LastOldData => lastOldData;

	    public IList<EventBean[]> NewDataList => newDataList;

	    public IList<EventBean[]> NewDataListCopy {
		    get {
			    lock (this) {
				    return new List<EventBean[]>(newDataList);
			    }
		    }
	    }

	    public IList<EventBean[]> OldDataList => oldDataList;

	    public bool IsInvoked() {
	        return isInvoked;
	    }

	    public bool GetAndClearIsInvoked() {
		    lock (this) {
			    var invoked = isInvoked;
			    isInvoked = false;
			    return invoked;
		    }
	    }

	    public EventBean[] NewDataListFlattened {
		    get {
			    lock (this) {
				    return Flatten(newDataList);
			    }
		    }
	    }

	    public EventBean[] OldDataListFlattened {
		    get {
			    lock (this) {
				    return Flatten(oldDataList);
			    }
		    }
	    }

	    private EventBean[] Flatten(IList<EventBean[]> list) {
	        var count = 0;
	        foreach (var events in list) {
	            if (events != null) {
	                count += events.Length;
	            }
	        }

	        var array = new EventBean[count];
	        count = 0;
	        foreach (var events in list) {
	            if (events != null) {
	                for (var i = 0; i < events.Length; i++) {
	                    array[count++] = events[i];
	                }
	            }
	        }
	        return array;
	    }
	}
} // end of namespace
