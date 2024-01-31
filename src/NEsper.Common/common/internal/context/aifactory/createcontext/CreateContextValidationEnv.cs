///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.context.aifactory.createcontext
{
    public class CreateContextValidationEnv
    {
        public CreateContextValidationEnv(
            string contextName,
            NameAccessModifier contextVisibility,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services,
            IList<FilterSpecTracked> filterSpecCompileds,
            IList<ScheduleHandleTracked> scheduleHandleCallbackProviders,
            IList<FilterSpecParamExprNodeForge> filterBooleanExpressions)
        {
            ContextName = contextName;
            ContextVisibility = contextVisibility;
            StatementRawInfo = statementRawInfo;
            Services = services;
            FilterSpecCompileds = filterSpecCompileds;
            ScheduleHandleCallbackProviders = scheduleHandleCallbackProviders;
            FilterBooleanExpressions = filterBooleanExpressions;
        }

        public string ContextName { get; }

        public NameAccessModifier ContextVisibility { get; }

        public StatementRawInfo StatementRawInfo { get; }

        public StatementCompileTimeServices Services { get; }

        public IList<FilterSpecTracked> FilterSpecCompileds { get; }

        public IList<ScheduleHandleTracked> ScheduleHandleCallbackProviders { get; }

        public IList<FilterSpecParamExprNodeForge> FilterBooleanExpressions { get; }
    }
} // end of namespace