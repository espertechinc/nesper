///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.filterspec;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    ///     Contains handles to implementations of services needed by evaluation nodes.
    /// </summary>
    public class PatternContext
    {
        public PatternContext()
        {
        }

        public PatternContext(
            int streamNumber,
            MatchedEventMapMeta matchedEventMapMeta,
            bool isContextDeclaration,
            int nestingLevel,
            bool isStartCondition)
        {
            StreamNumber = streamNumber;
            MatchedEventMapMeta = matchedEventMapMeta;
            IsContextDeclaration = isContextDeclaration;
            NestingLevel = nestingLevel;
            IsStartCondition = isStartCondition;
        }

        public int StreamNumber { get; set; }

        public MatchedEventMapMeta MatchedEventMapMeta { get; set; }

        public bool IsContextDeclaration { get; set; }

        public int NestingLevel { get; set; }

        public bool IsStartCondition { get; set; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(PatternContext), GetType(), classScope);
            method.Block
                .DeclareVar<PatternContext>("ctx", NewInstance(typeof(PatternContext)))
                .SetProperty(
                    Ref("ctx"),
                    "MatchedEventMapMeta",
                    LocalMethod(MatchedEventMapMeta.MakeCodegen(classScope, method, symbols)));
            if (StreamNumber != 0) {
                method.Block.SetProperty(Ref("ctx"), "StreamNumber", Constant(StreamNumber));
            }

            if (IsContextDeclaration) {
                method.Block
                    .SetProperty(Ref("ctx"), "IsContextDeclaration", Constant(IsContextDeclaration))
                    .SetProperty(Ref("ctx"), "NestingLevel", Constant(NestingLevel))
                    .SetProperty(Ref("ctx"), "IsStartCondition", Constant(IsStartCondition));
            }

            method.Block.MethodReturn(Ref("ctx"));
            return LocalMethod(method);
        }
    }
} // end of namespace