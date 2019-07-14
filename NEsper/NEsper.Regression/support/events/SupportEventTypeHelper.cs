///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.context.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.@event
{
    public class SupportEventTypeHelper
    {
        public static EventTypeIdPair GetTypeIdForName(
            StatementContext statementContext,
            string eventTypeName)
        {
            var type = statementContext.EventTypeRepositoryPreconfigured.GetTypeByName(eventTypeName);
            if (type == null) {
                Assert.Fail("Type by name '" + eventTypeName + "' not found as a public type");
            }

            return type.Metadata.EventTypeIdPair;
        }

        public static EventType GetEventTypeForTypeId(
            StatementContext statementContext,
            EventTypeIdPair key)
        {
            return statementContext.EventTypeRepositoryPreconfigured.GetTypeById(key.PublicId);
        }
    }
} // end of namespace