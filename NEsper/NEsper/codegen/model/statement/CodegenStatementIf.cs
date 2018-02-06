///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementIf : CodegenStatementWBlockBase
    {
        private List<CodegenStatementIfConditionBlock> blocks = new List<CodegenStatementIfConditionBlock>();
        private CodegenBlock optionalElse;

        public CodegenStatementIf(CodegenBlock parent) : base(parent)
        {
        }

        public CodegenBlock OptionalElse
        {
            get => optionalElse;
            set => this.optionalElse = value;
        }

        public override void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            string delimiter = "";
            foreach (CodegenStatementIfConditionBlock pair in blocks)
            {
                builder.Append(delimiter);
                builder.Append("if (");
                pair.Condition.Render(builder, imports);
                builder.Append(") {\n");
                pair.Block.Render(builder, imports);
                builder.Append("}");
                delimiter = "\n";
            }
            if (optionalElse != null)
            {
                builder.Append("else {\n");
                optionalElse.Render(builder, imports);
                builder.Append("}");
            }
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            foreach (CodegenStatementIfConditionBlock pair in blocks)
            {
                pair.MergeClasses(classes);
            }
            if (optionalElse != null)
            {
                optionalElse.MergeClasses(classes);
            }
        }

        public void Add(ICodegenExpression condition, CodegenBlock block)
        {
            blocks.Add(new CodegenStatementIfConditionBlock(condition, block));
        }
    }
} // end of namespace