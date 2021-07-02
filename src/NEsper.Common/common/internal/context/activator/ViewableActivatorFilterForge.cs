///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.filterspec;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorFilterForge : ViewableActivatorForge
    {
        private readonly bool canIterate;

        private readonly FilterSpecCompiled filterSpecCompiled;
        private readonly bool isSubSelect;
        private readonly int? streamNumFromClause;
        private readonly int subselectNumber;

        public ViewableActivatorFilterForge(
            FilterSpecCompiled filterSpecCompiled,
            bool canIterate,
            int? streamNumFromClause,
            bool isSubSelect,
            int subselectNumber)
        {
            this.filterSpecCompiled = filterSpecCompiled;
            this.canIterate = canIterate;
            this.streamNumFromClause = streamNumFromClause;
            this.isSubSelect = isSubSelect;
            this.subselectNumber = subselectNumber;
        }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ViewableActivatorFilter), GetType(), classScope);

            var makeFilter = filterSpecCompiled.MakeCodegen(method, symbols, classScope);
            method.Block
                .DeclareVar<FilterSpecActivatable>("filterSpecCompiled", LocalMethod(makeFilter));
                
            var initializer = ExprDotMethodChain(symbols.GetAddInitSvc(method))
                .Get(EPStatementInitServicesConstants.VIEWABLEACTIVATORFACTORY)
                .Add("CreateFilter");
            var builder = new CodegenSetterBuilder(typeof(ViewableActivatorFilter), typeof(ViewableActivatorFilterForge), "activator", classScope, method, initializer)
                .Expression("Container", ExprDotName(Ref("stmtInitSvc"), "Container"))
                .Expression("FilterSpec", Ref("filterSpecCompiled"))
                .ConstantDefaultChecked("CanIterate", canIterate)
                .ConstantDefaultChecked("StreamNumFromClause", streamNumFromClause)
                .ConstantDefaultChecked("IsSubSelect", isSubSelect)
                .ConstantDefaultChecked("SubselectNumber", subselectNumber);
            method.Block.MethodReturn(builder.RefName);
            return LocalMethod(method);
        }
    }
} // end of namespace