using System;
using System.Collections.Generic;
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

        public override void RenderStatement(StringBuilder builder, IDictionary<Type, string> imports)
        {
            builder.Append(_name).Append("[");
            _index.Render(builder, imports);
            builder.Append("]=");
            _expression.Render(builder, imports);
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            _index.MergeClasses(classes);
            _expression.MergeClasses(classes);
        }
    }
} // end of namespace