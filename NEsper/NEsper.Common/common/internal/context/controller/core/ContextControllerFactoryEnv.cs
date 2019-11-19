///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.core
{
    public class ContextControllerFactoryEnv
    {
        public ContextControllerFactoryEnv(
            string outermostContextName,
            string contextName,
            int nestingLevel,
            int numNestingLevels)
        {
            OutermostContextName = outermostContextName;
            ContextName = contextName;
            NestingLevel = nestingLevel;
            NumNestingLevels = numNestingLevels;
        }

        public string OutermostContextName { get; }

        public string ContextName { get; }

        public int NestingLevel { get; }

        public int NumNestingLevels { get; }

        public bool IsLeaf => NestingLevel == NumNestingLevels;

        public bool IsRoot => NumNestingLevels == 1 || NestingLevel == 1;

        public CodegenExpression ToExpression()
        {
            return NewInstance(
                GetType(),
                Constant(OutermostContextName),
                Constant(ContextName),
                Constant(NestingLevel),
                Constant(NumNestingLevels));
        }
    }
} // end of namespace