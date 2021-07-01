///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    /// View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public class InfraOnExprBaseViewResult
    {
        private readonly View view;
        private readonly AggregationService optionalAggregationService;

        public InfraOnExprBaseViewResult(
            View view,
            AggregationService optionalAggregationService)
        {
            this.view = view;
            this.optionalAggregationService = optionalAggregationService;
        }

        public View View {
            get => view;
        }

        public AggregationService OptionalAggregationService {
            get => optionalAggregationService;
        }
    }
} // end of namespace