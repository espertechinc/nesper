///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.epl.enummethod.codegen
{
    public class EnumForgeCodegenParams
    {
        public EnumForgeCodegenParams(
            CodegenExpression eps,
            CodegenExpression enumcoll,
            Type enumcollType,
            CodegenExpression isNewData,
            CodegenExpression exprCtx,
            Type desiredReturnType)
        {
            Eps = eps;
            Enumcoll = enumcoll;
            EnumcollType = enumcollType;
            IsNewData = isNewData;
            ExprCtx = exprCtx;
            DesiredReturnType = desiredReturnType;
        }

        public CodegenExpression Eps { get; }

        public CodegenExpression Enumcoll { get; }

        public Type EnumcollType { get; }
        
        public Type DesiredReturnType { get; set; }

        public CodegenExpression IsNewData { get; }

        public CodegenExpression ExprCtx { get; }

        public CodegenExpression[] Expressions => new[] {
            Eps, Enumcoll, IsNewData, ExprCtx
        };
    }
} // end of namespace