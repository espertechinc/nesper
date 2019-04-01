///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.groupwin
{
    public class GroupByViewUtil
    {
        protected internal static View MakeSubView(GroupByView view, object groupKey)
        {
            var agentInstanceContext = view.AgentInstanceContext;
            var mergeView = view.MergeView;

            var factories = view.ViewFactory.Groupeds;
            View first = factories[0].MakeView(agentInstanceContext);
            first.Parent = view;
            var currentParent = first;
            for (var i = 1; i < factories.Length; i++) {
                View next = factories[i].MakeView(agentInstanceContext);
                next.Parent = currentParent;
                currentParent.Child = next;
                currentParent = next;
            }

            if (view.ViewFactory.IsAddingProperties) {
                var adder = new AddPropertyValueOptionalView(view.ViewFactory, agentInstanceContext, groupKey);
                currentParent.Child = adder;
                adder.Parent = currentParent;

                adder.Child = mergeView;
                mergeView.AddParentView(adder);
            }
            else {
                currentParent.Child = mergeView;
                mergeView.AddParentView(currentParent);
            }

            return first;
        }

        public static void RemoveSubview(View view, AgentInstanceStopServices services)
        {
            view.Parent = null;
            if (view is AgentInstanceStopCallback) {
                ((AgentInstanceStopCallback) view).Stop(services);
            }

            RecursiveChildRemove(view, services);
        }

        private static void RecursiveChildRemove(View view, AgentInstanceStopServices services)
        {
            var child = view.Child;
            if (child == null) {
                return;
            }

            if (child is MergeView) {
                var mergeView = (MergeView) child;
                mergeView.RemoveParentView(view);
            }
            else {
                if (child is AgentInstanceStopCallback) {
                    ((AgentInstanceStopCallback) child).Stop(services);
                }

                RecursiveChildRemove(child, services);
            }
        }
    }
} // end of namespace