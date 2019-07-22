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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionBuilder
    {
        public static CodegenExpressionRef Ref(string @ref)
        {
            return new CodegenExpressionRef(@ref);
        }

        public static CodegenExpressionRefWCol RefCol(
            string @ref,
            int column)
        {
            return new CodegenExpressionRefWCol(@ref, column);
        }

        public static CodegenExpression Op(
            CodegenExpression left,
            string expressionText,
            CodegenExpression right)
        {
            return new CodegenExpressionOp(left, expressionText, right);
        }

        public static CodegenExpression And(
            CodegenExpression first,
            CodegenExpression second,
            params CodegenExpression[] more)
        {
            return new CodegenExpressionAndOr(true, first, second, more);
        }

        public static CodegenExpression Or(
            CodegenExpression first,
            CodegenExpression second,
            params CodegenExpression[] more)
        {
            return new CodegenExpressionAndOr(false, first, second, more);
        }

        public static CodegenExpressionExprDotName ExprDotName(
            CodegenExpression left,
            string name)
        {
            return new CodegenExpressionExprDotName(left, name);
        }

        public static CodegenExpression ExprDotMethod(
            CodegenExpression expression,
            string method,
            params CodegenExpression[] @params)
        {
            return new CodegenExpressionExprDotMethod(expression, method, @params);
        }

        public static CodegenExpression GetProperty(
            CodegenExpression expression,
            string property)
        {
            return new CodegenExpressionExprDotName(expression, property);
        }

        public static CodegenExpression SetProperty(
            CodegenExpression expression,
            string property,
            CodegenExpression value)
        {
            return new CodegenExpressionAssign(
                new CodegenExpressionExprDotName(expression, property),
                value);
        }

        public static CodegenExpression EnumValue<T>(T enumValue)
            where T : struct
        {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException("type is not an enumeration");
            }

            return EnumValue(typeof(T), enumValue.GetName());
        }

        public static CodegenExpression EnumValue(
            Type enumType,
            string enumValue)
        {
            return new CodegenExpressionEnumOrPublicConstantValue(enumType, enumValue);
        }

        public static CodegenExpression PublicConstValue(
            Type enumType,
            string enumValue)
        {
            return new CodegenExpressionEnumOrPublicConstantValue(enumType, enumValue);
        }

        public static CodegenExpression PublicConstValue(
            string enumType,
            string enumValue)
        {
            return new CodegenExpressionEnumOrPublicConstantValue(enumType, enumValue);
        }

        public static CodegenExpressionExprDotMethodChain ExprDotMethodChain(CodegenExpression expression)
        {
            return new CodegenExpressionExprDotMethodChain(expression);
        }

        public static CodegenExpression ExprDotUnderlying(CodegenExpression expression)
        {
            return new CodegenExpressionExprDotUnderlying(expression);
        }

        public static CodegenLocalMethodBuilder LocalMethodBuild(CodegenMethod methodNode)
        {
            return new CodegenLocalMethodBuilder(methodNode);
        }

        public static CodegenExpressionLocalMethod LocalMethod(
            CodegenMethod methodNode,
            params CodegenExpression[] parameters)
        {
            return new CodegenExpressionLocalMethod(methodNode, parameters);
        }

        public static CodegenExpression ConstantTrue()
        {
            return CodegenExpressionConstantTrue.INSTANCE;
        }

        public static CodegenExpression ConstantFalse()
        {
            return CodegenExpressionConstantFalse.INSTANCE;
        }

        public static CodegenExpression ConstantNull()
        {
            return CodegenExpressionConstantNull.INSTANCE;
        }

        public static CodegenExpression Constant(object constant)
        {
            return new CodegenExpressionConstant(constant);
        }

        public static CodegenExpressionField Field(CodegenField field)
        {
            return new CodegenExpressionField(field);
        }

        public static CodegenExpressionNewAnonymousClass NewAnonymousClass(
            CodegenBlock parentBlock,
            Type interfaceOrSuperClass,
            IList<CodegenExpression> ctorParams)
        {
            return new CodegenExpressionNewAnonymousClass(parentBlock, interfaceOrSuperClass, ctorParams);
        }

        public static CodegenExpressionNewAnonymousClass NewAnonymousClass(
            CodegenBlock parentBlock,
            Type interfaceOrSuperClass)
        {
            return new CodegenExpressionNewAnonymousClass(
                parentBlock,
                interfaceOrSuperClass,
                Collections.GetEmptyList<CodegenExpression>());
        }

        public static CodegenExpression Noop()
        {
            return CodegenExpressionNoOp.INSTANCE;
        }

        public static CodegenExpression CastUnderlying(
            Type clazz,
            CodegenExpression expression)
        {
            return new CodegenExpressionCastUnderlying(clazz, expression);
        }

        public static CodegenExpression InstanceOf(
            CodegenExpression lhs,
            Type clazz)
        {
            return new CodegenExpressionInstanceOf(lhs, clazz, false);
        }

        public static CodegenExpression InstanceOf<T>(
            CodegenExpression lhs)

        {
            return new CodegenExpressionInstanceOf(lhs, typeof(T), false);
        }


        public static CodegenExpression NotInstanceOf(
            CodegenExpression lhs,
            Type clazz)
        {
            return new CodegenExpressionInstanceOf(lhs, clazz, true);
        }

        public static CodegenExpression CastRef(
            Type clazz,
            string @ref)
        {
            return new CodegenExpressionCastRef(clazz, @ref);
        }

        public static CodegenExpression Increment(string @ref)
        {
            return new CodegenExpressionIncrementDecrementName(@ref, true);
        }

        public static CodegenExpression Increment(CodegenExpressionRef @ref)
        {
            return new CodegenExpressionIncrementDecrementRef(@ref, true);
        }

        public static CodegenExpression Decrement(string @ref)
        {
            return new CodegenExpressionIncrementDecrementName(@ref, false);
        }

        public static CodegenExpression Decrement(CodegenExpressionRef @ref)
        {
            return new CodegenExpressionIncrementDecrementRef(@ref, false);
        }

        public static CodegenExpression Conditional(
            CodegenExpression condition,
            CodegenExpression expressionTrue,
            CodegenExpression expressionFalse)
        {
            return new CodegenExpressionConditional(condition, expressionTrue, expressionFalse);
        }

        public static CodegenExpression Not(CodegenExpression expression)
        {
            return new CodegenExpressionNot(true, expression);
        }

        public static CodegenExpression NotOptional(
            bool isNot,
            CodegenExpression expression)
        {
            return new CodegenExpressionNot(isNot, expression);
        }

        public static CodegenExpression Cast<T>(
            CodegenExpression expression)
        {
            return new CodegenExpressionCastExpression(typeof(T), expression);
        }

        public static CodegenExpression Cast(
            Type clazz,
            CodegenExpression expression)
        {
            return new CodegenExpressionCastExpression(clazz, expression);
        }

        public static CodegenExpression Cast(
            string typeName,
            CodegenExpression expression)
        {
            return new CodegenExpressionCastExpression(typeName, expression);
        }

        public static CodegenExpression NotEqualsNull(CodegenExpression lhs)
        {
            return new CodegenExpressionEqualsNull(lhs, true);
        }

        public static CodegenExpression EqualsNull(CodegenExpression lhs)
        {
            return new CodegenExpressionEqualsNull(lhs, false);
        }

        public static CodegenExpression EqualsIdentity(
            CodegenExpression lhs,
            CodegenExpression rhs)
        {
            return new CodegenExpressionEqualsReference(lhs, rhs, false);
        }

        public static CodegenExpression StaticMethod<T>(
            string method,
            params CodegenExpression[] @params)
        {
            return new CodegenExpressionStaticMethod(typeof(T), method, @params);
        }

        public static CodegenExpression StaticMethod(
            Type clazz,
            string method,
            params CodegenExpression[] @params)
        {
            return new CodegenExpressionStaticMethod(clazz, method, @params);
        }

        public static CodegenExpression StaticMethod(
            string clazz,
            string method,
            params CodegenExpression[] @params)
        {
            return new CodegenExpressionStaticMethod(clazz, method, @params);
        }

        public static CodegenExpression ClassMethod(
            string method,
            params CodegenExpression[] @params)
        {
            return new CodegenExpressionClassMethod(method, @params);
        }

        public static CodegenExpression Clazz(Type clazz)
        {
            return new CodegenExpressionClass(clazz);
        }

        public static CodegenExpression ArrayAtIndex(
            CodegenExpression expression,
            CodegenExpression index)
        {
            return new CodegenExpressionArrayAtIndex(expression, index);
        }

        public static CodegenExpression Assign(
            CodegenExpression lhs,
            CodegenExpression rhs)
        {
            return new CodegenExpressionAssign(lhs, rhs);
        }

        public static CodegenExpression ArrayLength(CodegenExpression expression)
        {
            return new CodegenExpressionArrayLength(expression);
        }

        public static CodegenExpression NewInstance<T>(
            params CodegenExpression[] @params)
        {
            return new CodegenExpressionNewInstance(typeof(T), @params);
        }

        public static CodegenExpression NewInstance(
            Type clazz,
            params CodegenExpression[] @params)
        {
            return new CodegenExpressionNewInstance(clazz, @params);
        }

        public static CodegenExpression NewInstance(
            string name,
            params CodegenExpression[] @params)
        {
            return new CodegenExpressionNewInstanceInnerClass(name, @params);
        }

        public static CodegenExpression Relational(
            CodegenExpression lhs,
            CodegenExpressionRelational.CodegenRelational op,
            CodegenExpression rhs)
        {
            return new CodegenExpressionRelational(lhs, op, rhs);
        }

        public static CodegenExpression NewArrayByLength(
            Type component,
            CodegenExpression expression)
        {
            return new CodegenExpressionNewArrayByLength(component, expression);
        }

        public static CodegenExpression NewArrayWithInit(
            Type component,
            params CodegenExpression[] expressions)
        {
            return new CodegenExpressionNewArrayWithInit(component, expressions);
        }

        public static void RenderExpressions(
            StringBuilder builder,
            CodegenExpression[] expressions,
            bool isInnerClass)
        {
            var delimiter = "";
            foreach (var expression in expressions) {
                builder.Append(delimiter);
                expression.Render(builder, isInnerClass);
                delimiter = ",";
            }
        }

        public static void MergeClassesExpressions(
            ISet<Type> classes,
            CodegenExpression[] expressions)
        {
            foreach (var expression in expressions) {
                expression.MergeClasses(classes);
            }
        }
    }
} // end of namespace