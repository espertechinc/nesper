///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    public class ExprDeclaredForgeNoRewrite : ExprDeclaredForgeBase
    {
        public ExprDeclaredForgeNoRewrite(ExprDeclaredNodeImpl parent, ExprForge innerForge, bool isCache, bool audit, string statementName)

             : base(parent, innerForge, isCache, audit, statementName)

        {
        }

        public override EventBean[] GetEventsPerStreamRewritten(EventBean[] eventsPerStream)
        {
            return eventsPerStream;
        }

        protected override CodegenExpression CodegenEventsPerStreamRewritten(CodegenExpression eventsPerStream, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return eventsPerStream;
        }

        public override ExprForgeConstantType ForgeConstantType
        {
            get => ExprForgeConstantType.NONCONST;
        }
    }
} // end of namespace