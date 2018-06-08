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
    public class CodegenExpressionBuilder : ICodegenExpressionBuilder
    {
        public static ICodegenExpression Ref(string @ref)
        {
            return new CodegenExpressionRef(@ref);
        }

        ICodegenExpression ICodegenExpressionBuilder.Ref(string @ref)
        {
            return Ref(@ref);
        }
        
        public static ICodegenExpressionExprDotName ExprDotName(ICodegenExpression left, string name)
        {
            return new CodegenExpressionExprDotName(left, name);
        }

        ICodegenExpressionExprDotName ICodegenExpressionBuilder.ExprDotName(ICodegenExpression left, string name)
        {
            return ExprDotName(left, name);
        }

        public static ICodegenExpression ExprDotMethod(ICodegenExpression expression, string method, params ICodegenExpression[] parameters)
        {
            return new CodegenExpressionExprDotMethod(expression, method, parameters);
        }

        ICodegenExpression ICodegenExpressionBuilder.ExprDotMethod(ICodegenExpression expression, string method, params ICodegenExpression[] parameters)
        {
            return ExprDotMethod(expression, method, parameters);
        }

        public static ICodegenExpressionExprDotMethodChain ExprDotMethodChain(ICodegenExpression expression)
        {
            return new CodegenExpressionExprDotMethodChain(expression);
        }

        ICodegenExpressionExprDotMethodChain ICodegenExpressionBuilder.ExprDotMethodChain(ICodegenExpression expression)
        {
            return ExprDotMethodChain(expression);
        }

        public static ICodegenExpression ExprDotUnderlying(ICodegenExpression expression)
        {
            return new CodegenExpressionExprDotUnderlying(expression);
        }

        ICodegenExpression ICodegenExpressionBuilder.ExprDotUnderlying(ICodegenExpression expression)
        {
            return ExprDotUnderlying(expression);
        }

        public static ICodegenExpression BeanUndCastDotMethodConst(Type clazz, ICodegenExpression beanExpression, string method, string constant)
        {
            return new CodegenExpressionBeanUndCastDotMethodConst(clazz, beanExpression, method, constant);
        }

        ICodegenExpression ICodegenExpressionBuilder.BeanUndCastDotMethodConst(
            Type clazz, ICodegenExpression beanExpression, string method, string constant)
        {
            return BeanUndCastDotMethodConst(clazz, beanExpression, method, constant);
        }

        public static ICodegenExpression BeanUndCastArrayAtIndex(Type clazz, ICodegenExpression beanExpression, int index)
        {
            return new CodegenExpressionBeanUndCastArrayAtIndex(clazz, beanExpression, index);
        }

        ICodegenExpression ICodegenExpressionBuilder.BeanUndCastArrayAtIndex(Type clazz, ICodegenExpression beanExpression, int index)
        {
            return BeanUndCastArrayAtIndex(clazz, beanExpression, index);
        }

        public static ICodegenExpression LocalMethod(string methodName, ICodegenExpression expression)
        {
            return new CodegenExpressionLocalMethod(methodName, expression);
        }

        ICodegenExpression ICodegenExpressionBuilder.LocalMethod(string methodName, ICodegenExpression expression)
        {
            return LocalMethod(methodName, expression);
        }

        public static ICodegenExpression ConstantTrue()
        {
            return CodegenExpressionConstantTrue.INSTANCE;
        }

        ICodegenExpression ICodegenExpressionBuilder.ConstantTrue()
        {
            return ConstantTrue();
        }

        public static ICodegenExpression ConstantFalse()
        {
            return CodegenExpressionConstantFalse.INSTANCE;
        }

        ICodegenExpression ICodegenExpressionBuilder.ConstantFalse()
        {
            return ConstantFalse();
        }

        public static ICodegenExpression ConstantNull()
        {
            return CodegenExpressionConstantNull.INSTANCE;
        }

        ICodegenExpression ICodegenExpressionBuilder.ConstantNull()
        {
            return ConstantNull();
        }

        public static ICodegenExpression Constant(object constant)
        {
            return new CodegenExpressionConstant(constant);
        }

        ICodegenExpression ICodegenExpressionBuilder.Constant(object constant)
        {
            return Constant(constant);
        }

        public static ICodegenExpression CastUnderlying(Type clazz, ICodegenExpression expression)
        {
            return new CodegenExpressionCastUnderlying(clazz, expression);
        }

        ICodegenExpression ICodegenExpressionBuilder.CastUnderlying(Type clazz, ICodegenExpression expression)
        {
            return CastUnderlying(clazz, expression);
        }

        public static ICodegenExpression InstanceOf(ICodegenExpression lhs, Type clazz)
        {
            return new CodegenExpressionInstanceOf(lhs, clazz, false);
        }

        ICodegenExpression ICodegenExpressionBuilder.InstanceOf(ICodegenExpression lhs, Type clazz)
        {
            return InstanceOf(lhs, clazz);
        }

        public static ICodegenExpression NotInstanceOf(ICodegenExpression lhs, Type clazz)
        {
            return new CodegenExpressionInstanceOf(lhs, clazz, true);
        }

        ICodegenExpression ICodegenExpressionBuilder.NotInstanceOf(ICodegenExpression lhs, Type clazz)
        {
            return NotInstanceOf(lhs, clazz);
        }

        public static ICodegenExpression CastRef(Type clazz, string @ref)
        {
            return new CodegenExpressionCastRef(clazz, @ref);
        }

        ICodegenExpression ICodegenExpressionBuilder.CastRef(Type clazz, string @ref)
        {
            return CastRef(clazz, @ref);
        }

        public static ICodegenExpression Conditional(ICodegenExpression condition, ICodegenExpression expressionTrue, ICodegenExpression expressionFalse)
        {
            return new CodegenExpressionConditional(condition, expressionTrue, expressionFalse);
        }

        ICodegenExpression ICodegenExpressionBuilder.Conditional(
            ICodegenExpression condition, ICodegenExpression expressionTrue, ICodegenExpression expressionFalse)
        {
            return Conditional(condition, expressionTrue, expressionFalse);
        }

        public static ICodegenExpression Not(ICodegenExpression expression)
        {
            return new CodegenExpressionNot(expression);
        }

        ICodegenExpression ICodegenExpressionBuilder.Not(ICodegenExpression expression)
        {
            return Not(expression);
        }

        public static ICodegenExpression Cast(Type clazz, ICodegenExpression expression)
        {
            return new CodegenExpressionCastExpression(clazz, expression);
        }

        ICodegenExpression ICodegenExpressionBuilder.Cast(Type clazz, ICodegenExpression expression)
        {
            return Cast(clazz, expression);
        }

        public static ICodegenExpression NotEqualsNull(ICodegenExpression lhs)
        {
            return new CodegenExpressionEqualsNull(lhs, true);
        }

        ICodegenExpression ICodegenExpressionBuilder.NotEqualsNull(ICodegenExpression lhs)
        {
            return NotEqualsNull(lhs);
        }

        public static ICodegenExpression EqualsNull(ICodegenExpression lhs)
        {
            return new CodegenExpressionEqualsNull(lhs, false);
        }

        ICodegenExpression ICodegenExpressionBuilder.EqualsNull(ICodegenExpression lhs)
        {
            return EqualsNull(lhs);
        }

        public static ICodegenExpression StaticMethod(Type clazz, string method, params string[] refs)
        {
            return new CodegenExpressionStaticMethodTakingRefs(clazz, method, refs);
        }

        ICodegenExpression ICodegenExpressionBuilder.StaticMethod(Type clazz, string method, params string[] refs)
        {
            return StaticMethod(clazz, method, refs);
        }

        public static ICodegenExpression StaticMethod(Type clazz, string method, params ICodegenExpression[] parameters)
        {
            return new CodegenExpressionStaticMethodTakingAny(clazz, method, parameters);
        }

        ICodegenExpression ICodegenExpressionBuilder.StaticMethod(Type clazz, string method, params ICodegenExpression[] parameters)
        {
            return StaticMethod(clazz, method, parameters);
        }

        public static ICodegenExpression StaticMethodTakingExprAndConst(Type clazz, string method, ICodegenExpression expression, params object[] consts)
        {
            return new CodegenExpressionStaticMethodTakingExprAndConst(clazz, method, expression, consts);
        }

        ICodegenExpression ICodegenExpressionBuilder.StaticMethodTakingExprAndConst(
            Type clazz, string method, ICodegenExpression expression, params object[] consts)
        {
            return StaticMethodTakingExprAndConst(clazz, method, expression, consts);
        }

        public static ICodegenExpression ArrayAtIndex(ICodegenExpression expression, ICodegenExpression index)
        {
            return new CodegenExpressionArrayAtIndex(expression, index);
        }

        ICodegenExpression ICodegenExpressionBuilder.ArrayAtIndex(ICodegenExpression expression, ICodegenExpression index)
        {
            return ArrayAtIndex(expression, index);
        }

        public static ICodegenExpression ArrayLength(ICodegenExpression expression)
        {
            return new CodegenExpressionArrayLength(expression);
        }

        ICodegenExpression ICodegenExpressionBuilder.ArrayLength(ICodegenExpression expression)
        {
            return ArrayLength(expression);
        }

        public static ICodegenExpression NewInstance(Type clazz, params ICodegenExpression[] parameters)
        {
            return new CodegenExpressionNewInstance(clazz, parameters);
        }

        ICodegenExpression ICodegenExpressionBuilder.NewInstance(Type clazz, params ICodegenExpression[] parameters)
        {
            return NewInstance(clazz, parameters);
        }

        public static ICodegenExpression Relational(ICodegenExpression lhs, CodegenRelational op, ICodegenExpression rhs)
        {
            return new CodegenExpressionRelational(lhs, op, rhs);
        }

        ICodegenExpression ICodegenExpressionBuilder.Relational(ICodegenExpression lhs, CodegenRelational op, ICodegenExpression rhs)
        {
            return Relational(lhs, op, rhs);
        }

        public static ICodegenExpression NewArray(Type component, ICodegenExpression expression)
        {
            return new CodegenExpressionNewArray(component, expression);
        }

        ICodegenExpression ICodegenExpressionBuilder.NewArray(Type component, ICodegenExpression expression)
        {
            return NewArray(component, expression);
        }

        public static void RenderExpressions(TextWriter textWriter, ICodegenExpression[] expressions)
        {
            string delimiter = "";
            foreach (ICodegenExpression expression in expressions)
            {
                textWriter.Write(delimiter);
                expression.Render(textWriter);
                delimiter = ",";
            }
        }

        void ICodegenExpressionBuilder.RenderExpressions(TextWriter textWriter, ICodegenExpression[] expressions)
        {
            RenderExpressions(textWriter, expressions);
        }

        public static void MergeClassesExpressions(ICollection<Type> classes, ICodegenExpression[] expressions)
        {
            foreach (ICodegenExpression expression in expressions)
            {
                expression.MergeClasses(classes);
            }
        }

        void ICodegenExpressionBuilder.MergeClassesExpressions(ICollection<Type> classes, ICodegenExpression[] expressions)
        {
            MergeClassesExpressions(classes, expressions);
        }
    }
} // end of namespace