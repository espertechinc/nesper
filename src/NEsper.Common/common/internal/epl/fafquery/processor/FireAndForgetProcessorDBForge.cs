///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.hook.type;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.historical.database.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.settings;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public class FireAndForgetProcessorDBForge : FireAndForgetProcessorForge
    {
        private readonly HistoricalEventViewableDatabaseForge dbAccessForge;

        private readonly DBStatementStreamSpec sqlStreamSpec;

        public FireAndForgetProcessorDBForge(
            DBStatementStreamSpec sqlStreamSpec,
            StatementSpecCompiled statementSpec,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            this.sqlStreamSpec = sqlStreamSpec;

            var annotations = raw.Annotations;
            var typeConversionHook = (SQLColumnTypeConversion)ImportUtil.GetAnnotationHook(
                annotations,
                HookType.SQLCOL,
                typeof(SQLColumnTypeConversion),
                services.ImportServiceCompileTime);
            var outputRowConversionHook =
                (SQLOutputRowConversion)ImportUtil.GetAnnotationHook(
                    annotations,
                    HookType.SQLROW,
                    typeof(SQLOutputRowConversion),
                    services.ImportServiceCompileTime);
            dbAccessForge = HistoricalEventViewableDatabaseForgeFactory.CreateDBStatementView(
                0,
                sqlStreamSpec,
                typeConversionHook,
                outputRowConversionHook,
                raw,
                services,
                annotations);
        }

        public void ValidateDependentExpr(
            StatementSpecCompiled statementSpec,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            dbAccessForge.Validate(new StreamTypeServiceImpl(true), statementSpec.Raw.SqlParameters, raw, services);
        }

        public string ProcessorName => sqlStreamSpec.DatabaseName;

        public string ContextName => null;

        public EventType EventTypeRSPInputEvents => dbAccessForge.EventType;

        public EventType EventTypePublic => EventTypeRSPInputEvents;

        public string[][] UniqueIndexes => Array.Empty<string[]>();

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(FireAndForgetProcessorDB), GetType(), classScope);
            var factory = dbAccessForge.Make(method, symbols, classScope);
            var db = Ref("db");
            method.Block
                .DeclareVarNewInstance(typeof(FireAndForgetProcessorDB), db.Ref)
                .ExprDotMethod(db, "SetFactory", factory)
                .MethodReturn(db);
            return LocalMethod(method);
        }
    }
} // end of namespace