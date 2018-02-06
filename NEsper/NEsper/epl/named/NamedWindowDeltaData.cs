///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;

namespace com.espertech.esper.epl.named
{
/// <summary>
/// A holder for events posted by a named window as an insert and remove stream.
/// </summary>
	public class NamedWindowDeltaData
	{
	    private readonly EventBean[] newData;
	    private readonly EventBean[] oldData;

	    /// <summary>Ctor.</summary>
	    /// <param name="newData">is the insert stream events, or null if none</param>
	    /// <param name="oldData">is the remove stream events, or null if none</param>
	    public NamedWindowDeltaData(EventBean[] newData, EventBean[] oldData)
	    {
	        this.newData = newData;
	        this.oldData = oldData;
	    }

	    /// <summary>Ctor aggregates two deltas into a single delta.</summary>
	    /// <param name="deltaOne">
	    /// is the insert and remove stream events of a first result
	    /// </param>
	    /// <param name="deltaTwo">
	    /// is the insert and remove stream events of a second result
	    /// </param>
	    public NamedWindowDeltaData(NamedWindowDeltaData deltaOne, NamedWindowDeltaData deltaTwo)
	    {
	        this.newData = Aggregate(deltaOne.NewData, deltaTwo.NewData);
	        this.oldData = Aggregate(deltaOne.OldData, deltaTwo.OldData);
	    }

	    /// <summary>Returns the insert stream events.</summary>
	    /// <returns>insert stream</returns>
	    public EventBean[] NewData => newData;

	    /// <summary>Returns the remove stream events.</summary>
	    /// <returns>remove stream</returns>
	    public EventBean[] OldData => oldData;

	    private static EventBean[] Aggregate(EventBean[] arrOne, EventBean[] arrTwo)
	    {
	        if (arrOne == null)
	        {
	            return arrTwo;
	        }
	        if (arrTwo == null)
	        {
	            return arrOne;
	        }
	        EventBean[] arr = new EventBean[arrOne.Length + arrTwo.Length];
	        Array.Copy(arrOne, 0, arr, 0, arrOne.Length);
	        Array.Copy(arrTwo, 0, arr, arrOne.Length, arrTwo.Length);
	        return arr;
	    }
	}
} // End of namespace
