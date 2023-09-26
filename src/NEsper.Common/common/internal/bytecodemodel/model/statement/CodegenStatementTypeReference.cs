///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementTypeReference : CodegenStatementBase
    {
        private readonly Type _type;

        public CodegenStatementTypeReference(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
        }


        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            if (_type != null) {
                classes.AddToSet(_type);
            }
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
        }
    }
} // end of namespace