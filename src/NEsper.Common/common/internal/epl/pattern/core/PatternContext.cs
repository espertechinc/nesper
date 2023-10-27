///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.filterspec;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    /// Contains handles to implementations of services needed by evaluation nodes.
    /// </summary>
    public class PatternContext
    {
        private int streamNumber;
        private MatchedEventMapMeta matchedEventMapMeta;
        private bool isContextDeclaration;
        private int nestingLevel;
        private bool startCondition;

        public PatternContext()
        {
        }

        public PatternContext(
            int streamNumber,
            MatchedEventMapMeta matchedEventMapMeta,
            bool isContextDeclaration,
            int nestingLevel,
            bool startCondition)
        {
            this.streamNumber = streamNumber;
            this.matchedEventMapMeta = matchedEventMapMeta;
            this.isContextDeclaration = isContextDeclaration;
            this.nestingLevel = nestingLevel;
            this.startCondition = startCondition;
        }

        public bool IsContextDeclaration => isContextDeclaration;

        public bool IsStartCondition => startCondition;

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(PatternContext), GetType(), classScope);
            method.Block.DeclareVar<PatternContext>("ctx", NewInstance(typeof(PatternContext)))
                .SetProperty(
                    Ref("ctx"),
                    "MatchedEventMapMeta",
                    LocalMethod(matchedEventMapMeta.MakeCodegen(classScope, method, symbols)));
            if (streamNumber != 0) {
                method.Block.SetProperty(Ref("ctx"), "StreamNumber", Constant(streamNumber));
            }

            if (isContextDeclaration) {
                method.Block.SetProperty(Ref("ctx"), "ContextDeclaration", Constant(isContextDeclaration))
                    .SetProperty(Ref("ctx"), "NestingLevel", Constant(nestingLevel))
                    .SetProperty(Ref("ctx"), "StartCondition", Constant(startCondition));
            }

            method.Block.MethodReturn(Ref("ctx"));
            return LocalMethod(method);
        }

        public int StreamNumber {
            get => streamNumber;

            set => streamNumber = value;
        }

        public MatchedEventMapMeta MatchedEventMapMeta {
            get => matchedEventMapMeta;

            set => matchedEventMapMeta = value;
        }

        public bool ContextDeclaration {
            set => isContextDeclaration = value;
        }

        public int NestingLevel {
            get => nestingLevel;

            set => nestingLevel = value;
        }

        public bool StartCondition {
            set => startCondition = value;
        }
    }
} // end of namespace