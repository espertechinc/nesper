///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.guard;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.regressionlib.support.extend.pattern
{
    public class MyCountToPatternGuardForge : GuardForge
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private MatchedEventConvertorForge _convertor;
        private ExprNode _numCountToExpr;

        public void SetGuardParameters(
            IList<ExprNode> guardParameters,
            MatchedEventConvertorForge convertor,
            StatementCompileTimeServices services)
        {
            var message = "Count-to guard takes a single integer-value expression as parameter";
            if (guardParameters.Count != 1) {
                throw new GuardParameterException(message);
            }

            var paramType = guardParameters[0].Forge.EvaluationType;
            if (!paramType.IsInt32()) {
                throw new GuardParameterException(message);
            }

            _numCountToExpr = guardParameters[0];
            _convertor = convertor;
        }

        public void CollectSchedule(IList<ScheduleHandleCallbackProvider> schedules)
        {
        }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var builder = new SAIFFInitializeBuilder(
                typeof(MyCountToPatternGuardFactory),
                GetType(),
                "GuardFactory",
                parent,
                symbols,
                classScope);
            return builder.Exprnode("numCountToExpr", _numCountToExpr)
                .Expression("convertor", _convertor.MakeAnonymous(builder.Method(), classScope))
                .Build();
        }
    }
} // end of namespace