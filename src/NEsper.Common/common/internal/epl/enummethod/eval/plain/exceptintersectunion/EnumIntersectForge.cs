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
    public class EnumIntersectForge : EnumForge
    {
        internal readonly int numStreams;
        internal readonly ExprEnumerationForge evaluatorForge;
        internal readonly bool scalar;

        public EnumIntersectForge(
            int numStreams,
            ExprEnumerationForge evaluatorForge,
            bool scalar)
        {
            this.numStreams = numStreams;
            this.evaluatorForge = evaluatorForge;
            this.scalar = scalar;
        }

        public int StreamNumSize {
            get => numStreams;
        }

        public virtual EnumEval EnumEvaluator {
            get => new EnumIntersectForgeEval(this, evaluatorForge.ExprEvaluatorEnumeration);
        }

        public virtual CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return EnumIntersectForgeEval.Codegen(this, premade, codegenMethodScope, codegenClassScope);
        }
    }
} // end of namespace