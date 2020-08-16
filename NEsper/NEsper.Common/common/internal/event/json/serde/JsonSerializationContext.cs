///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text.Json;

using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.serializers;

using JsonSerializer = com.espertech.esper.common.@internal.@event.json.serializers.JsonSerializer;

namespace com.espertech.esper.common.@internal.@event.json.serde
{
	public interface JsonSerializationContext
	{
		JsonSerializer Serializer { get; }
		
		JsonDeserializer<object> Deserializer { get; }

		object NewUnderlying();

		void SetValue(
			string name,
			object value,
			object und);

		object GetValue(
			string name,
			object und);

		object Copy(object und);
	}
} // end of namespace
