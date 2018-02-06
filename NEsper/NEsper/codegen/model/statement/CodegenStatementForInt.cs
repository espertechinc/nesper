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
    public class CodegenStatementForInt : CodegenStatementWBlockBase
    {
        private readonly string _ref;
        private readonly ICodegenExpression _upperLimit;
        private CodegenBlock _block;

        public CodegenStatementForInt(CodegenBlock parent, string @ref, ICodegenExpression upperLimit)
            : base(parent)
        {
            this._ref = @ref;
            this._upperLimit = upperLimit;
        }

        public CodegenBlock Block
        {
            get => _block;
            set => _block = value;
        }

        public override void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            builder.Append("for (int ").Append(_ref).Append("=0; i<");
            _upperLimit.Render(builder, imports);
            builder.Append("; i++) {\n");
            _block.Render(builder, imports);
            builder.Append("}\n");
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            _block.MergeClasses(classes);
            _upperLimit.MergeClasses(classes);
        }
    }
} // end of namespace