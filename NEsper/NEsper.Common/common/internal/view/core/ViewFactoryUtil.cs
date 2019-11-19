///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.view.core
{
    public class ViewFactoryUtil
    {
        public static int EvaluateSizeParam(
            string viewName,
            ExprEvaluator sizeEvaluator,
            AgentInstanceContext context)
        {
            var size = sizeEvaluator.Evaluate(null, true, context);
            if (!ValidateSize(size)) {
                throw new EPException(GetSizeValidationMsg(viewName, size));
            }

            return size.AsInt();
        }

        private static bool ValidateSize(object size)
        {
            return !(size == null || size.AsInt() <= 0);
        }

        private static string GetSizeValidationMsg(
            string viewName,
            object size)
        {
            return viewName + " view requires a positive integer for size but received " + size;
        }

        public static ViewablePair Materialize(
            ViewFactory[] factories,
            Viewable eventStreamParent,
            AgentInstanceViewFactoryChainContext viewFactoryChainContext,
            IList<AgentInstanceStopCallback> stopCallbacks)
        {
            if (factories.Length == 0) {
                return new ViewablePair(eventStreamParent, eventStreamParent);
            }

            var current = eventStreamParent;
            Viewable topView = null;
            Viewable streamView = null;

            foreach (var viewFactory in factories) {
                var view = viewFactory.MakeView(viewFactoryChainContext);
                if (view is AgentInstanceStopCallback) {
                    stopCallbacks.Add((AgentInstanceStopCallback) view);
                }

                current.Child = view;
                view.Parent = current;
                if (topView == null) {
                    topView = view;
                }

                streamView = view;
                current = view;
            }

            return new ViewablePair(topView, streamView);
        }
    }
} // end of namespace