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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
	public class CodegenStatementSwitch : CodegenStatementWBlockBase {
	    private readonly string @ref;
	    private readonly int[] options;
	    private readonly CodegenBlock[] blocks;
	    private readonly bool blocksReturnValues;

	    public CodegenStatementSwitch(CodegenBlock parent, string @ref, int[] options, bool blocksReturnValues) : base(parent)
	        {
	        this.@ref = @ref;
	        this.options = options;
	        blocks = new CodegenBlock[options.Length];
	        for (int i = 0; i < options.Length; i++) {
	            blocks[i] = new CodegenBlock(this);
	        }
	        this.blocksReturnValues = blocksReturnValues;
	    }

	    public CodegenBlock[] Blocks
	    {
	        get => blocks;
	    }

	    public override void Render(StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass, int level, CodegenIndent indent) {
	        builder.Append("switch(").Append(@ref).Append(") {\n");

	        for (int i = 0; i < options.Length; i++) {
	            indent.Indent(builder, level + 1);
	            builder.Append("case ").Append(options[i]).Append(": {\n");
	            blocks[i].Render(builder, imports, isInnerClass, level + 2, indent);

	            if (!blocksReturnValues) {
	                indent.Indent(builder, level + 2);
	                builder.Append("break;\n");
	            }

	            indent.Indent(builder, level + 1);
	            builder.Append("}\n");
	        }

	        indent.Indent(builder, level + 1);
	        builder.Append("default: throw new UnsupportedOperationException();\n");

	        indent.Indent(builder, level);
	        builder.Append("}\n");
	    }

	    public override void MergeClasses(ISet<Type> classes) {
	        for (int i = 0; i < blocks.Length; i++) {
	            blocks[i].MergeClasses(classes);
	        }
	    }
	}
} // end of namespace