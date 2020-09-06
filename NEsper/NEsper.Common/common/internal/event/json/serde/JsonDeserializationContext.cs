///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.@event.json.parser.core;

namespace com.espertech.esper.common.@internal.@event.json.serde
{
	/// <summary>
	/// This object provides context during deserialization.
	/// </summary>
	public interface JsonDeserializationContext
	{
		/// <summary>
		/// Gets a deserializer for a given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		IJsonDeserializer GetDeserializer(Type type);
	}
} // end of namespace
