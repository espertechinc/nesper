///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedValidation : ContextControllerPortableInfo
    {
        public ContextControllerKeyedValidation(ContextControllerKeyedValidationItem[] items)
        {
            Items = items;
        }

        public ContextControllerKeyedValidationItem[] Items { get; }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            var init = new CodegenExpression[Items.Length];
            for (var i = 0; i < init.Length; i++) {
                init[i] = Items[i].Make(addInitSvc);
            }

            return NewInstance<ContextControllerKeyedValidation>(
                NewArrayWithInit(typeof(ContextControllerKeyedValidationItem), init));
        }

        public void ValidateStatement(
            string contextName,
            StatementSpecCompiled spec,
            StatementCompileTimeServices compileTimeServices)
        {
            ContextControllerForgeUtil.ValidateStatementKeyAndHash(
                Items.Select(i => (Supplier<EventType>) i.Get).ToArray(),
                contextName,
                spec,
                compileTimeServices);
        }
    }
} // end of namespace