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
using com.espertech.esper.common.@internal.epl.index.composite;


namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    public class PollResultIndexingStrategyComposite : PollResultIndexingStrategy
    {
        private int streamNum;
        private string[] optionalKeyedProps;
        private Type[] optKeyCoercedTypes;
        private EventPropertyValueGetter hashGetter;
        private string[] rangeProps;
        private Type[] optRangeCoercedTypes;
        private EventPropertyValueGetter[] rangeGetters;
        private PropertyCompositeEventTableFactory factory;

        public EventTable[] Index(
            IList<EventBean> pollResult,
            bool isActiveCache,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (!isActiveCache) {
                return new EventTable[] {
                    new UnindexedEventTableList(pollResult, streamNum)
                };
            }

            var tables = factory.MakeEventTables(exprEvaluatorContext, null);
            foreach (var table in tables) {
                table.Add(pollResult.ToArray(), exprEvaluatorContext);
            }

            return tables;
        }

        public void Init()
        {
            factory = new PropertyCompositeEventTableFactory(
                streamNum,
                optionalKeyedProps,
                optKeyCoercedTypes,
                hashGetter,
                null,
                rangeProps,
                optRangeCoercedTypes,
                rangeGetters);
        }

        public int StreamNum {
            set => streamNum = value;
        }

        public string[] OptionalKeyedProps {
            set => optionalKeyedProps = value;
        }

        public Type[] OptKeyCoercedTypes {
            set => optKeyCoercedTypes = value;
        }

        public EventPropertyValueGetter HashGetter {
            set => hashGetter = value;
        }

        public string[] RangeProps {
            set => rangeProps = value;
        }

        public Type[] OptRangeCoercedTypes {
            set => optRangeCoercedTypes = value;
        }

        public EventPropertyValueGetter[] RangeGetters {
            set => rangeGetters = value;
        }

        public PropertyCompositeEventTableFactory Factory {
            set => factory = value;
        }
    }
} // end of namespace