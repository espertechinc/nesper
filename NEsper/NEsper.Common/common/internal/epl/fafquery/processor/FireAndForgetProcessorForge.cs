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
using com.espertech.esper.common.@internal.context.aifactory.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public interface FireAndForgetProcessorForge
    {
        string NamedWindowOrTableName { get; }

        string ContextName { get; }

        EventType EventTypeRspInputEvents { get; }

        EventType EventTypePublic { get; }

        string[][] UniqueIndexes { get; }

        CodegenExpression Make(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope);
    }

    public static class FireAndForgetProcessorForgeExtensions
    {
        private static CodegenExpression MakeArray(
            FireAndForgetProcessorForge[] processors,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(FireAndForgetProcessor[]),
                typeof(FireAndForgetProcessorForge),
                classScope);
            method.Block.DeclareVar(
                typeof(FireAndForgetProcessor[]), "processors",
                NewArrayByLength(typeof(FireAndForgetProcessor), Constant(processors.Length)));
            for (var i = 0; i < processors.Length; i++) {
                method.Block.AssignArrayElement(
                    "processors", Constant(i), processors[i].Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("processors"));
            return LocalMethod(method);
        }
    }
}