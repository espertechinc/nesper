///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.path;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.module
{
    public interface EPModuleTableInitServices
    {
        TableCollector TableCollector { get; }

        EventTypeResolver EventTypeResolver { get; }
    }

    public static class EPModuleTableInitServicesConstants
    {
        public static readonly string GETEVENTTYPERESOLVER = EPStatementInitServicesConstants.EVENTTYPERESOLVER;
        public static readonly string GETTABLECOLLECTOR = "TableCollector";
        public static readonly CodegenExpressionRef REF = Ref("epModuleTableInitServices");
    }
} // end of namespace