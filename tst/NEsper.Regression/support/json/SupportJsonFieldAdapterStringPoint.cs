///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Drawing;
using System.Text.Json;

using com.espertech.esper.common.client.json.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.regressionlib.support.json
{
	public class SupportJsonFieldAdapterStringPoint : JsonFieldAdapterString<Point?> {
	    public Point? Parse(string value) {
	        if (value == null) {
	            return null;
	        }
	        var split = value.SplitCsv();
	        return new Point(
		        int.Parse(split[0]),
		        int.Parse(split[1]));
	    }

	    public void Write(
		    Point? value,
		    Utf8JsonWriter writer)
	    {
	        if (value == null) {
	            writer.WriteNullValue();
	            return;
	        }

	        writer.WriteStringValue(value.Value.X + "," + value.Value.Y);
	    }
	}
} // end of namespace
