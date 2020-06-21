///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementTryCatchCatchBlock
    {
        public CodegenStatementTryCatchCatchBlock(
            Type ex,
            string name,
            CodegenBlock block)
        {
            Ex = ex;
            Name = name;
            Block = block;
        }

        public Type Ex { get; }

        public string Name { get; }

        public CodegenBlock Block { get; }

        internal void MergeClasses(ISet<Type> classes)
        {
            classes.AddToSet(Ex);
            Block.MergeClasses(classes);
        }
        
        public void TraverseExpressions(Consumer<CodegenExpression> consumer) {
            Block.TraverseExpressions(consumer);
        }
    }
} // end of namespace