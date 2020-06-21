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

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementForEach : CodegenStatementWBlockBase
    {
        private readonly string _ref;
        private readonly CodegenExpression _target;
        private readonly Type _type;
        private readonly bool _useVar;

        public CodegenStatementForEach(
            CodegenBlock parent,
            Type type,
            string @ref,
            CodegenExpression target)
            : base(parent)
        {
            _useVar = false;
            _type = type;
            _ref = @ref;
            _target = target;
        }

        public CodegenStatementForEach(
            CodegenBlock parent,
            string @ref,
            CodegenExpression target)
            : base(parent)
        {
            _useVar = true;
            _type = null;
            _ref = @ref;
            _target = target;
        }

        public CodegenBlock Block { get; set; }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("foreach (");
            if (_useVar) {
                builder.Append("var");
            }
            else {
                AppendClassName(builder, _type);
            }

            builder.Append(" ").Append(_ref).Append(" in ");
            _target.Render(builder, isInnerClass, level, indent);
            builder.Append(") {\n");
            Block.Render(builder, isInnerClass, level + 1, indent);
            indent.Indent(builder, level);
            builder.Append("}\n");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            if (!_useVar) {
                classes.AddToSet(_type);
            }

            Block.MergeClasses(classes);
            _target.MergeClasses(classes);
        }
        
        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_target);
            Block.TraverseExpressions(consumer);
        }
    }
} // end of namespace