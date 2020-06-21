///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue;

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.array
{
    public class JsonDelegateArrayDoublePrimitive : JsonDelegateArrayBase
    {
        public JsonDelegateArrayDoublePrimitive(
            JsonHandlerDelegator baseHandler,
            JsonDelegateBase parent) : base(baseHandler, parent)
        {
        }

        public override void EndOfArrayValue(string name)
        {
            collection.Add(JsonEndValueForgeDouble.JsonToDoubleNonNull(StringValue, name));
        }

        public override object GetResult()
        {
            return collection.Cast<double>().ToArray();
        }
    }
} // end of namespace