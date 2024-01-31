///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public interface FireAndForgetProcessorForge
    {
        string ProcessorName { get; }

        string ContextName { get; }

        EventType EventTypeRSPInputEvents { get; }

        EventType EventTypePublic { get; }

        string[][] UniqueIndexes { get; }

        CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        void ValidateDependentExpr(
            StatementSpecCompiled statementSpec,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
        }
    }
}