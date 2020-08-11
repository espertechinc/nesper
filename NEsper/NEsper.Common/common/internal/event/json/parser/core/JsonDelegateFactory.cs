///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text.Json;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
	public interface JsonDelegateFactory
	{
		JsonDeserializerBase Make(JsonDeserializerBase optionalParent);

		void Write(
			Utf8JsonWriter writer,
			object und);

		object NewUnderlying();

		void SetValue(
			int num,
			object value,
			object und);

		object GetValue(
			int num,
			object und);

		object Copy(object und);
	}
} // end of namespace
