///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.contained
{
    public class ContainedEventEvalGetterForge : ContainedEventEvalForge
    {
        private readonly EventPropertyGetterSPI getter;

        public ContainedEventEvalGetterForge(EventPropertyGetterSPI getter)
        {
            this.getter = getter;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(ContainedEventEvalGetter), this.GetType(), classScope);

            var getFragment = new CodegenExpressionLambda(method.Block)
                .WithParams(new CodegenNamedParam(typeof(EventBean), "@event"));
            var anonymousClass = NewInstance<ProxyEventPropertyFragmentGetter>(getFragment);

            //var anonymousClass = NewAnonymousClass(
            //    method.Block,
            //    typeof(EventPropertyFragmentGetter));
            //var getFragment = CodegenMethod.MakeParentNode(typeof(object), this.GetType(), classScope)
            //    .AddParam(typeof(EventBean), "@event");
            //anonymousClass.AddMethod("GetFragment", getFragment);
            getFragment.Block.BlockReturn(getter.EventBeanFragmentCodegen(Ref("@event"), method /* getFragment */, classScope));

            method.Block.MethodReturn(NewInstance<ContainedEventEvalGetter>(anonymousClass));
            return LocalMethod(method);
        }
    }
} // end of namespace