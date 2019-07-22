///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.historical.common;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorHistoricalForge : ViewableActivatorForge
    {
        private readonly HistoricalEventViewableForge viewableForge;

        public ViewableActivatorHistoricalForge(HistoricalEventViewableForge viewableForge)
        {
            this.viewableForge = viewableForge;
        }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ViewableActivatorHistorical), GetType(), classScope);
            method.Block.DeclareVar<ViewableActivatorHistorical>(
                    "hist",
                    NewInstance(typeof(ViewableActivatorHistorical)))
                .SetProperty(Ref("hist"), "Factory", viewableForge.Make(method, symbols, classScope))
                .MethodReturn(Ref("hist"));
            return LocalMethod(method);
        }
    }
} // end of namespace