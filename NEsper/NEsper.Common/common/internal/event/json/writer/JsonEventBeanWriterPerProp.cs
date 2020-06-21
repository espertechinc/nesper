///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.@event.json.writer
{
	/// <summary>
	/// Writer method for writing to Json-type events.
	/// </summary>
	public class JsonEventBeanWriterPerProp : EventBeanWriter {
	    private readonly JsonEventBeanPropertyWriter[] writers;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="writers">names of properties to write</param>
	    public JsonEventBeanWriterPerProp(JsonEventBeanPropertyWriter[] writers) {
	        this.writers = writers;
	    }

	    /// <summary>
	    /// Write values to an event.
	    /// </summary>
	    /// <param name="values">to write</param>
	    /// <param name="theEvent">to write to</param>
	    public void Write(object[] values, EventBean theEvent) {
	        object @event = theEvent.Underlying;
	        for (int i = 0; i < writers.Length; i++) {
	            writers[i].Write(values[i], @event);
	        }
	    }
	}
} // end of namespace
