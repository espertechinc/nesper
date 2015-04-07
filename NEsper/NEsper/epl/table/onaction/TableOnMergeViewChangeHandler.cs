///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.onaction
{
    public class TableOnMergeViewChangeHandler
    {
        private readonly TableMetadata _tableMetadata;
        private OneEventCollection _coll;

        public TableOnMergeViewChangeHandler(TableMetadata tableMetadata)
        {
            _tableMetadata = tableMetadata;
        }

        public EventBean[] Events
        {
            get
            {
                if (_coll == null)
                {
                    return null;
                }
                return _coll.ToArray();
            }
        }

        public void Add(EventBean theEvent, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            if (_coll == null)
            {
                _coll = new OneEventCollection();
            }
            if (theEvent is NaturalEventBean)
            {
                theEvent = ((NaturalEventBean) theEvent).OptionalSynthetic;
            }
            _coll.Add(_tableMetadata.EventToPublic.Convert(theEvent, eventsPerStream, isNewData, context));
        }
    }
}