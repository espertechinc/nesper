///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Text;
using System.Text.Json;

using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.@internal.@event.json.core;

namespace com.espertech.esper.common.client.render
{
	public class JSONEventRendererJsonEventType : JSONEventRenderer
	{
		public static readonly JSONEventRendererJsonEventType INSTANCE = new JSONEventRendererJsonEventType();

		private JSONEventRendererJsonEventType()
		{
		}

		public string Render(
			string title,
			EventBean theEvent)
		{
			var @event = (JsonEventObject) theEvent.Underlying;

			using var stream = new MemoryStream();
			using var writer = new Utf8JsonWriter(stream);

			writer.WriteStartObject();
			writer.WritePropertyName(title);

			try {
				@event.WriteTo(writer);
			}
			catch (IOException e) {
				throw new EPException("Failed to write json: " + e.Message, e);
			}

			writer.WriteEndObject();
			writer.Flush();

			return Encoding.UTF8.GetString(stream.ToArray());
		}

		public string Render(EventBean theEvent)
		{
			var underlying = theEvent.Underlying;
			if (underlying is JsonEventObject) {
				var @event = (JsonEventObject) underlying;
				return @event.ToString(default(JsonWriterOptions));
			}

			var eventType = (JsonEventType) theEvent.EventType;

			using var stream = new MemoryStream();
			using var writer = new Utf8JsonWriter(stream);

			eventType.DelegateFactory.Write(writer, underlying);
			writer.Flush();

			return Encoding.UTF8.GetString(stream.ToArray());
		}
	}
} // end of namespace
