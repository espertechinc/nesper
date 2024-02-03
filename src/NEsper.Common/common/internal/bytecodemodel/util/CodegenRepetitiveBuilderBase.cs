///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.util
{
    public abstract class CodegenRepetitiveBuilderBase
    {
        protected readonly CodegenMethod methodNode;
        protected readonly CodegenClassScope classScope;
        protected readonly Type provider;
        protected readonly IList<CodegenNamedParam> @params = new List<CodegenNamedParam>(2);

        public abstract void Build();

        public CodegenRepetitiveBuilderBase(
            CodegenMethod methodNode,
            CodegenClassScope classScope,
            Type provider)
        {
            this.methodNode = methodNode;
            this.classScope = classScope;
            this.provider = provider;
        }

        internal static int TargetMethodComplexity(CodegenClassScope classScope)
        {
            return Math.Max(1, classScope.NamespaceScope.Config.InternalUseOnlyMaxMethodComplexity);
        }

        protected CodegenExpression[] ParamNames()
        {
            var names = new CodegenExpression[@params.Count];
            for (var i = 0; i < @params.Count; i++) {
                names[i] = Ref(@params[i].Name);
            }

            return names;
        }
    }
} // end of namespace