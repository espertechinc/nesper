///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.prev;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.groupwin;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.util
{
    public class ViewResourceVerifyHelper
    {
        public static ViewResourceVerifyResult VerifyPreviousAndPriorRequirements(
            IList<ViewFactoryForge>[] unmaterializedViewChain,
            ViewResourceDelegateExpr @delegate,
            int? subqueryNumber,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            var numStreams = unmaterializedViewChain.Length;
            var perStream = new ViewResourceDelegateDesc[numStreams];
            var fabricCharge = services.StateMgmtSettingsProvider.NewCharge();

            // verify "previous"
            var previousPerStream = new bool[numStreams];
            foreach (var previousNode in @delegate.PreviousRequests) {
                var stream = previousNode.StreamNumber;
                var forges = unmaterializedViewChain[stream];

                var pass = InspectViewFactoriesForPrevious(forges);
                if (!pass) {
                    throw new ExprValidationException(
                        "Previous function requires a single data window view onto the stream");
                }

                var found = FindDataWindow(forges);
                if (!found) {
                    throw new ExprValidationException(
                        "Required data window not found for the 'prev' function, specify a data window for which previous events are retained");
                }

                previousPerStream[stream] = true;
            }

            var previousStateMgmtSettings = new StateMgmtSetting[numStreams];
            for (var streamNum = 0; streamNum < numStreams; streamNum++) {
                if (previousPerStream[streamNum]) {
                    var forges = unmaterializedViewChain[streamNum];
                    var eventType = forges[^1].EventType;
                    previousStateMgmtSettings[streamNum] = services.StateMgmtSettingsProvider.Previous(
                        fabricCharge,
                        raw,
                        streamNum,
                        subqueryNumber,
                        eventType);
                }
            }

            // determine 'prior' indexes
            var priorPerStream = new IOrderedDictionary<int, IList<ExprPriorNode>>[numStreams];
            foreach (var priorNode in @delegate.PriorRequests) {
                var stream = priorNode.StreamNumber;

                if (priorPerStream[stream] == null) {
                    priorPerStream[stream] = new OrderedListDictionary<int, IList<ExprPriorNode>>();
                }

                var treemap = priorPerStream[stream];
                var callbackList = treemap.Get(priorNode.ConstantIndexNumber);
                if (callbackList == null) {
                    callbackList = new List<ExprPriorNode>();
                    treemap.Put(priorNode.ConstantIndexNumber, callbackList);
                }

                callbackList.Add(priorNode);
            }

            // when a given stream has multiple 'prior' nodes, assign a relative index
            for (var i = 0; i < numStreams; i++) {
                if (priorPerStream[i] != null) {
                    var relativeIndex = 0;
                    foreach (var entry in priorPerStream[i]) {
                        foreach (var node in entry.Value) {
                            node.RelativeIndex = relativeIndex;
                        }

                        relativeIndex++;
                    }
                }
            }

            // build per-stream info
            for (var i = 0; i < numStreams; i++) {
                if (priorPerStream[i] == null) {
                    priorPerStream[i] = new OrderedListDictionary<int, IList<ExprPriorNode>>();
                }

                perStream[i] = new ViewResourceDelegateDesc(
                    previousPerStream[i],
                    previousStateMgmtSettings[i],
                    new SortedSet<int>(priorPerStream[i].Keys));
            }

            return new ViewResourceVerifyResult(perStream, fabricCharge);
        }

        private static bool FindDataWindow(IList<ViewFactoryForge> forges)
        {
            foreach (var forge in forges) {
                if (forge is DataWindowViewForgeWithPrevious) {
                    return true;
                }

                if (forge is GroupByViewFactoryForge group) {
                    return FindDataWindow(group.Groupeds);
                }
            }

            return false;
        }

        private static bool InspectViewFactoriesForPrevious(IList<ViewFactoryForge> viewFactories)
        {
            // We allow the capability only if
            //  - 1 view
            //  - 2 views and the first view is a group-by (for window-per-group access)
            if (viewFactories.Count == 1) {
                return true;
            }

            if (viewFactories.Count == 2) {
                if (viewFactories[0] is GroupByViewFactoryForge) {
                    return true;
                }

                return false;
            }

            return true;
        }
    }
} // end of namespace