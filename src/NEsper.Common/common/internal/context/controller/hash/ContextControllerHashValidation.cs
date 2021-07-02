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

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashValidation : ContextControllerPortableInfo
    {
        private readonly ContextControllerHashValidationItem[] _items;

        public ContextControllerHashValidation(ContextControllerHashValidationItem[] items)
        {
            this._items = items;
        }

        public ContextControllerHashValidationItem[] GetItems()
        {
            return _items;
        }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            CodegenExpression[] init = new CodegenExpression[_items.Length];
            for (int i = 0; i < init.Length; i++) {
                init[i] = _items[i].Make(addInitSvc);
            }

            return NewInstance<ContextControllerHashValidation>(
                NewArrayWithInit(typeof(ContextControllerHashValidationItem), init));
        }

        public void ValidateStatement(
            string contextName,
            StatementSpecCompiled spec,
            StatementCompileTimeServices compileTimeServices)
        {
            ContextControllerForgeUtil.ValidateStatementKeyAndHash(
                _items.Select(i => (Supplier<EventType>) i.Get),
                contextName,
                spec,
                compileTimeServices);
        }
    }
} // end of namespace