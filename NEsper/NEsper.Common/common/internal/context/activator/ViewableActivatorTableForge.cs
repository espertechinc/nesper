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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorTableForge : ViewableActivatorForge
    {
        private readonly ExprNode optionalFilterExpression;
        private readonly TableMetaData table;

        public ViewableActivatorTableForge(
            TableMetaData table,
            ExprNode optionalFilterExpression)
        {
            this.table = table;
            this.optionalFilterExpression = optionalFilterExpression;
        }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ViewableActivatorTable), GetType(), classScope);
            method.Block
                .DeclareVar<ViewableActivatorTable>("va", NewInstance(typeof(ViewableActivatorTable)))
                .SetProperty(
                    Ref("va"),
                    "Table",
                    TableDeployTimeResolver.MakeResolveTable(table, symbols.GetAddInitSvc(method)))
                .SetProperty(
                    Ref("va"),
                    "FilterEval",
                    optionalFilterExpression == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            optionalFilterExpression.Forge,
                            method,
                            GetType(),
                            classScope))
                .MethodReturn(Ref("va"));
            return LocalMethod(method);
        }
    }
} // end of namespace