///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.view;
using com.espertech.esper.view.internals;
using com.espertech.esper.view.window;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPStatementStartMethodHelperPrior
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static PriorEventViewFactory FindPriorViewFactory(IList<ViewFactory> factories)
        {
            ViewFactory factoryFound = null;
            foreach (var factory in factories)
            {
                if (factory is PriorEventViewFactory)
                {
                    factoryFound = factory;
                    break;
                }
            }
            if (factoryFound == null)
            {
                throw new EPException("Failed to find 'prior'-handling view factory");  // was verified earlier, should not occur
            }
            return (PriorEventViewFactory)factoryFound;
        }

        public static PriorEventViewFactory GetPriorEventViewFactory(
            StatementContext statementContext,
            int streamNum,
            bool unboundStream,
            bool isSubquery,
            int subqueryNumber)
        {
            try
            {
                var @namespace = ViewEnum.PRIOR_EVENT_VIEW.GetNamespace();
                var name = ViewEnum.PRIOR_EVENT_VIEW.GetName();
                var factory = statementContext.ViewResolutionService.Create(statementContext.Container, @namespace, name);

                var context = new ViewFactoryContext(statementContext, streamNum, @namespace, name, isSubquery, subqueryNumber, false);
                factory.SetViewParameters(context, ((ExprNode)new ExprConstantNodeImpl(unboundStream)).AsSingleton());

                return (PriorEventViewFactory)factory;
            }
            catch (ViewProcessingException ex)
            {
                const string text = "Exception creating prior event view factory";
                throw new EPException(text, ex);
            }
            catch (ViewParameterException ex)
            {
                var text = "Exception creating prior event view factory";
                throw new EPException(text, ex);
            }
        }

        public static IDictionary<ExprPriorNode, ExprPriorEvalStrategy> CompilePriorNodeStrategies(ViewResourceDelegateVerified viewResourceDelegate, AgentInstanceViewFactoryChainContext[] viewFactoryChainContexts)
        {

            if (!viewResourceDelegate.HasPrior)
            {
                return new Dictionary<ExprPriorNode, ExprPriorEvalStrategy>();
            }

            IDictionary<ExprPriorNode, ExprPriorEvalStrategy> strategies = new Dictionary<ExprPriorNode, ExprPriorEvalStrategy>();

            for (var streamNum = 0; streamNum < viewResourceDelegate.PerStream.Length; streamNum++)
            {
                var viewUpdatedCollection = viewFactoryChainContexts[streamNum].PriorViewUpdatedCollection;
                var callbacksPerIndex = viewResourceDelegate.PerStream[streamNum].PriorRequests;
                HandlePrior(viewUpdatedCollection, callbacksPerIndex, strategies);
            }

            return strategies;
        }

        private static void HandlePrior(ViewUpdatedCollection viewUpdatedCollection, IDictionary<int, IList<ExprPriorNode>> callbacksPerIndex, IDictionary<ExprPriorNode, ExprPriorEvalStrategy> strategies)
        {

            // Since an expression such as "prior(2, price), prior(8, price)" translates
            // into {2, 8} the relative index is {0, 1}.
            // Map the expression-supplied index to a relative viewUpdatedCollection-known index via wrapper
            var relativeIndex = 0;
            foreach (var reqIndex in callbacksPerIndex.Keys)
            {
                var priorNodes = callbacksPerIndex.Get(reqIndex);
                foreach (var callback in priorNodes)
                {
                    ExprPriorEvalStrategy strategy;
                    if (viewUpdatedCollection is RelativeAccessByEventNIndex)
                    {
                        var relativeAccess = (RelativeAccessByEventNIndex)viewUpdatedCollection;
                        var impl = new PriorEventViewRelAccess(relativeAccess, relativeIndex);
                        strategy = new ExprPriorEvalStrategyRelativeAccess(impl);
                    }
                    else
                    {
                        if (viewUpdatedCollection is RandomAccessByIndex)
                        {
                            strategy = new ExprPriorEvalStrategyRandomAccess((RandomAccessByIndex)viewUpdatedCollection);
                        }
                        else
                        {
                            strategy = new ExprPriorEvalStrategyRelativeAccess((RelativeAccessByEventNIndex)viewUpdatedCollection);
                        }
                    }

                    strategies.Put(callback, strategy);
                }
                relativeIndex++;
            }
        }
    }
}
