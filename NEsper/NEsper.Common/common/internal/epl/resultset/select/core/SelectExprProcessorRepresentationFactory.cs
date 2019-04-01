///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
	public interface SelectExprProcessorRepresentationFactory {
	    SelectExprProcessorForge MakeSelectNoWildcard(SelectExprForgeContext selectExprForgeContext, ExprForge[] exprForges, EventType resultEventType, TableCompileTimeResolver tableService, string statementName) ;

	    SelectExprProcessorForge MakeRecast(EventType[] eventTypes, SelectExprForgeContext selectExprForgeContext, int streamNumber, AvroSchemaEventType insertIntoTargetType, ExprNode[] exprNodes, string statementName) ;

	    SelectExprProcessorForge MakeJoinWildcard(string[] streamNames, EventType resultEventType);
	}
} // end of namespace