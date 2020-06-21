///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public class JsonDelegateJsonGenericObject : JsonDelegateJsonGenericBase
    {
        private readonly IDictionary<string, object> jsonObject = new LinkedHashMap<string, object>();

        public JsonDelegateJsonGenericObject(
            JsonHandlerDelegator baseHandler,
            JsonDelegateBase parent) : base(baseHandler, parent)
        {
        }

        public override object GetResult() => jsonObject;

        public override bool EndObjectValue(string name)
        {
            AddGeneralJson(jsonObject, name);
            return true;
        }
    }
} // end of namespace