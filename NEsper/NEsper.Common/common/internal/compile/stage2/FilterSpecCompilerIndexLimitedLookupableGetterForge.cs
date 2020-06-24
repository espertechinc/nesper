///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerIndexLimitedLookupableGetterForge : ExprEventEvaluatorForge
    {
        private readonly ExprNode _lookupable;

        public FilterSpecCompilerIndexLimitedLookupableGetterForge(ExprNode lookupable)
        {
            this._lookupable = lookupable;
        }

        public CodegenExpression EventBeanWithCtxGet(
            CodegenExpression beanExpression,
            CodegenExpression ctxExpression,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent
                .MakeChild(typeof(object), GetType(), classScope)
                .AddParam(typeof(EventBean), "eventBean")
                .AddParam(typeof(ExprEvaluatorContext), "ctx");
            var getImpl = CodegenLegoMethodExpression.CodegenExpression(_lookupable.Forge, method, classScope);
            method.Block
                .DeclareVar(typeof(EventBean[]), "events", NewArrayWithInit(typeof(EventBean), Ref("eventBean")))
                .MethodReturn(LocalMethod(getImpl, NewArrayWithInit(typeof(EventBean), Ref("eventBean")), ConstantTrue(), Ref("ctx")));
            return LocalMethod(method, beanExpression, ctxExpression);
        }
    }
} // end of namespace