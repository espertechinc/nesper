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

//import static com.espertech.esper.codegen.core.CodeGenerationHelper.appendClassName;
//import static com.espertech.esper.codegen.model.expression.CodegenExpressionUtil.renderConstant;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementIfRefNotTypeReturnConst : CodegenStatementBase
    {
        private readonly string _var;
        private readonly Type _type;
        private readonly Object _constant;

        public CodegenStatementIfRefNotTypeReturnConst(string var, Type type, Object constant)
        {
            this._var = var;
            this._type = type;
            this._constant = constant;
        }

        public override void RenderStatement(TextWriter textWriter)
        {
            textWriter.Write("if (!(");
            textWriter.Write(_var);
            textWriter.Write(" is ");
            CodeGenerationHelper.AppendClassName(textWriter, _type, null);
            textWriter.Write(")) return ");
            CodegenExpressionUtil.RenderConstant(textWriter, _constant);
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(_type);
        }
    }
} // end of namespace