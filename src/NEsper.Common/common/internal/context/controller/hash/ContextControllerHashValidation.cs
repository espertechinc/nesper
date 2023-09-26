///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashValidation : ContextControllerPortableInfo
    {
        private readonly ContextControllerHashValidationItem[] items;

        public ContextControllerHashValidation(ContextControllerHashValidationItem[] items)
        {
            this.items = items;
        }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            var init = new CodegenExpression[items.Length];
            for (var i = 0; i < init.Length; i++) {
                init[i] = items[i].Make(addInitSvc);
            }

            return NewInstance(
                typeof(ContextControllerHashValidation),
                NewArrayWithInit(typeof(ContextControllerHashValidationItem), init));
        }

        public void ValidateStatement(
            string contextName,
            StatementSpecCompiled spec,
            StatementCompileTimeServices compileTimeServices)
        {
            var typeProvider = items
                .Select(_ => (Supplier<EventType>) _.Get)
                .ToArray();

            ContextControllerForgeUtil.ValidateStatementKeyAndHash(
                typeProvider,
                contextName,
                spec,
                compileTimeServices);
        }

        public void VisitFilterAddendumEventTypes(Consumer<EventType> consumer)
        {
            foreach (var item in items) {
                item.VisitFilterAddendumEventTypes(consumer);
            }
        }

        public ContextControllerHashValidationItem[] Items => items;
    }
} // end of namespace