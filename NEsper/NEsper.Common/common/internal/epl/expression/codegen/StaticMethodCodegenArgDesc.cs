///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

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

        public string BlockRefName {
            get => blockRefName;
        }

        public Type DeclareType {
            get => declareType;
        }

        public CodegenExpression ArgExpression {
            get => argExpression;
        }
    }
} // end of namespace