///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.core.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.events.avro;

namespace com.espertech.esper.epl.core
{
    public interface SelectExprProcessorRepresentationFactory
    {
        SelectExprProcessor MakeSelectNoWildcard(
            SelectExprContext selectExprContext,
            EventType resultEventType,
            TableService tableService,
            string statementName,
            string engineURI);

        SelectExprProcessor MakeRecast(
            EventType[] eventTypes,
            SelectExprContext selectExprContext,
            int streamNumber,
            AvroSchemaEventType insertIntoTargetType,
            ExprNode[] exprNodes,
            string statementName,
            string engineURI);

        SelectExprProcessor MakeJoinWildcard(
            string[] streamNames,
            EventType resultEventType,
            EventAdapterService eventAdapterService);
    }
} // end of namespace