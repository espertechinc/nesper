///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.subselect;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.update
{
    public class StatementAgentInstanceFactoryUpdateForge
    {
        private readonly InternalEventRouterDescForge forge;
        private readonly IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselects;

        public StatementAgentInstanceFactoryUpdateForge(
            InternalEventRouterDescForge forge,
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselects)
        {
            this.forge = forge;
            this.subselects = subselects;
        }

        public CodegenMethod InitializeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(StatementAgentInstanceFactoryUpdate), GetType(), classScope);
            method.Block
                .DeclareVar<StatementAgentInstanceFactoryUpdate>(
                    "saiff",
                    NewInstance(typeof(StatementAgentInstanceFactoryUpdate)))
                .SetProperty(Ref("saiff"), "Desc", forge.Make(method, symbols, classScope))
                .SetProperty(
                    Ref("saiff"),
                    "Subselects",
                    SubSelectFactoryForge.CodegenInitMap(subselects, GetType(), method, symbols, classScope))
                .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add("AddReadyCallback", Ref("saiff")))
                .MethodReturn(Ref("saiff"));
            return method;
        }
    }
} // end of namespace