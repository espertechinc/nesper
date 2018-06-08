///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementIfConditionBlock
    {
        private readonly ICodegenExpression _condition;
        private readonly CodegenBlock _block;

        public CodegenStatementIfConditionBlock(ICodegenExpression condition, CodegenBlock block)
        {
            this._condition = condition;
            this._block = block;
        }

        public ICodegenExpression Condition => _condition;

        public CodegenBlock Block => _block;

        internal void MergeClasses(ICollection<Type> classes)
        {
            _condition.MergeClasses(classes);
            _block.MergeClasses(classes);
        }
    }
} // end of namespace