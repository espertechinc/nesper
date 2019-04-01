///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.SelectExprRep
{
    /// <summary>
    ///     Processor for select-clause expressions that handles wildcards. Computes results based on matching events.
    /// </summary>
    public class SelectExprJoinWildcardProcessorAvro : SelectExprProcessor
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventType _resultEventType;
        private readonly RecordSchema _schema;

        public SelectExprJoinWildcardProcessorAvro(EventType resultEventType, EventAdapterService eventAdapterService)
        {
            _resultEventType = resultEventType;
            _schema = ((AvroEventType) resultEventType).SchemaAvro;
            _eventAdapterService = eventAdapterService;
        }

        public EventBean Process(
            EventBean[] eventsPerStream,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var fields = _schema.Fields;
            var @event = new GenericRecord(_schema);
            for (int i = 0; i < eventsPerStream.Length; i++)
            {
                EventBean streamEvent = eventsPerStream[i];
                if (streamEvent != null)
                {
                    var record = (GenericRecord) streamEvent.Underlying;
                    @event.Put(fields[i], record);
                }
            }
            return _eventAdapterService.AdapterForTypedAvro(@event, _resultEventType);
        }

        public EventType ResultEventType
        {
            get { return _resultEventType; }
        }
    }
} // end of namespace