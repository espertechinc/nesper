///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodIUDDeleteForge : FAFQueryMethodIUDBaseForge
    {
        public FAFQueryMethodIUDDeleteForge(
            StatementSpecCompiled spec,
            Compilable compilable,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
            : base(spec, compilable, statementRawInfo, services)
        {
        }

        protected override void InitExec(
            string aliasName,
            StatementSpecCompiled spec,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
        }

        protected override Type TypeOfMethod()
        {
            return typeof(FAFQueryMethodIUDDelete);
        }

        protected override void MakeInlineSpecificSetter(
            CodegenExpressionRef queryMethod,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.SetProperty(
                queryMethod,
                "OptionalWhereClause",
                whereClause == null
                    ? ConstantNull()
                    : ExprNodeUtilityCodegen.CodegenEvaluator(whereClause.Forge, method, GetType(), classScope));
        }
    }
} // end of namespace