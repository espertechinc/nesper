using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using com.espertech.esper.codegen.model.expression;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementAssignArrayElement : CodegenStatementBase
    {
        private readonly ICodegenExpression _expression;
        private readonly ICodegenExpression _index;
        private readonly string _name;

        public CodegenStatementAssignArrayElement(string name, ICodegenExpression index, ICodegenExpression expression)
        {
            _name = name;
            _index = index;
            _expression = expression;
        }

        public override void RenderStatement(TextWriter textWriter)
        {
            textWriter.Write(_name);
            textWriter.Write("[");
            _index.Render(textWriter);
            textWriter.Write("]=");
            _expression.Render(textWriter);
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            _index.MergeClasses(classes);
            _expression.MergeClasses(classes);
        }
    }
} // end of namespace