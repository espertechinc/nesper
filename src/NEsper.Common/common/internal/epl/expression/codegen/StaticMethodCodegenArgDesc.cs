///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.epl.expression.codegen
{
    public class StaticMethodCodegenArgDesc
    {
        private readonly string blockRefName;
        private readonly Type declareType;
        private readonly CodegenExpression argExpression;

        public StaticMethodCodegenArgDesc(
            string blockRefName,
            Type declareType,
            CodegenExpression argExpression)
        {
            this.blockRefName = blockRefName;
            this.declareType = declareType;
            this.argExpression = argExpression;
        }

        public string BlockRefName => blockRefName;

        public Type DeclareType => declareType;

        public CodegenExpression ArgExpression => argExpression;
    }
} // end of namespace