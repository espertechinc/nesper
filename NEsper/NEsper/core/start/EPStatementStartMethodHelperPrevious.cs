///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.view;
using com.espertech.esper.view.window;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPStatementStartMethodHelperPrevious
    {
        private static readonly Dictionary<ExprPreviousNode, ExprPreviousEvalStrategy> EmptyMap =
            new Dictionary<ExprPreviousNode, ExprPreviousEvalStrategy>();

        public static IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> CompilePreviousNodeStrategies(ViewResourceDelegateVerified viewResourceDelegate, AgentInstanceViewFactoryChainContext[] contexts)
        {
            if (!viewResourceDelegate.HasPrevious)
            {
                return EmptyMap;
            }

            IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> strategies = new Dictionary<ExprPreviousNode, ExprPreviousEvalStrategy>();

            for (int streamNum = 0; streamNum < contexts.Length; streamNum++)
            {
                // get stream-specific INFO
                ViewResourceDelegateVerifiedStream @delegate = viewResourceDelegate.PerStream[streamNum];

                // obtain getter
                HandlePrevious(@delegate.PreviousRequests, contexts[streamNum].PreviousNodeGetter, strategies);
            }

            return strategies;
        }

        public static DataWindowViewWithPrevious FindPreviousViewFactory(IList<ViewFactory> factories)
        {
            ViewFactory factoryFound = null;
            foreach (ViewFactory factory in factories)
            {
                if (factory is DataWindowViewWithPrevious)
                {
                    factoryFound = factory;
                    break;
                }
            }
            if (factoryFound == null)
            {
                throw new EPException("Failed to find 'previous'-handling view factory");  // was verified earlier, should not occur
            }
            return (DataWindowViewWithPrevious)factoryFound;
        }

        private static void HandlePrevious(IList<ExprPreviousNode> previousRequests, Object previousNodeGetter, IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> strategies)
        {
            if (previousRequests.IsEmpty())
            {
                return;
            }

            RandomAccessByIndexGetter randomAccessGetter = null;
            RelativeAccessByEventNIndexGetter relativeAccessGetter = null;
            if (previousNodeGetter is RandomAccessByIndexGetter)
            {
                randomAccessGetter = (RandomAccessByIndexGetter)previousNodeGetter;
            }
            else if (previousNodeGetter is RelativeAccessByEventNIndexGetter)
            {
                relativeAccessGetter = (RelativeAccessByEventNIndexGetter)previousNodeGetter;
            }
            else
            {
                throw new EPException("Unexpected 'previous' handler: " + previousNodeGetter);
            }

            foreach (ExprPreviousNode previousNode in previousRequests)
            {
                int streamNumber = previousNode.StreamNumber;
                ExprPreviousNodePreviousType previousType = previousNode.PreviousType;
                ExprPreviousEvalStrategy evaluator;

                if (previousType == ExprPreviousNodePreviousType.PREVWINDOW)
                {
                    evaluator = new ExprPreviousEvalStrategyWindow(streamNumber, previousNode.ChildNodes[1].ExprEvaluator, previousNode.ReturnType.GetElementType(),
                            randomAccessGetter, relativeAccessGetter);
                }
                else if (previousType == ExprPreviousNodePreviousType.PREVCOUNT)
                {
                    evaluator = new ExprPreviousEvalStrategyCount(streamNumber, randomAccessGetter, relativeAccessGetter);
                }
                else
                {
                    evaluator = new ExprPreviousEvalStrategyPrev(streamNumber, previousNode.ChildNodes[0].ExprEvaluator, previousNode.ChildNodes[1].ExprEvaluator,
                            randomAccessGetter, relativeAccessGetter, previousNode.IsConstantIndex, previousNode.ConstantIndexNumber, previousType == ExprPreviousNodePreviousType.PREVTAIL);
                }

                strategies.Put(previousNode, evaluator);
            }
        }
    }
}
