///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.util;

namespace com.espertech.esper.linq
{
    using Expression = client.soda.Expression;

    public class LinqToSoda
    {
        /// <summary>
        /// Converts a LINQ expression to a SODA expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static Expression LinqToSodaExpression(System.Linq.Expressions.Expression expression)
        {
            if ( expression == null ) {
                return null;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.And:
                    return Expressions.And(
                        LinqToSodaExpression(((BinaryExpression)expression).Left),
                        LinqToSodaExpression(((BinaryExpression)expression).Right));
                case ExpressionType.AndAlso:
                    return Expressions.And(
                        LinqToSodaExpression(((BinaryExpression)expression).Left),
                        LinqToSodaExpression(((BinaryExpression)expression).Right));
                case ExpressionType.Or:
                    return Expressions.Or(
                        LinqToSodaExpression(((BinaryExpression)expression).Left),
                        LinqToSodaExpression(((BinaryExpression)expression).Right));
                case ExpressionType.OrElse:
                    return Expressions.Or(
                        LinqToSodaExpression(((BinaryExpression)expression).Left),
                        LinqToSodaExpression(((BinaryExpression)expression).Right));
                case ExpressionType.Not:
                    return Expressions.Not(
                        LinqToSodaExpression(((UnaryExpression)expression).Operand));
                case ExpressionType.NotEqual:
                    return Expressions.Not(
                        Expressions.Eq(
                            LinqToSodaExpression(((BinaryExpression)expression).Left),
                            LinqToSodaExpression(((BinaryExpression)expression).Right)));
                case ExpressionType.Equal:
                    return Expressions.Eq(
                        LinqToSodaExpression(((BinaryExpression)expression).Left),
                        LinqToSodaExpression(((BinaryExpression)expression).Right));
                case ExpressionType.LessThan:
                    return Expressions.Lt(
                        LinqToSodaExpression(((BinaryExpression)expression).Left),
                        LinqToSodaExpression(((BinaryExpression)expression).Right));
                case ExpressionType.LessThanOrEqual:
                    return Expressions.Le(
                        LinqToSodaExpression(((BinaryExpression)expression).Left),
                        LinqToSodaExpression(((BinaryExpression)expression).Right));
                case ExpressionType.GreaterThan:
                    return Expressions.Gt(
                        LinqToSodaExpression(((BinaryExpression)expression).Left),
                        LinqToSodaExpression(((BinaryExpression)expression).Right));
                case ExpressionType.GreaterThanOrEqual:
                    return Expressions.Ge(
                        LinqToSodaExpression(((BinaryExpression)expression).Left),
                        LinqToSodaExpression(((BinaryExpression)expression).Right));
                case ExpressionType.MemberAccess:
                    return MemberToSoda(expression);
                case ExpressionType.Lambda:
                    return LambdaToSoda(expression);

                case ExpressionType.Convert:
                    {
                        var unary = (UnaryExpression)expression;
                        return Expressions.Cast(
                            LinqToSodaExpression(unary.Operand),
                            unary.Type.GetSimpleTypeName());
                    }

                case ExpressionType.Constant:
                    return Expressions.Constant(
                        ((System.Linq.Expressions.ConstantExpression)expression).Value);

                case ExpressionType.Call:
                    return CallToSoda(expression);
            }

            throw new ArgumentException(
                String.Format("Expression of type {0} is not supported", expression.NodeType), "expression");
        }

        /// <summary>
        /// Converts a lambda expression to a soda expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private static Expression LambdaToSoda(System.Linq.Expressions.Expression expression)
        {
            var lambda = (System.Linq.Expressions.LambdaExpression)expression;
            using (ScopedInstance<System.Linq.Expressions.LambdaExpression>.Set(lambda))
            {
                return LinqToSodaExpression(lambda.Body);
            }
        }

