///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerDetailKeyed : ContextControllerDetail,
        StatementReadyCallback
    {
        public ContextControllerDetailKeyedItem[] Items { get; set; }

        public ContextConditionDescriptorFilter[] OptionalInit { get; set; }

        public ContextConditionDescriptor OptionalTermination { get; set; }

        public IList<FilterSpecActivatable> FilterSpecActivatables { get; private set; }

        public bool HasAsName { get; private set; }

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            FilterSpecActivatables = new List<FilterSpecActivatable>();
            foreach (var item in Items) {
                FilterSpecActivatables.Add(item.FilterSpecActivatable);
            }

            if (OptionalTermination != null) {
                OptionalTermination.AddFilterSpecActivatable(FilterSpecActivatables);
            }

            // determine whether we have named-partitioning-events
            foreach (var item in Items) {
                if (item.AliasName != null) {
                    HasAsName = true;
                }
            }

            if (!HasAsName && OptionalInit != null) {
                foreach (var filter in OptionalInit) {
                    if (filter.OptionalFilterAsName != null) {
                        HasAsName = true;
                    }
                }
            }
        }
    }
} // end of namespace