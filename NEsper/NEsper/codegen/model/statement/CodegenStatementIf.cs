///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementIf : CodegenStatementWBlockBase
    {
        private readonly List<CodegenStatementIfConditionBlock> _blocks = new List<CodegenStatementIfConditionBlock>();
        private CodegenBlock _optionalElse;

        public CodegenStatementIf(CodegenBlock parent) : base(parent)
        {
        }

        public CodegenBlock OptionalElse
        {
            get => _optionalElse;
            set => this._optionalElse = value;
        }

        public override void Render(TextWriter textWriter)
        {
            string delimiter = "";
            foreach (CodegenStatementIfConditionBlock pair in _blocks)
            {
                textWriter.Write(delimiter);
                textWriter.Write("if (");
                pair.Condition.Render(textWriter);
                textWriter.WriteLine(") {{");
                pair.Block.Render(textWriter);
                textWriter.WriteLine("}}");
                delimiter = "\n";
            }
            if (_optionalElse != null)
            {
                textWriter.Write("else {\n");
                _optionalElse.Render(textWriter);
                textWriter.WriteLine("}}");
            }
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            foreach (CodegenStatementIfConditionBlock pair in _blocks)
            {
                pair.MergeClasses(classes);
            }
            if (_optionalElse != null)
            {
                _optionalElse.MergeClasses(classes);
            }
        }

        public void Add(ICodegenExpression condition, CodegenBlock block)
        {
            _blocks.Add(new CodegenStatementIfConditionBlock(condition, block));
        }
    }
} // end of namespace