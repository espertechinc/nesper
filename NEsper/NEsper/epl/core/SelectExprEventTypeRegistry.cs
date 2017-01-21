///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core
{
    /// <summary>Registry for event types creates as part of the select expression analysis. </summary>
    public class SelectExprEventTypeRegistry
    {
        private readonly String statementName;
        private readonly StatementEventTypeRef statementEventTypeRef;
    
        public SelectExprEventTypeRegistry(String statementName, StatementEventTypeRef statementEventTypeRef) {
            this.statementName = statementName;
            this.statementEventTypeRef = statementEventTypeRef;
        }
    
        /// <summary>Adds an event type. </summary>
        /// <param name="eventType">to add</param>
        public void Add(EventType eventType)
        {
            if (!(eventType is EventTypeSPI))
            {
                return;
            }
            statementEventTypeRef.AddReferences(statementName, new String[]{((EventTypeSPI) eventType).Metadata.PrimaryName});
        }
    }
}
