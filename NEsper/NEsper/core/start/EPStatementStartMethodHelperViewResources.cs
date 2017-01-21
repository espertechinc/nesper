///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.rowregex;
using com.espertech.esper.view;
using com.espertech.esper.view.internals;
using com.espertech.esper.view.std;

namespace com.espertech.esper.core.start
{
    public class EPStatementStartMethodHelperViewResources
    {
        public static ViewResourceDelegateVerified VerifyPreviousAndPriorRequirements(ViewFactoryChain[] unmaterializedViewChain, ViewResourceDelegateUnverified @delegate)
        {
            var hasPriorNodes = !@delegate.PriorRequests.IsEmpty();
            var hasPreviousNodes = !@delegate.PreviousRequests.IsEmpty();
    
            var numStreams = unmaterializedViewChain.Length;
            var perStream = new ViewResourceDelegateVerifiedStream[numStreams];
    
            // verify "previous"
            var previousPerStream = new IList<ExprPreviousNode>[numStreams];
            foreach (var previousNode in @delegate.PreviousRequests) {
                var stream = previousNode.StreamNumber;
                var factories = unmaterializedViewChain[stream].FactoryChain;
    
                var pass = InspectViewFactoriesForPrevious(factories);
                if (!pass) {
                    throw new ExprValidationException("Previous function requires a single data window view onto the stream");
                }
    
                var found = factories.OfType<DataWindowViewWithPrevious>().Any();
                if (!found) {
                    throw new ExprValidationException("Required data window not found for the 'prev' function, specify a data window for which previous events are retained");
                }
    
                if (previousPerStream[stream] == null) {
                    previousPerStream[stream] = new List<ExprPreviousNode>();
                }
                previousPerStream[stream].Add(previousNode);
            }
    
            // verify "prior"
            var priorPerStream = new IDictionary<int, IList<ExprPriorNode>>[numStreams];
            foreach (var priorNode in @delegate.PriorRequests) {
                var stream = priorNode.StreamNumber;
    
                if (priorPerStream[stream] == null) {
                    priorPerStream[stream] = new SortedDictionary<int, IList<ExprPriorNode>>();
                }

                var treemap = priorPerStream[stream];
                var callbackList = treemap.Get(priorNode.ConstantIndexNumber);
                if (callbackList == null)
                {
                    callbackList = new List<ExprPriorNode>();
                    treemap.Put(priorNode.ConstantIndexNumber, callbackList);
                }
                callbackList.Add(priorNode);
            }
    
            // build per-stream info
            for (var i = 0; i < numStreams; i++) {
                if (previousPerStream[i] == null) {
                    previousPerStream[i] = Collections.GetEmptyList<ExprPreviousNode>();
                }
                if (priorPerStream[i] == null) {
                    priorPerStream[i] = new OrderedDictionary<int, IList<ExprPriorNode>>();
                }
    
                // determine match-recognize "prev"
                var matchRecognizePrevious = Collections.GetEmptySet<ExprPreviousMatchRecognizeNode>();
                if (i == 0) {
                    foreach (var viewFactory in unmaterializedViewChain[i].FactoryChain) {
                        if (viewFactory is EventRowRegexNFAViewFactory) {
                            var matchRecognize = (EventRowRegexNFAViewFactory) viewFactory;
                            matchRecognizePrevious = matchRecognize.PreviousExprNodes;
                        }
                    }
                }
    
                perStream[i] = new ViewResourceDelegateVerifiedStream(previousPerStream[i], priorPerStream[i], matchRecognizePrevious);
            }
    
            return new ViewResourceDelegateVerified(hasPriorNodes, hasPreviousNodes, perStream);
        }
    
        private static bool InspectViewFactoriesForPrevious(IList<ViewFactory> viewFactories)
        {
            // We allow the capability only if
            //  - 1 view
            //  - 2 views and the first view is a group-by (for window-per-group access)
            if (viewFactories.Count == 1)
            {
                return true;
            }
            if (viewFactories.Count == 2)
            {
                if (viewFactories[0] is GroupByViewFactory)
                {
                    return true;
                }
                if (viewFactories[1] is PriorEventViewFactory) {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}
