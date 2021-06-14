///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.datetime.reformatop;
using com.espertech.esper.common.@internal.epl.expression.codegen;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public abstract class DTLocalForgeCalopReformatBase : DTLocalForge
    {
        internal readonly IList<CalendarForge> calendarForges;
        internal readonly ReformatForge reformatForge;

        public abstract DTLocalEvaluator DTEvaluator { get; }

        public abstract CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        protected DTLocalForgeCalopReformatBase(
            IList<CalendarForge> calendarForges,
            ReformatForge reformatForge)
        {
            this.calendarForges = calendarForges;
            this.reformatForge = reformatForge;
        }
    }
} // end of namespace