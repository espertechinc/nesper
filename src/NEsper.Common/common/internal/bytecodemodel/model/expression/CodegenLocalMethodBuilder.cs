///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenLocalMethodBuilder
    {
        private readonly CodegenMethod _methodNode;
        private readonly IList<CodegenExpression> _parameters = new List<CodegenExpression>(2);

        public CodegenLocalMethodBuilder(CodegenMethod methodNode)
        {
            _methodNode = methodNode;
        }

        public CodegenLocalMethodBuilder Pass(CodegenExpression expression)
        {
            _parameters.Add(expression);
            return this;
        }

        public CodegenExpression Call()
        {
            return new CodegenExpressionLocalMethod(_methodNode, _parameters);
        }
    }
} // end of namespace