///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class ExprDotEvalUnpackBeanTable : ExprDotEval
    {
        private readonly EPType _returnType;
        private readonly TableMetadata _tableMetadata;

        public ExprDotEvalUnpackBeanTable(EventType lambdaType, TableMetadata tableMetadata)
        {
            _tableMetadata = tableMetadata;
            _returnType = EPTypeHelper.SingleValue(tableMetadata.PublicEventType.UnderlyingType);
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target == null)
            {
                return null;
            }
            EventBean theEvent = (EventBean) target;
            if (theEvent == null)
            {
                return null;
            }
            return _tableMetadata.EventToPublic.ConvertToUnd(theEvent, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public EPType TypeInfo
        {
            get { return _returnType; }
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitUnderlyingEvent();
        }
    }
}
