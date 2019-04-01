///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.agg.method
{
    public class ExprMethodAggUtil
    {
        public static ExprForge[] GetDefaultForges(ExprNode[] childNodes, bool join, EventType[] typesPerStream)
        {
            if (childNodes.Length == 0)
            {
                return ExprNodeUtilityQuery.EMPTY_FORGE_ARRAY;
            }

            var forges = new ExprForge[childNodes.Length];
            for (var i = 0; i < childNodes.Length; i++)
            {
                if (childNodes[i] is ExprWildcard)
                {
                    ValidateWildcard(typesPerStream, join);
                    forges[i] = new ExprForgeWildcard(typesPerStream[0].UnderlyingType);
                }
                else
                {
                    forges[i] = childNodes[i].Forge;
                }
            }

            return forges;
        }

        public static ExprEvaluator GetMultiNodeEvaluator(ExprNode[] childNodes, bool join, EventType[] typesPerStream)
        {
            var evaluators = new ExprEvaluator[childNodes.Length];

            // determine constant nodes
            var count = 0;
            foreach (var node in childNodes)
            {
                if (node is ExprWildcard)
                {
                    evaluators[count] = GetWildcardEvaluator(typesPerStream, join);
                }
                else
                {
                    evaluators[count] = node.Forge.ExprEvaluator;
                }

                count++;
            }

            return new ProxyExprEvaluator
            {
                ProcEvaluate = (eventsPerStream, isNewData, exprEvaluatorContext) =>
                {
                    var values = new object[evaluators.Length];
                    for (var i = 0; i < evaluators.Length; i++)
                    {
                        values[i] = evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                    }

                    return values;
                }
            };
        }

        private static ExprEvaluator GetWildcardEvaluator(EventType[] typesPerStream, bool isJoin)
        {
            ValidateWildcard(typesPerStream, isJoin);
            return ExprEvaluatorWildcard.INSTANCE;
        }

        private static void ValidateWildcard(EventType[] typesPerStream, bool isJoin)
        {
            var returnType = typesPerStream != null && typesPerStream.Length > 0
                ? typesPerStream[0].UnderlyingType
                : null;
            if (isJoin || returnType == null)
            {
                throw new ExprValidationException(
                    "Invalid use of wildcard (*) for stream selection in a join or an empty from-clause, please use the stream-alias syntax to select a specific stream instead");
            }
        }
    }
} // end of namespace