        /// <summary>
        /// Converts a member expression to a soda expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private static PropertyValueExpression MemberToSoda(System.Linq.Expressions.Expression expression)
        {
            // Get the current model - this defines the streams that are available
            var model = ScopedInstance<EPStatementObjectModel>.Current;
            // Get the current lambda - this defines the parameter that was used in the query
            var lambda = ScopedInstance<System.Linq.Expressions.LambdaExpression>.Current;
            // Get the member expression
            var memberExpr = (MemberExpression)expression;
            var memberInfo = memberExpr.Member;      // Field, Method or Property
            var memberData = memberExpr.Expression;  // Parameter expression - hopefully

            if (memberInfo is PropertyInfo)
            {
            }
            else if (memberInfo is FieldInfo)
            {
            }
            else
            {
                throw new NotSupportedException("Linq support only handles properties and fields");
            }

            var asParameter = memberData as ParameterExpression;
            if (asParameter != null)
            {
                var param = asParameter.Name;
                if (!lambda.Parameters.Contains(asParameter))
                {
                    throw new ArgumentException(
                        String.Format("Expression unable to find parameter named '{0}'", param));
                }

                // Parameter was located ... see if we can match the parameter against any of the stream names
                if (model.FromClause != null) {
                    var fromClauseStreams = model.FromClause.Streams;
                    var matchingStream = fromClauseStreams.FirstOrDefault(
                        stream => stream.StreamName == param);
                    if (matchingStream == null) {
                        do {
                            // Object was not found in the 'from' clause
                            var asOnDeleteClause = model.OnExpr as OnDeleteClause;
                            if (asOnDeleteClause != null) {
                                if ((param == asOnDeleteClause.WindowName) ||
                                    (param == asOnDeleteClause.OptionalAsName)) {
                                    return Expressions.Property(String.Format("{0}.{1}", param, memberInfo.Name));
                                }
                            }

                            var asOnSelectClause = model.OnExpr as OnSelectClause;
                            if (asOnSelectClause != null) {
                                if ((param == asOnSelectClause.WindowName) ||
                                    (param == asOnSelectClause.OptionalAsName)) {
                                    return Expressions.Property(String.Format("{0}.{1}", param, memberInfo.Name));
                                }
                            }

                            matchingStream = fromClauseStreams.FirstOrDefault();
                            if (matchingStream == null) {
                                throw new IllegalStateException("Object model does not have a stream in from clause");
                            }

                            return Expressions.Property(String.Format("{0}.{1}", param, memberInfo.Name));
                        } while (false);
                    }
                }

                // We should have a matching clause at this point
                return Expressions.Property(String.Format("{0}.{1}", param, memberInfo.Name));
            }

            throw new ArgumentException(
                String.Format("Member expression of type {0} is not supported", memberData.GetType().Name), "expression");
        }

        /// <summary>
        /// Converts a call expression to soda expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private static Expression CallToSoda(System.Linq.Expressions.Expression expression)
        {
            var methodCall = (MethodCallExpression)expression;
            var method = methodCall.Method;
            var declaringType = method.DeclaringType;

            if (method.IsPublic && method.IsStatic)
            {
                if (declaringType == typeof(EventBeanExtensions)) {
                    // Masquerade extensions for properties ...
                    return UnmasqProperty(methodCall.Arguments[1]);
                }

                var paramList = new List<Expression>();
                foreach (var argument in methodCall.Arguments)
                {
                    paramList.Add(LinqToSodaExpression(argument));
                }

                return Expressions.StaticMethod(
                    method.DeclaringType.FullName,
                    method.Name,
                    paramList.ToArray());
            }

            if (method.IsPublic && !method.IsStatic) {
                if (declaringType == typeof (EventBean)) {
                    if (method.Name == "Get") {
                        // Masquerade extensions for properties ...
                        return UnmasqProperty(methodCall.Arguments[1]);
                    }
                }
            }

            throw new NotSupportedException("Instance methods are not supported in this version");
        }

        /// <summary>
        /// Unmasqs a property.
        /// </summary>
        /// <param name="propertyNameExpr">The property name expr.</param>
        /// <returns></returns>
        private static Expression UnmasqProperty(System.Linq.Expressions.Expression propertyNameExpr)
        {
            var propertyNameConst = LinqToSodaExpression(propertyNameExpr);
            if (propertyNameConst is client.soda.ConstantExpression) {
                var propertyName = ((client.soda.ConstantExpression) propertyNameConst).Constant;
                if (propertyName is string) {
                    return Expressions.Property((string) propertyName);
                }
            }

            throw new ArgumentException("IsConstant property name expression must yield a constant string");
        }

        /// <summary>
        /// Converts the new invocation call into a select clause.
        /// </summary>
        /// <param name="newExpression">The new expression.</param>
        /// <returns></returns>
        public static SelectClause NewToSelectClause(System.Linq.Expressions.NewExpression newExpression)
        {
            var selectClause = new SelectClause();
            selectClause.IsDistinct = false;
            selectClause.StreamSelector = StreamSelector.RSTREAM_ISTREAM_BOTH;
            selectClause.SelectList = new List<SelectClauseElement>();

            foreach (var argExpression in newExpression.Arguments) {
                if (argExpression is MemberExpression) {
                    var memberExpression = (MemberExpression) argExpression;
                    var propertyExpression = MemberToSoda(memberExpression);
                    var selectClauseElement = new SelectClauseExpression(propertyExpression);
                    selectClause.SelectList.Add(selectClauseElement);
                } else {
                    throw new ArgumentException(
                        String.Format("Expression of type {0} is not supported", argExpression.NodeType));
                }
            }

            return selectClause;
        }

        /// <summary>
        /// Converts a LINQ expression to a select clause expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static SelectClause LinqToSelectClause(System.Linq.Expressions.Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                    do {
                        var lambda = (System.Linq.Expressions.LambdaExpression)expression;
                        using (ScopedInstance<System.Linq.Expressions.LambdaExpression>.Set(lambda))
                        {
                            return LinqToSelectClause(lambda.Body);
                        }
                    } while (false);
                case ExpressionType.New:
                    return NewToSelectClause(
                        (System.Linq.Expressions.NewExpression)expression);
            }

            throw new ArgumentException(
                String.Format("Expression of type {0} is not supported", expression.NodeType), "expression");
        }
    }
}
