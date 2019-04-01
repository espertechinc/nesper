///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.path;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.module
{
    public interface EPModuleEventTypeInitServices
    {
        EventTypeCollector EventTypeCollector { get; }
        EventTypeResolver EventTypeResolver { get; }
    }

    public static class EPModuleEventTypeInitServicesConstants
    {
        public static readonly string GETEVENTTYPECOLLECTOR = "getEventTypeCollector";
        public static readonly string GETEVENTTYPERESOLVER = EPStatementInitServicesConstants.GETEVENTTYPERESOLVER;
        public static readonly CodegenExpressionRef REF = Ref("epModuleETInit");
    }
} // end of namespace