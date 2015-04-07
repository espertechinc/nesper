///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.pattern;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    public class EPStatementStartMethodHelperUtil
    {
        public static Pair<ResultSetProcessor, AggregationService> StartResultSetAndAggregation(ResultSetProcessorFactoryDesc resultSetProcessorPrototype, AgentInstanceContext agentInstanceContext) {
            AggregationService aggregationService = null;
            if (resultSetProcessorPrototype.AggregationServiceFactoryDesc != null) {
                aggregationService = resultSetProcessorPrototype.AggregationServiceFactoryDesc.AggregationServiceFactory.MakeService(agentInstanceContext, agentInstanceContext.StatementContext.MethodResolutionService);
            }
    
            OrderByProcessor orderByProcessor = null;
            if (resultSetProcessorPrototype.OrderByProcessorFactory != null) {
                orderByProcessor = resultSetProcessorPrototype.OrderByProcessorFactory.Instantiate(aggregationService, agentInstanceContext);
            }
    
            ResultSetProcessor resultSetProcessor = resultSetProcessorPrototype.ResultSetProcessorFactory.Instantiate(orderByProcessor, aggregationService, agentInstanceContext);
    
            return new Pair<ResultSetProcessor, AggregationService>(resultSetProcessor, aggregationService);
        }
    
        /// <summary>
        /// Returns a stream name assigned for each stream, generated if none was supplied.
        /// </summary>
        /// <param name="streams">stream specifications</param>
        /// <returns>array of stream names</returns>
        
        internal static String[] DetermineStreamNames(StreamSpecCompiled[] streams)
        {
            String[] streamNames = new String[streams.Length];
            for (int i = 0; i < streams.Length; i++)
            {
                // Assign a stream name for joins, if not supplied
                streamNames[i] = streams[i].OptionalStreamName;
                if (streamNames[i] == null)
                {
                    streamNames[i] = "stream_" + i;
                }
            }
            return streamNames;
        }

        internal static bool[] GetHasIStreamOnly(bool[] isNamedWindow, ViewFactoryChain[] unmaterializedViewChain)
        {
            bool[] result = new bool[unmaterializedViewChain.Length];
            for (int i = 0; i < unmaterializedViewChain.Length; i++) {
                if (isNamedWindow[i]) {
                    continue;
                }
                result[i] = unmaterializedViewChain[i].DataWindowViewFactoryCount == 0;
            }
            return result;
        }

        internal static bool DetermineSubquerySameStream(StatementSpecCompiled statementSpec, FilterStreamSpecCompiled filterStreamSpec)
        {
            foreach (ExprSubselectNode subselect in statementSpec.SubSelectExpressions) {
                StreamSpecCompiled streamSpec = subselect.StatementSpecCompiled.StreamSpecs[0];
                if (!(streamSpec is FilterStreamSpecCompiled)) {
                    continue;
                }
                FilterStreamSpecCompiled filterStream = (FilterStreamSpecCompiled) streamSpec;
                EventType typeSubselect = filterStream.FilterSpec.FilterForEventType;
                EventType typeFiltered = filterStreamSpec.FilterSpec.FilterForEventType;
                if (EventTypeUtility.IsTypeOrSubTypeOf(typeSubselect, typeFiltered) || EventTypeUtility.IsTypeOrSubTypeOf(typeFiltered, typeSubselect)) {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsConsumingFilters(EvalFactoryNode evalNode)
        {
            if (evalNode is EvalFilterFactoryNode) {
                return ((EvalFilterFactoryNode) evalNode).ConsumptionLevel != null;
            }
            bool consumption = false;
            foreach (EvalFactoryNode child in evalNode.ChildNodes) {
                consumption = consumption || IsConsumingFilters(child);
            }
            return consumption;
        }
    }
}
