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

namespace com.espertech.esper.codegen.model.expression
{
    public interface ICodegenExpressionBuilder
    {
        ICodegenExpression Ref(string @ref);
        ICodegenExpressionExprDotName ExprDotName(ICodegenExpression left, string name);
        ICodegenExpression ExprDotMethod(ICodegenExpression expression, string method, params ICodegenExpression[] parameters);
        ICodegenExpressionExprDotMethodChain ExprDotMethodChain(ICodegenExpression expression);
        ICodegenExpression ExprDotUnderlying(ICodegenExpression expression);
        ICodegenExpression BeanUndCastDotMethodConst(Type clazz, ICodegenExpression beanExpression, string method, string constant);
        ICodegenExpression BeanUndCastArrayAtIndex(Type clazz, ICodegenExpression beanExpression, int index);
        ICodegenExpression LocalMethod(string methodName, ICodegenExpression expression);
        ICodegenExpression ConstantTrue();
        ICodegenExpression ConstantFalse();
        ICodegenExpression ConstantNull();
        ICodegenExpression Constant(object constant);
        ICodegenExpression CastUnderlying(Type clazz, ICodegenExpression expression);
        ICodegenExpression InstanceOf(ICodegenExpression lhs, Type clazz);
        ICodegenExpression NotInstanceOf(ICodegenExpression lhs, Type clazz);
        ICodegenExpression CastRef(Type clazz, string @ref);
        ICodegenExpression Conditional(ICodegenExpression condition, ICodegenExpression expressionTrue, ICodegenExpression expressionFalse);
        ICodegenExpression Not(ICodegenExpression expression);
        ICodegenExpression Cast(Type clazz, ICodegenExpression expression);
        ICodegenExpression NotEqualsNull(ICodegenExpression lhs);
        ICodegenExpression EqualsNull(ICodegenExpression lhs);
        ICodegenExpression StaticMethod(Type clazz, string method, params string[] refs);
        ICodegenExpression StaticMethod(Type clazz, string method, params ICodegenExpression[] parameters);
        ICodegenExpression StaticMethodTakingExprAndConst(Type clazz, string method, ICodegenExpression expression, params object[] consts);
        ICodegenExpression ArrayAtIndex(ICodegenExpression expression, ICodegenExpression index);
        ICodegenExpression ArrayLength(ICodegenExpression expression);
        ICodegenExpression NewInstance(Type clazz, params ICodegenExpression[] parameters);
        ICodegenExpression Relational(ICodegenExpression lhs, CodegenRelational op, ICodegenExpression rhs);
        ICodegenExpression NewArray(Type component, ICodegenExpression expression);
        void RenderExpressions(TextWriter textWriter, ICodegenExpression[] expressions);
        void MergeClassesExpressions(ICollection<Type> classes, ICodegenExpression[] expressions);

    }
}