///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text.Json;

using com.espertech.esper.common.client.json.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.regressionlib.support.json
{
	public class SupportJsonFieldAdapterStringDate : JsonFieldAdapterString<DateTimeEx>
	{
		public DateTimeEx Parse(string value)
		{
			return value == null ? null : DateTimeParsingFunctions.ParseDefaultEx(value);
		}

		public void Write(
			DateTimeEx value,
			Utf8JsonWriter writer)
		{
			if (value == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(value.ToString());
		}
	}
} // end of namespace
