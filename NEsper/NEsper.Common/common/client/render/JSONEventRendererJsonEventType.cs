///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

//using com.espertech.esper.common.client.json.minimaljson;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.@internal.@event.json.core;

namespace com.espertech.esper.common.client.render
{
	public class JSONEventRendererJsonEventType : JSONEventRenderer {
	    public readonly static JSONEventRendererJsonEventType INSTANCE = new JSONEventRendererJsonEventType();

	    private JSONEventRendererJsonEventType() {
	    }

	    public string Render(
		    string title,
		    EventBean theEvent)
	    {
		    var @event = (JsonEventObject) theEvent.Underlying;
		    var writer = new StringWriter();
		    writer.Write("{\"");
		    writer.Write(title);
		    writer.Write("\":");
		    try {
			    @event.WriteTo(writer, WriterConfig.MINIMAL);
		    }
		    catch (IOException e) {
			    throw new EPException("Failed to write json: " + e.Message, e);
		    }

		    writer.Write("}");
		    return writer.ToString();
	    }

	    public string Render(EventBean theEvent) {
	        var underlying = theEvent.Underlying;
	        if (underlying is JsonEventObject) {
	            var @event = (JsonEventObject) underlying;
	            return @event.ToString(WriterConfig.MINIMAL);
	        }
	        var eventType = (JsonEventType) theEvent.EventType;
	        var writer = new StringWriter();
	        try {
	            WritingBuffer buffer = new WritingBuffer(writer, 128);
	            eventType.DelegateFactory.Write(WriterConfig.MINIMAL.CreateWriter(buffer), underlying);
	            buffer.Flush();
	        } catch (IOException exception) {
	            // StringWriter does not throw IOExceptions
	            throw new EPRuntimeException(exception);
	        }
	        return writer.ToString();
	    }
	}
} // end of namespace
