///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public class JsonDelegateJsonGenericArray : JsonDelegateJsonGenericBase
    {
        private readonly ICollection<object> collection = new List<object>();

        public JsonDelegateJsonGenericArray(
            JsonHandlerDelegator baseHandler,
            JsonDelegateBase parent) : base(baseHandler, parent)
        {
        }

        public override object GetResult() => collection.ToArray();

        public override bool EndObjectValue(string name)
        {
            return false;
        }

        public override void EndArrayValue(string name)
        {
            collection.Add(ValueToObject());
        }
    }
} // end of namespace