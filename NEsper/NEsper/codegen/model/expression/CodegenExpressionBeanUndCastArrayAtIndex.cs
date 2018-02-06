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

// import static com.espertech.esper.codegen.core.CodeGenerationHelper.appendClassName;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionBeanUndCastArrayAtIndex : ICodegenExpression
    {
        private readonly Type _clazz;
        private readonly ICodegenExpression _ref;
        private readonly int _index;

        public CodegenExpressionBeanUndCastArrayAtIndex(Type clazz, ICodegenExpression @ref, int index)
        {
            this._clazz = clazz;
            this._ref = @ref;
            this._index = index;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            builder.Append("((");
            CodeGenerationHelper.AppendClassName(builder, _clazz, null, imports);
            builder.Append(")");
            _ref.Render(builder, imports);
            builder.Append(".Underlying)");
            builder.Append("[").Append(_index).Append("]");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            _ref.MergeClasses(classes);
            classes.Add(_clazz);
        }
    }
} // end of namespace