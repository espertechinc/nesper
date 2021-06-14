///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.plain.exceptintersectunion
{
    public class EnumExceptForge : EnumForge
    {
        internal readonly ExprEnumerationForge evaluatorForge;
        internal readonly bool scalar;

        public EnumExceptForge(
            int numStreams,
            ExprEnumerationForge evaluatorForge,
            bool scalar)
        {
            StreamNumSize = numStreams;
            this.evaluatorForge = evaluatorForge;
            this.scalar = scalar;
        }

        public int StreamNumSize { get; }

        public EnumEval EnumEvaluator => new EnumExceptForgeEval(this, evaluatorForge.ExprEvaluatorEnumeration);

        public CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return EnumExceptForgeEval.Codegen(this, premade, codegenMethodScope, codegenClassScope);
        }
    }
} // end of namespace