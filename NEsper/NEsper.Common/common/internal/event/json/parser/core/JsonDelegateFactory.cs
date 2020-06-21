///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client.json.minimaljson;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
	public interface JsonDelegateFactory {
	    JsonDelegateBase Make(JsonHandlerDelegator handler, JsonDelegateBase optionalParent);
	    void Write(JsonWriter writer, object und) ;
	    object NewUnderlying();
	    void SetValue(int num, object value, object und);
	    object GetValue(int num, object und);
	    object Copy(object und);
	}
} // end of namespace
