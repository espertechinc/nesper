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
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.codegen.model.statement;

namespace com.espertech.esper.codegen.core
{
    public interface ICodegenBlock
    {
        ICodegenBlock Expression(ICodegenExpression expression);
        ICodegenBlock IfConditionReturnConst(ICodegenExpression condition, Object constant);
        ICodegenBlock IfNotInstanceOf(string name, Type clazz);
        ICodegenBlock IfInstanceOf(string name, Type clazz);
        ICodegenBlock ForLoopInt(string name, ICodegenExpression upperLimit);
        ICodegenBlock BlockElseIf(ICodegenExpression condition);
        ICodegenBlock BlockElse();
        ICodegenBlock DeclareVarWCast(Type clazz, string var, string rhsName);
        ICodegenBlock DeclareVar(Type clazz, string var, ICodegenExpression initializer);
        ICodegenBlock DeclareVarNull(Type clazz, string var);
        ICodegenBlock AssignRef(string @ref, ICodegenExpression assignment);
        ICodegenBlock AssignArrayElement(string @ref, ICodegenExpression index, ICodegenExpression assignment);
        ICodegenBlock ExprDotMethod(ICodegenExpression expression, string method, params ICodegenExpression[] parameters);
        ICodegenBlock IfRefNullReturnFalse(string @ref);
        ICodegenBlock IfRefNotTypeReturnConst(string @ref, Type type, Object constant);
        ICodegenBlock IfRefNullReturnNull(string @ref);
        ICodegenBlock DeclareVarEventPerStreamUnd(Type clazz, int streamNum);
        ICodegenBlock BlockReturn(ICodegenExpression expression);
        ICodegenBlock BlockEnd();
        string MethodReturn(ICodegenExpression expression);
        void Render(TextWriter textWriter);

        IList<ICodegenStatement> Statements { get; }
    }
} // end of namespace