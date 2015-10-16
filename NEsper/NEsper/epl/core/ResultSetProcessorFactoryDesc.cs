///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Processor Prototype for result sets for instances that apply the select-clause, group-by-clause and having-clauses as supplied.
    /// </summary>
    public class ResultSetProcessorFactoryDesc
    {
        public ResultSetProcessorFactoryDesc(ResultSetProcessorFactory resultSetProcessorFactory, OrderByProcessorFactory orderByProcessorFactory, AggregationServiceFactoryDesc aggregationServiceFactoryDesc)
        {
            ResultSetProcessorFactory = resultSetProcessorFactory;
            OrderByProcessorFactory = orderByProcessorFactory;
            AggregationServiceFactoryDesc = aggregationServiceFactoryDesc;
        }

        public ResultSetProcessorFactory ResultSetProcessorFactory { get; private set; }

        public OrderByProcessorFactory OrderByProcessorFactory { get; private set; }

        public AggregationServiceFactoryDesc AggregationServiceFactoryDesc { get; private set; }
    }
}
