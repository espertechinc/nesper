///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.sorted;

namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    public class PollResultIndexingStrategySorted : PollResultIndexingStrategy
    {
        private PropertySortedEventTableFactory factory;
        private string propertyName;
        private int streamNum;
        private EventPropertyValueGetter valueGetter;
        private Type valueType;

        public int StreamNum {
            set => streamNum = value;
        }

        public string PropertyName {
            set => propertyName = value;
        }

        public EventPropertyValueGetter ValueGetter {
            set => valueGetter = value;
        }

        public Type ValueType {
            set => valueType = value;
        }

        public EventTable[] Index(
            IList<EventBean> pollResult,
            bool isActiveCache,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (!isActiveCache) {
                return new EventTable[] { new UnindexedEventTableList(pollResult, streamNum) };
            }

            var tables = factory.MakeEventTables(exprEvaluatorContext, null);
            foreach (var table in tables) {
                table.Add(pollResult.ToArray(), exprEvaluatorContext);
            }

            return tables;
        }

        public void Init()
        {
            factory = new PropertySortedEventTableFactory(streamNum, propertyName, valueGetter, valueType);
        }
    }
} // end of namespace