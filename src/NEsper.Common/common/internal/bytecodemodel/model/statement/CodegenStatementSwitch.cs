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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementSwitch : CodegenStatementWBlockBase
    {
        private readonly CodegenExpression _switchExpression;
        private readonly CodegenExpression[] _options;
        private readonly CodegenBlock[] _blocks;
        private readonly CodegenBlock _defaultBlock;
        private readonly bool _blocksReturnValues;
        private readonly bool _withDefaultUnsupported;

        public CodegenStatementSwitch(
            CodegenBlock parent,
            CodegenExpression switchExpression,
            CodegenExpression[] options,
            bool blocksReturnValues,
            bool withDefaultUnsupported)
            : base(parent)
        {
            _switchExpression = switchExpression;
            _options = options;
            _blocks = new CodegenBlock[options.Length];
            for (int i = 0; i < options.Length; i++) {
                _blocks[i] = new CodegenBlock(this);
            }

            _blocksReturnValues = blocksReturnValues;
            _withDefaultUnsupported = withDefaultUnsupported;
            _defaultBlock = new CodegenBlock(this);
        }

        public CodegenBlock[] Blocks => _blocks;

        public CodegenBlock DefaultBlock => _defaultBlock;

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("switch(");
            _switchExpression.Render(builder, isInnerClass, level, indent);
            builder.Append(") {\n");

            for (int i = 0; i < _options.Length; i++) {
                indent.Indent(builder, level + 1);
                builder.Append("case ");
                _options[i].Render(builder, isInnerClass, level, indent); 
                builder.Append(": {\n");

                _blocks[i].Render(builder, isInnerClass, level + 2, indent);

                if (!_blocksReturnValues) {
                    indent.Indent(builder, level + 2);
                    builder.Append("break;\n");
                }

                indent.Indent(builder, level + 1);
                builder.Append("}\n");
            }

            builder.Append("default: ");
            if (_withDefaultUnsupported) {
                indent.Indent(builder, level + 1);
                builder.Append("throw new UnsupportedOperationException();\n");
            }
            else {
                _defaultBlock.Render(builder, isInnerClass, level + 2, indent);
                if (!_blocksReturnValues) {
                    indent.Indent(builder, level + 2);
                    builder.Append("break;\n");
                }
            }

            indent.Indent(builder, level);
            builder.Append("}\n");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            _switchExpression.MergeClasses(classes);
            _blocks.ForEach(b => b.MergeClasses(classes));
            _options.ForEach(o => o.MergeClasses(classes));
            _defaultBlock?.MergeClasses(classes);
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_switchExpression);
            _blocks.ForEach(b => b.TraverseExpressions(consumer));
            _options.ForEach(o => consumer.Invoke(o));
            _defaultBlock?.TraverseExpressions(consumer);
        }
    }
} // end of namespace