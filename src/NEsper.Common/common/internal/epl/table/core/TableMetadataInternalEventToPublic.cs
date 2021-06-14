///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public interface TableMetadataInternalEventToPublic
    {
        EventBean Convert(
            EventBean @event,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        object[] ConvertToUnd(
            EventBean @event,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);
    }

    public class ProxyTableMetadataInternalEventToPublic : TableMetadataInternalEventToPublic
    {
        public delegate EventBean ConvertFunc(
            EventBean @event,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);
        public delegate object[] ConvertToUndFunc(
            EventBean @event,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        public ProxyTableMetadataInternalEventToPublic()
        {
        }

        public ProxyTableMetadataInternalEventToPublic(ConvertFunc procConvert,
            ConvertToUndFunc procConvertToUnd)
        {
            ProcConvert = procConvert;
            ProcConvertToUnd = procConvertToUnd;
        }

        public ConvertFunc ProcConvert { get; set; }
        public EventBean Convert(
            EventBean @event,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return ProcConvert?.Invoke(@event, eventsPerStream, isNewData, context);
        }

        public ConvertToUndFunc ProcConvertToUnd { get; set; }
        public object[] ConvertToUnd(
            EventBean @event,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return ProcConvertToUnd?.Invoke(@event, eventsPerStream, isNewData, context);
        }
    }
} // end of namespace