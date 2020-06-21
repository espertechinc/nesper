///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client.json.minimaljson;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.json.util
{
	/// <summary>
	/// All JSON underlying event objects implement this interface.
	/// <para />In general, byte code does not use the Map methods and instead
	/// uses the implementation class fields directly.
	/// <para />This is a read-only implementation of the Map interface.
	/// <para />All predefined properties as well as all dynamic properties become available through the Map interface.
	/// </summary>
	public interface JsonEventObject : IDictionary<string, object> {
	    /// <summary>
	    /// Write JSON to the provided writer and using the provided configuration.
	    /// </summary>
	    /// <param name="writer">writer</param>
	    /// <param name="config">JSON writer settings</param>
	    /// <throws>IOException when an IO exception occurs</throws>
	    void WriteTo(Writer writer, WriterConfig config) ;

	    /// <summary>
	    /// Returns the JSON string given a writer configuration
	    /// </summary>
	    /// <param name="config">JSON writer settings</param>
	    /// <returns>JSON</returns>
	    string ToString(WriterConfig config);
	}
} // end of namespace
