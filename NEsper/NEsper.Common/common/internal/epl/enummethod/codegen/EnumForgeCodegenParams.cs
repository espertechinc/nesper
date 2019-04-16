///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.epl.enummethod.codegen
{
    public class EnumForgeCodegenParams
    {
        public EnumForgeCodegenParams(
            CodegenExpression eps,
            CodegenExpression enumcoll,
            CodegenExpression isNewData,
            CodegenExpression exprCtx)
        {
            Eps = eps;
            Enumcoll = enumcoll;
            IsNewData = isNewData;
            ExprCtx = exprCtx;
        }

        public CodegenExpression Eps { get; }

        public CodegenExpression Enumcoll { get; }

        public CodegenExpression IsNewData { get; }

        public CodegenExpression ExprCtx { get; }

        public CodegenExpression[] Expressions => new[] {Eps, Enumcoll, IsNewData, ExprCtx};
    }
} // end of namespace