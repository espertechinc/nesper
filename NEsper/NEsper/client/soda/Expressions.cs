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

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Convenience factory for creating <seealso cref="Expression" /> instances.
    /// <para>
    /// Provides quick-access methods to create all possible expressions and provides typical parameter lists to each.
    /// </para>
    /// <para>
    /// Note that only the typical parameter lists are provided and expressions can allow adding
    /// additional parameters.
    /// </para>
    /// <para>
    /// Many expressions, for example logical AND and OR (conjunction and disjunction), allow
    /// adding an unlimited number of additional sub-expressions to an expression. For those expressions
    /// there are additional add methods provided.
    /// </para>
    /// </summary>
    [Serializable]
    public class Expressions
    {

        /// <summary>
        /// Current system time supplies internal-timer provided time or
        /// the time provided by external timer events.
        /// </summary>
        /// <returns>expression</returns>
        public static CurrentTimestampExpression CurrentTimestamp()
        {
            return new CurrentTimestampExpression();
        }

        /// <summary>
        /// Exists-function for use with dynamic properties to test property existence.
        /// </summary>
        /// <param name="propertyName">name of the property to test whether it exists or not</param>
        /// <returns>expression</returns>
        public static PropertyExistsExpression ExistsProperty(string propertyName)
        {
            return new PropertyExistsExpression(propertyName);
        }

        /// <summary>
        /// Cast function, casts the result on an expression to the desired type, or
        /// returns null if the type cannot be casted to the type.
        /// <para>
        /// The list of types can include fully-qualified class names plus any of the
        /// primitive type names: byte, char, short, int, long, float, double, bool.
        /// Alternatively to "System.String" the simple "string" is also permitted.
        /// </para>
        /// <para>
        /// Type checks include all superclasses and interfaces of the value returned by the expression.
        /// </para>
        /// </summary>
        /// <param name="expression">returns the value to cast</param>
        /// <param name="typeName">is type to cast to</param>
        /// <returns>expression</returns>
        public static CastExpression Cast(Expression expression, string typeName)
        {
            return new CastExpression(expression, typeName);
        }

        /// <summary>
        /// Cast function, casts the result on an expression to the desired type, or
        /// returns null if the type cannot be casted to the type.
        /// <para>
        /// The list of types can include fully-qualified class names plus any of the
        /// primitive type names: byte, char, short, int, long, float, double, bool.
        /// Alternatively to "System.String" the simple "string" is also permitted.
        /// </para>
        /// <para>
        /// Type checks include all superclasses and interfaces of the value returned by the expression.
        /// </para>
        /// </summary>
        /// <param name="propertyName">name of the property supplying the value to cast</param>
        /// <param name="typeName">is type to cast to</param>
        /// <returns>expression</returns>
        public static CastExpression Cast(string propertyName, string typeName)
        {
            return new CastExpression(GetPropExpr(propertyName), typeName);
        }

        /// <summary>
        /// Instance-of function, tests if the type of the return value of an expression is in a list of types.
        /// <para>
        /// The list of types can include fully-qualified class names plus any of the
        /// primitive type names: byte, char, short, int, long, float, double, bool.
        /// Alternatively to "System.String" the simple "string" is also permitted.
        /// </para>
        /// <para>
        /// Type checks include all superclasses and interfaces of the value returned by the expression.
        /// </para>
        /// </summary>
        /// <param name="expression">returns the value to test whether the type returned is any of the  is the function name</param>
        /// <param name="typeName">is one type to check for</param>
        /// <param name="typeNames">is optional additional types to check for in a list</param>
        /// <returns>expression</returns>
        public static InstanceOfExpression InstanceOf(Expression expression, string typeName, params string[] typeNames)
        {
            return new InstanceOfExpression(expression, typeName, typeNames);
        }

        /// <summary>
        /// Instance-of function, tests if the type of the return value of a property is in a list of types.
        /// <para>
        /// Useful with dynamic (unchecked) properties to check the type of property returned.
        /// </para>
        /// <para>
        /// The list of types can include fully-qualified class names plus any of the
        /// primitive type names: byte, char, short, int, long, float, double, bool.
        /// Alternatively to "System.String" the simple "string" is also permitted.
        /// </para>
        /// <para>
        /// Type checks include all superclasses and interfaces of the value returned by the expression.
        /// </para>
        /// </summary>
        /// <param name="propertyName">name of the property supplying the value to test</param>
        /// <param name="typeName">is one type to check for</param>
        /// <param name="typeNames">is optional additional types to check for in a list</param>
        /// <returns>expression</returns>
        public static InstanceOfExpression InstanceOf(string propertyName, string typeName, params string[] typeNames)
        {
            return new InstanceOfExpression(GetPropExpr(propertyName), typeName, typeNames);
        }

        /// <summary>
        /// Type-of function, returns the event type name or result type as a string of a stream name, property or expression.
        /// </summary>
        /// <param name="expression">to evaluate and return it's result type as a string</param>
        /// <returns>expression</returns>
        public static TypeOfExpression TypeOf(Expression expression)
        {
            return new TypeOfExpression(expression);
        }

        /// <summary>
        /// Type-of function, returns the event type name or result type as a string of a stream name, property or expression.
        /// </summary>
        /// <param name="propertyName">returns the property to evaluate and return its event type name or property class type</param>
        /// <returns>expression</returns>
        public static TypeOfExpression TypeOf(string propertyName)
        {
            return new TypeOfExpression(GetPropExpr(propertyName));
        }

        /// <summary>
        /// Plug-in aggregation function.
        /// </summary>
        /// <param name="functionName">is the function name</param>
        /// <param name="moreExpressions">provides the values to aggregate</param>
        /// <returns>expression</returns>
        public static PlugInProjectionExpression PlugInAggregation(
            string functionName,
            params Expression[] moreExpressions)
        {
            return new PlugInProjectionExpression(functionName, false, moreExpressions);
        }

        /// <summary>
        /// Regular expression.
        /// </summary>
        /// <param name="left">returns the values to match</param>
        /// <param name="right">returns the value to match against</param>
        /// <returns>expression</returns>
        public static RegExpExpression Regexp(Expression left, Expression right)
        {
            return new RegExpExpression(left, right);
        }

        /// <summary>
        /// Regular expression.
        /// </summary>
        /// <param name="left">returns the values to match</param>
        /// <param name="right">returns the value to match against</param>
        /// <param name="escape">is the escape character</param>
        /// <returns>expression</returns>
        public static RegExpExpression Regexp(Expression left, Expression right, string escape)
        {
            return new RegExpExpression(left, right, new ConstantExpression(escape));
        }

        /// <summary>
        /// Regular expression.
        /// </summary>
        /// <param name="property">the name of the property returning values to match</param>
        /// <param name="regExExpression">a regular expression to match against</param>
        /// <returns>expression</returns>
        public static RegExpExpression Regexp(string property, string regExExpression)
        {
            return new RegExpExpression(GetPropExpr(property), new ConstantExpression(regExExpression));
        }

        /// <summary>
        /// Regular expression.
        /// </summary>
        /// <param name="property">the name of the property returning values to match</param>
        /// <param name="regExExpression">a regular expression to match against</param>
        /// <param name="escape">is the escape character</param>
        /// <returns>expression</returns>
        public static RegExpExpression Regexp(string property, string regExExpression, string escape)
        {
            return new RegExpExpression(
                GetPropExpr(property), new ConstantExpression(regExExpression), new ConstantExpression(escape));
        }

        /// <summary>
        /// Regular expression negated (not regexp).
        /// </summary>
        /// <param name="left">returns the values to match</param>
        /// <param name="right">returns the value to match against</param>
        /// <returns>expression</returns>
        public static RegExpExpression NotRegexp(Expression left, Expression right)
        {
            return new RegExpExpression(left, right, true);
        }

        /// <summary>
        /// Regular expression negated (not regexp).
        /// </summary>
        /// <param name="left">returns the values to match</param>
        /// <param name="right">returns the value to match against</param>
        /// <param name="escape">is the escape character</param>
        /// <returns>expression</returns>
        public static RegExpExpression NotRegexp(Expression left, Expression right, string escape)
        {
            return new RegExpExpression(left, right, new ConstantExpression(escape), true);
        }

        /// <summary>
        /// Regular expression negated (not regexp).
        /// </summary>
        /// <param name="property">the name of the property returning values to match</param>
        /// <param name="regExExpression">a regular expression to match against</param>
        /// <returns>expression</returns>
        public static RegExpExpression NotRegexp(string property, string regExExpression)
        {
            return new RegExpExpression(GetPropExpr(property), new ConstantExpression(regExExpression), true);
        }

        /// <summary>
        /// Regular expression negated (not regexp).
        /// </summary>
        /// <param name="property">the name of the property returning values to match</param>
        /// <param name="regExExpression">a regular expression to match against</param>
        /// <param name="escape">is the escape character</param>
        /// <returns>expression</returns>
        public static RegExpExpression NotRegexp(string property, string regExExpression, string escape)
        {
            return new RegExpExpression(
                GetPropExpr(property), new ConstantExpression(regExExpression), new ConstantExpression(escape), true);
        }

        /// <summary>
        /// Array expression, representing the syntax of "{1, 2, 3}" returning an integer array of 3 elements valued 1, 2, 3.
        /// </summary>
        /// <returns>expression</returns>
        public static ArrayExpression Array()
        {
            return new ArrayExpression();
        }

        /// <summary>
        /// Bitwise (binary) AND.
        /// </summary>
        /// <returns>expression</returns>
        public static BitwiseOpExpression BinaryAnd()
        {
            return new BitwiseOpExpression(BitWiseOpEnum.BAND);
        }

        /// <summary>
        /// Bitwise (binary) OR.
        /// </summary>
        /// <returns>expression</returns>
        public static BitwiseOpExpression BinaryOr()
        {
            return new BitwiseOpExpression(BitWiseOpEnum.BOR);
        }

        /// <summary>
        /// Bitwise (binary) XOR.
        /// </summary>
        /// <returns>expression</returns>
        public static BitwiseOpExpression BinaryXor()
        {
            return new BitwiseOpExpression(BitWiseOpEnum.BXOR);
        }

        /// <summary>
        /// Minimum value per-row function (not aggregating).
        /// </summary>
        /// <param name="propertyOne">the name of a first property to compare</param>
        /// <param name="propertyTwo">the name of a second property to compare</param>
        /// <param name="moreProperties">optional additional properties to compare</param>
        /// <returns>expression</returns>
        public static MinRowExpression Min(string propertyOne, string propertyTwo, params string[] moreProperties)
        {
            return new MinRowExpression(propertyOne, propertyTwo, moreProperties);
        }

        /// <summary>
        /// Minimum value per-row function (not aggregating).
        /// </summary>
        /// <param name="exprOne">returns the first value to compare</param>
        /// <param name="exprTwo">returns the second value to compare</param>
        /// <param name="moreExpressions">optional additional values to compare</param>
        /// <returns>expression</returns>
        public static MinRowExpression Min(Expression exprOne, Expression exprTwo, params Expression[] moreExpressions)
        {
            return new MinRowExpression(exprOne, exprTwo, moreExpressions);
        }

        /// <summary>
        /// Maximum value per-row function (not aggregating).
        /// </summary>
        /// <param name="propertyOne">the name of a first property to compare</param>
        /// <param name="propertyTwo">the name of a second property to compare</param>
        /// <param name="moreProperties">optional additional properties to compare</param>
        /// <returns>expression</returns>
        public static MaxRowExpression Max(string propertyOne, string propertyTwo, params string[] moreProperties)
        {
            return new MaxRowExpression(propertyOne, propertyTwo, moreProperties);
        }

        /// <summary>
        /// Maximum value per-row function (not aggregating).
        /// </summary>
        /// <param name="exprOne">returns the first value to compare</param>
        /// <param name="exprTwo">returns the second value to compare</param>
        /// <param name="moreExpressions">optional additional values to compare</param>
        /// <returns>expression</returns>
        public static MaxRowExpression Max(Expression exprOne, Expression exprTwo, params Expression[] moreExpressions)
        {
            return new MaxRowExpression(exprOne, exprTwo, moreExpressions);
        }

        /// <summary>
        /// Coalesce.
        /// </summary>
        /// <param name="propertyOne">name of the first property returning value to coealesce</param>
        /// <param name="propertyTwo">name of the second property returning value to coealesce</param>
        /// <param name="moreProperties">name of the optional additional properties returning values to coealesce</param>
        /// <returns>expression</returns>
        public static CoalesceExpression Coalesce(
            string propertyOne,
            string propertyTwo,
            params string[] moreProperties)
        {
            return new CoalesceExpression(propertyOne, propertyTwo, moreProperties);
        }

        /// <summary>
        /// Coalesce.
        /// </summary>
        /// <param name="exprOne">returns value to coalesce</param>
        /// <param name="exprTwo">returns value to coalesce</param>
        /// <param name="moreExpressions">returning optional additional values to coalesce</param>
        /// <returns>expression</returns>
        public static CoalesceExpression Coalesce(
            Expression exprOne,
            Expression exprTwo,
            params Expression[] moreExpressions)
        {
            return new CoalesceExpression(exprOne, exprTwo, moreExpressions);
        }

        /// <summary>
        /// IsConstant.
        /// </summary>
        /// <param name="value">is the constant value</param>
        /// <returns>expression</returns>
        public static ConstantExpression Constant(Object value)
        {
            return new ConstantExpression(value);
        }

        /// <summary>
        /// IsConstant, use when the value is null.
        /// </summary>
        /// <param name="value">is the constant value</param>
        /// <param name="constantType">is the type of the constant</param>
        /// <returns>expression</returns>
        public static ConstantExpression Constant(Object value, Type constantType)
        {
            return new ConstantExpression(value, constantType.Name);
        }

        /// <summary>
        /// Case-when-then expression.
        /// </summary>
        /// <returns>expression</returns>
        public static CaseWhenThenExpression CaseWhenThen()
        {
            return new CaseWhenThenExpression();
        }

        /// <summary>
        /// Case-switch expresssion.
        /// </summary>
        /// <param name="valueToSwitchOn">provides the switch value</param>
        /// <returns>expression</returns>
        public static CaseSwitchExpression CaseSwitch(Expression valueToSwitchOn)
        {
            return new CaseSwitchExpression(valueToSwitchOn);
        }

        /// <summary>
        /// Case-switch expresssion.
        /// </summary>
        /// <param name="propertyName">the name of the property that provides the switch value</param>
        /// <returns>expression</returns>
        public static CaseSwitchExpression CaseSwitch(string propertyName)
        {
            return new CaseSwitchExpression(GetPropExpr(propertyName));
        }

        /// <summary>
        /// In-expression that is equivalent to the syntax of "property in (value, value, ... value)".
        /// </summary>
        /// <param name="property">is the name of the property</param>
        /// <param name="values">are the constants to check against</param>
        /// <returns>expression</returns>
        public static InExpression In(string property, params Object[] values)
        {
            return new InExpression(GetPropExpr(property), false, values);
        }

        /// <summary>
        /// Not-In-expression that is equivalent to the syntax of "property not in (value, value, ... value)".
        /// </summary>
        /// <param name="property">is the name of the property</param>
        /// <param name="values">are the constants to check against</param>
        /// <returns>expression</returns>
        public static InExpression NotIn(string property, params Object[] values)
        {
            return new InExpression(GetPropExpr(property), true, values);
        }

        /// <summary>
        /// In-expression that is equivalent to the syntax of "property in (value, value, ... value)".
        /// </summary>
        /// <param name="value">provides values to match</param>
        /// <param name="set">are expressons that provide match-against values</param>
        /// <returns>expression</returns>
        public static InExpression In(Expression value, params Expression[] set)
        {
            return new InExpression(value, false, set);
        }

        /// <summary>
        /// Not-In-expression that is equivalent to the syntax of "property not in (value, value, ... value)".
        /// </summary>
        /// <param name="value">provides values to match</param>
        /// <param name="set">are expressons that provide match-against values</param>
        /// <returns>expression</returns>
        public static InExpression NotIn(Expression value, params Expression[] set)
        {
            return new InExpression(value, true, (Object) set);
        }

        /// <summary>
        /// Not expression negates the sub-expression to the not which is expected to return bool-typed values.
        /// </summary>
        /// <param name="inner">is the sub-expression</param>
        /// <returns>expression</returns>
        public static NotExpression Not(Expression inner)
        {
            return new NotExpression(inner);
        }

        /// <summary>
        /// Static method invocation.
        /// </summary>
        /// <param name="className">the name of the class to invoke a method on</param>
        /// <param name="method">the name of the method to invoke</param>
        /// <param name="parameters">zero, one or more constants that are the parameters to the static method</param>
        /// <returns>expression</returns>
        public static StaticMethodExpression StaticMethod(string className, string method, params Object[] parameters)
        {
            return new StaticMethodExpression(className, method, parameters);
        }

        /// <summary>
        /// Static method invocation.
        /// </summary>
        /// <param name="className">the name of the class to invoke a method on</param>
        /// <param name="method">the name of the method to invoke</param>
        /// <param name="parameters">zero, one or more expressions that provide parameters to the static method</param>
        /// <returns>expression</returns>
        public static StaticMethodExpression StaticMethod(
            string className,
            string method,
            params Expression[] parameters)
        {
            return new StaticMethodExpression(className, method, parameters);
        }

        /// <summary>
        /// Prior function.
        /// </summary>
        /// <param name="index">the numeric index of the prior event</param>
        /// <param name="property">the name of the property to obtain the value for</param>
        /// <returns>expression</returns>
        public static PriorExpression Prior(int index, string property)
        {
            return new PriorExpression(index, property);
        }

        /// <summary>
        /// Previous function.
        /// </summary>
        /// <param name="expression">provides the numeric index of the previous event</param>
        /// <param name="property">the name of the property to obtain the value for</param>
        /// <returns>expression</returns>
        public static PreviousExpression Previous(Expression expression, string property)
        {
            return new PreviousExpression(expression, property);
        }

        /// <summary>
        /// Previous function.
        /// </summary>
        /// <param name="index">the numeric index of the previous event</param>
        /// <param name="property">the name of the property to obtain the value for</param>
        /// <returns>expression</returns>
        public static PreviousExpression Previous(int index, string property)
        {
            return new PreviousExpression(index, property);
        }

        /// <summary>
        /// Previous tail function.
        /// </summary>
        /// <param name="expression">provides the numeric index of the previous event</param>
        /// <param name="property">the name of the property to obtain the value for</param>
        /// <returns>expression</returns>
        public static PreviousExpression PreviousTail(Expression expression, string property)
        {
            var expr = new PreviousExpression(expression, property);
            expr.ExpressionType = PreviousExpressionType.PREVTAIL;
            return expr;
        }

        /// <summary>
        /// Previous tail function.
        /// </summary>
        /// <param name="index">the numeric index of the previous event</param>
        /// <param name="property">the name of the property to obtain the value for</param>
        /// <returns>expression</returns>
        public static PreviousExpression PreviousTail(int index, string property)
        {
            var expr = new PreviousExpression(index, property);
            expr.ExpressionType = PreviousExpressionType.PREVTAIL;
            return expr;
        }

        /// <summary>
        /// Previous count function.
        /// </summary>
        /// <param name="property">provides the properties or stream name to select for the previous event</param>
        /// <returns>expression</returns>
        public static PreviousExpression PreviousCount(string property)
        {
            return new PreviousExpression(PreviousExpressionType.PREVCOUNT, Property(property));
        }

        /// <summary>
        /// Previous window function.
        /// </summary>
        /// <param name="property">provides the properties or stream name to select for the previous event</param>
        /// <returns>expression</returns>
        public static PreviousExpression PreviousWindow(string property)
        {
            return new PreviousExpression(PreviousExpressionType.PREVWINDOW, Property(property));
        }

        /// <summary>
        /// Between.
        /// </summary>
        /// <param name="property">the name of the property supplying data points.</param>
        /// <param name="lowBoundaryProperty">the name of the property supplying lower boundary.</param>
        /// <param name="highBoundaryProperty">the name of the property supplying upper boundary.</param>
        /// <returns>expression</returns>
        public static BetweenExpression BetweenProperty(
            string property,
            string lowBoundaryProperty,
            string highBoundaryProperty)
        {
            return new BetweenExpression(
                GetPropExpr(property), GetPropExpr(lowBoundaryProperty), GetPropExpr(highBoundaryProperty));
        }

        /// <summary>
        /// Between.
        /// </summary>
        /// <param name="property">the name of the property that returns the datapoint to check range</param>
        /// <param name="lowBoundary">constant indicating the lower boundary</param>
        /// <param name="highBoundary">constant indicating the upper boundary</param>
        /// <returns>expression</returns>
        public static BetweenExpression Between(string property, Object lowBoundary, Object highBoundary)
        {
            return new BetweenExpression(
                GetPropExpr(property), new ConstantExpression(lowBoundary), new ConstantExpression(highBoundary));
        }

        /// <summary>
        /// Between.
        /// </summary>
        /// <param name="datapoint">returns the datapoint to check range</param>
        /// <param name="lowBoundary">returns values for the lower boundary</param>
        /// <param name="highBoundary">returns values for the upper boundary</param>
        /// <returns>expression</returns>
        public static BetweenExpression Between(Expression datapoint, Expression lowBoundary, Expression highBoundary)
        {
            return new BetweenExpression(datapoint, lowBoundary, highBoundary);
        }

        /// <summary>
        /// Between (or range).
        /// </summary>
        /// <param name="datapoint">returns the datapoint to check range</param>
        /// <param name="lowBoundary">returns values for the lower boundary</param>
        /// <param name="highBoundary">returns values for the upper boundary</param>
        /// <param name="isLowIncluded">true to indicate lower boundary itself is included in the range</param>
        /// <param name="isHighIncluded">true to indicate upper boundary itself is included in the range</param>
        /// <returns>expression</returns>
        public static BetweenExpression Range(
            Expression datapoint,
            Expression lowBoundary,
            Expression highBoundary,
            bool isLowIncluded,
            bool isHighIncluded)
        {
            return new BetweenExpression(datapoint, lowBoundary, highBoundary, isLowIncluded, isHighIncluded, false);
        }

        /// <summary>
        /// Logical OR disjunction. Use add methods to add expressions.
        /// </summary>
        /// <returns>expression</returns>
        public static Disjunction Or()
        {
            return new Disjunction();
        }

        /// <summary>
        /// Logical OR disjunction.
        /// </summary>
        /// <param name="first">an expression returning values to junction</param>
        /// <param name="second">an expression returning values to junction</param>
        /// <param name="expressions">an optional list of expressions returning values to junction</param>
        /// <returns>expression</returns>
        public static Disjunction Or(Expression first, Expression second, params Expression[] expressions)
        {
            return new Disjunction(first, second, expressions);
        }

        /// <summary>
        /// Logical AND conjunction. Use add methods to add expressions.
        /// </summary>
        /// <returns>expression</returns>
        public static Conjunction And()
        {
            return new Conjunction();
        }

        /// <summary>
        /// Logical AND conjunction.
        /// </summary>
        /// <param name="first">an expression returning values to junction</param>
        /// <param name="second">an expression returning values to junction</param>
        /// <param name="expressions">an optional list of expressions returning values to junction</param>
        /// <returns>expression</returns>
        public static Conjunction And(Expression first, Expression second, params Expression[] expressions)
        {
            return new Conjunction(first, second, expressions);
        }

        /// <summary>
        /// Greater-or-equal between a property and a constant.
        /// </summary>
        /// <param name="propertyName">the name of the property providing left hand side values</param>
        /// <param name="value">is the constant to compare</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression Ge(string propertyName, Object value)
        {
            return new RelationalOpExpression(GetPropExpr(propertyName), ">=", new ConstantExpression(value));
        }

        /// <summary>
        /// Greater-or-equals between expression results.
        /// </summary>
        /// <param name="left">the expression providing left hand side values</param>
        /// <param name="right">the expression providing right hand side values</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression Ge(Expression left, Expression right)
        {
            return new RelationalOpExpression(left, ">=", right);
        }

        /// <summary>
        /// Greater-or-equal between properties.
        /// </summary>
        /// <param name="propertyLeft">the name of the property providing left hand side values</param>
        /// <param name="propertyRight">the name of the property providing right hand side values</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression GeProperty(string propertyLeft, string propertyRight)
        {
            return new RelationalOpExpression(
                GetPropExpr(propertyLeft), ">=", new PropertyValueExpression(propertyRight));
        }

        /// <summary>
        /// Greater-then between a property and a constant.
        /// </summary>
        /// <param name="propertyName">the name of the property providing left hand side values</param>
        /// <param name="value">is the constant to compare</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression Gt(string propertyName, Object value)
        {
            return new RelationalOpExpression(GetPropExpr(propertyName), ">", new ConstantExpression(value));
        }

        /// <summary>
        /// Greater-then between expression results.
        /// </summary>
        /// <param name="left">the expression providing left hand side values</param>
        /// <param name="right">the expression providing right hand side values</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression Gt(Expression left, Expression right)
        {
            return new RelationalOpExpression(left, ">", right);
        }

        /// <summary>
        /// Greater-then between properties.
        /// </summary>
        /// <param name="propertyLeft">the name of the property providing left hand side values</param>
        /// <param name="propertyRight">the name of the property providing right hand side values</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression GtProperty(string propertyLeft, string propertyRight)
        {
            return new RelationalOpExpression(
                GetPropExpr(propertyLeft), ">", new PropertyValueExpression(propertyRight));
        }

        /// <summary>
        /// Less-or-equals between a property and a constant.
        /// </summary>
        /// <param name="propertyName">the name of the property providing left hand side values</param>
        /// <param name="value">is the constant to compare</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression Le(string propertyName, Object value)
        {
            return new RelationalOpExpression(GetPropExpr(propertyName), "<=", new ConstantExpression(value));
        }

        /// <summary>
        /// Less-or-equal between properties.
        /// </summary>
        /// <param name="propertyLeft">the name of the property providing left hand side values</param>
        /// <param name="propertyRight">the name of the property providing right hand side values</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression LeProperty(string propertyLeft, string propertyRight)
        {
            return new RelationalOpExpression(
                GetPropExpr(propertyLeft), "<=", new PropertyValueExpression(propertyRight));
        }

        /// <summary>
        /// Less-or-equal between expression results.
        /// </summary>
        /// <param name="left">the expression providing left hand side values</param>
        /// <param name="right">the expression providing right hand side values</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression Le(Expression left, Expression right)
        {
            return new RelationalOpExpression(left, "<=", right);
        }

        /// <summary>
        /// Less-then between a property and a constant.
        /// </summary>
        /// <param name="propertyName">the name of the property providing left hand side values</param>
        /// <param name="value">is the constant to compare</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression Lt(string propertyName, Object value)
        {
            return new RelationalOpExpression(GetPropExpr(propertyName), "<", new ConstantExpression(value));
        }

        /// <summary>
        /// Less-then between properties.
        /// </summary>
        /// <param name="propertyLeft">the name of the property providing left hand side values</param>
        /// <param name="propertyRight">the name of the property providing right hand side values</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression LtProperty(string propertyLeft, string propertyRight)
        {
            return new RelationalOpExpression(
                GetPropExpr(propertyLeft), "<", new PropertyValueExpression(propertyRight));
        }

        /// <summary>
        /// Less-then between expression results.
        /// </summary>
        /// <param name="left">the expression providing left hand side values</param>
        /// <param name="right">the expression providing right hand side values</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression Lt(Expression left, Expression right)
        {
            return new RelationalOpExpression(left, "<", right);
        }

        /// <summary>
        /// Equals between a property and a constant.
        /// </summary>
        /// <param name="propertyName">the name of the property providing left hand side values</param>
        /// <param name="value">is the constant to compare</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression Eq(string propertyName, Object value)
        {
            return new RelationalOpExpression(GetPropExpr(propertyName), "=", new ConstantExpression(value));
        }

        /// <summary>
        /// Not-Equals between a property and a constant.
        /// </summary>
        /// <param name="propertyName">the name of the property providing left hand side values</param>
        /// <param name="value">is the constant to compare</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression Neq(string propertyName, Object value)
        {
            return new RelationalOpExpression(GetPropExpr(propertyName), "!=", new ConstantExpression(value));
        }

        /// <summary>
        /// Equals between properties.
        /// </summary>
        /// <param name="propertyLeft">the name of the property providing left hand side values</param>
        /// <param name="propertyRight">the name of the property providing right hand side values</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression EqProperty(string propertyLeft, string propertyRight)
        {
            return new RelationalOpExpression(
                GetPropExpr(propertyLeft), "=", new PropertyValueExpression(propertyRight));
        }

        /// <summary>
        /// Not-Equals between properties.
        /// </summary>
        /// <param name="propertyLeft">the name of the property providing left hand side values</param>
        /// <param name="propertyRight">the name of the property providing right hand side values</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression NeqProperty(string propertyLeft, string propertyRight)
        {
            return new RelationalOpExpression(
                GetPropExpr(propertyLeft), "!=", new PropertyValueExpression(propertyRight));
        }

        /// <summary>
        /// Equals between expression results.
        /// </summary>
        /// <param name="left">the expression providing left hand side values</param>
        /// <param name="right">the expression providing right hand side values</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression Eq(Expression left, Expression right)
        {
            return new RelationalOpExpression(left, "=", right);
        }

        /// <summary>
        /// Not-Equals between expression results.
        /// </summary>
        /// <param name="left">the expression providing left hand side values</param>
        /// <param name="right">the expression providing right hand side values</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression Neq(Expression left, Expression right)
        {
            return new RelationalOpExpression(left, "!=", right);
        }

        /// <summary>
        /// Not-null test.
        /// </summary>
        /// <param name="property">the name of the property supplying the value to check for null</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression IsNotNull(string property)
        {
            return new RelationalOpExpression(GetPropExpr(property), "is not", null);
        }

        /// <summary>
        /// Not-null test.
        /// </summary>
        /// <param name="expression">supplies the value to check for null</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression IsNotNull(Expression expression)
        {
            return new RelationalOpExpression(expression, "is not", null);
        }

        /// <summary>
        /// Is-null test.
        /// </summary>
        /// <param name="property">the name of the property supplying the value to check for null</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression IsNull(string property)
        {
            return new RelationalOpExpression(GetPropExpr(property), "is", null);
        }

        /// <summary>
        /// Is-null test.
        /// </summary>
        /// <param name="expression">supplies the value to check for null</param>
        /// <returns>expression</returns>
        public static RelationalOpExpression IsNull(Expression expression)
        {
            return new RelationalOpExpression(expression, "is", null);
        }

        /// <summary>
        /// Property value.
        /// <para>
        /// An expression that returns the value of the named property.
        /// </para>
        /// <para>
        /// Nested, indexed or mapped properties follow the documented sytnax.
        /// </para>
        /// </summary>
        /// <param name="propertyName">is the name of the property to return the value for.</param>
        /// <returns>expression</returns>
        public static PropertyValueExpression Property(string propertyName)
        {
            return GetPropExpr(propertyName);
        }

        /// <summary>
        /// SQL-Like.
        /// </summary>
        /// <param name="propertyName">the name of the property providing values to match</param>
        /// <param name="value">is the string to match against</param>
        /// <returns>expression</returns>
        public static LikeExpression Like(string propertyName, string value)
        {
            return new LikeExpression(GetPropExpr(propertyName), new ConstantExpression(value));
        }

        /// <summary>
        /// SQL-Like.
        /// </summary>
        /// <param name="left">provides value to match</param>
        /// <param name="right">provides string to match against</param>
        /// <returns>expression</returns>
        public static LikeExpression Like(Expression left, Expression right)
        {
            return new LikeExpression(left, right);
        }

        /// <summary>
        /// SQL-Like.
        /// </summary>
        /// <param name="propertyName">the name of the property providing values to match</param>
        /// <param name="value">is the string to match against</param>
        /// <param name="escape">the escape Character(s)</param>
        /// <returns>expression</returns>
        public static LikeExpression Like(string propertyName, Object value, string escape)
        {
            return new LikeExpression(
                GetPropExpr(propertyName), new ConstantExpression(value), new ConstantExpression(escape));
        }

        /// <summary>
        /// SQL-Like.
        /// </summary>
        /// <param name="left">provides value to match</param>
        /// <param name="right">provides string to match against</param>
        /// <param name="escape">the escape Character(s)</param>
        /// <returns>expression</returns>
        public static LikeExpression Like(Expression left, Expression right, Expression escape)
        {
            return new LikeExpression(left, right, escape);
        }

        /// <summary>
        /// SQL-Like negated (not like).
        /// </summary>
        /// <param name="propertyName">the name of the property providing values to match</param>
        /// <param name="value">is the string to match against</param>
        /// <returns>expression</returns>
        public static LikeExpression NotLike(string propertyName, string value)
        {
            return new LikeExpression(GetPropExpr(propertyName), new ConstantExpression(value), true);
        }

        /// <summary>
        /// SQL-Like negated (not like).
        /// </summary>
        /// <param name="left">provides value to match</param>
        /// <param name="right">provides string to match against</param>
        /// <returns>expression</returns>
        public static LikeExpression NotLike(Expression left, Expression right)
        {
            return new LikeExpression(left, right, true);
        }

        /// <summary>
        /// SQL-Like negated (not like).
        /// </summary>
        /// <param name="propertyName">the name of the property providing values to match</param>
        /// <param name="value">is the string to match against</param>
        /// <param name="escape">the escape Character(s)</param>
        /// <returns>expression</returns>
        public static LikeExpression NotLike(string propertyName, Object value, string escape)
        {
            return new LikeExpression(
                GetPropExpr(propertyName), new ConstantExpression(value), new ConstantExpression(escape), true);
        }

        /// <summary>
        /// SQL-Like negated (not like).
        /// </summary>
        /// <param name="left">provides value to match</param>
        /// <param name="right">provides string to match against</param>
        /// <param name="escape">the escape Character(s)</param>
        /// <returns>expression</returns>
        public static LikeExpression NotLike(Expression left, Expression right, Expression escape)
        {
            return new LikeExpression(left, right, escape, true);
        }

        /// <summary>
        /// Average aggregation function.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static AvgProjectionExpression Avg(string propertyName)
        {
            return new AvgProjectionExpression(GetPropExpr(propertyName), false);
        }

        /// <summary>
        /// Average aggregation function.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static AvgProjectionExpression Avg(Expression expression)
        {
            return new AvgProjectionExpression(expression, false);
        }

        /// <summary>
        /// Average aggregation function considering distinct values only.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static AvgProjectionExpression AvgDistinct(string propertyName)
        {
            return new AvgProjectionExpression(GetPropExpr(propertyName), true);
        }

        /// <summary>
        /// Average aggregation function considering distinct values only.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static AvgProjectionExpression AvgDistinct(Expression expression)
        {
            return new AvgProjectionExpression(expression, true);
        }

        /// <summary>
        /// Median aggregation function.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static MedianProjectionExpression Median(string propertyName)
        {
            return new MedianProjectionExpression(GetPropExpr(propertyName), false);
        }

        /// <summary>
        /// Median aggregation function.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static MedianProjectionExpression Median(Expression expression)
        {
            return new MedianProjectionExpression(expression, false);
        }

        /// <summary>
        /// Median aggregation function considering distinct values only.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static MedianProjectionExpression MedianDistinct(string propertyName)
        {
            return new MedianProjectionExpression(GetPropExpr(propertyName), true);
        }

        /// <summary>
        /// Median aggregation function considering distinct values only.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static MedianProjectionExpression MedianDistinct(Expression expression)
        {
            return new MedianProjectionExpression(expression, true);
        }

        /// <summary>
        /// Standard deviation aggregation function.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static StddevProjectionExpression Stddev(string propertyName)
        {
            return new StddevProjectionExpression(GetPropExpr(propertyName), false);
        }

        /// <summary>
        /// Standard deviation aggregation function.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static StddevProjectionExpression Stddev(Expression expression)
        {
            return new StddevProjectionExpression(expression, false);
        }

        /// <summary>
        /// Standard deviation function considering distinct values only.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static StddevProjectionExpression StddevDistinct(string propertyName)
        {
            return new StddevProjectionExpression(GetPropExpr(propertyName), true);
        }

        /// <summary>
        /// Standard deviation function considering distinct values only.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static StddevProjectionExpression StddevDistinct(Expression expression)
        {
            return new StddevProjectionExpression(expression, true);
        }

        /// <summary>
        /// Mean deviation aggregation function.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static AvedevProjectionExpression Avedev(string propertyName)
        {
            return new AvedevProjectionExpression(GetPropExpr(propertyName), false);
        }

        /// <summary>
        /// Lastever-value aggregation function.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static LastEverProjectionExpression LastEver(string propertyName)
        {
            return new LastEverProjectionExpression(GetPropExpr(propertyName), false);
        }

        /// <summary>
        /// Lastever-value aggregation function.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static LastProjectionExpression Last(string propertyName)
        {
            return new LastProjectionExpression(GetPropExpr(propertyName));
        }

        /// <summary>
        /// Lastever-value aggregation function.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static LastEverProjectionExpression LastEver(Expression expression)
        {
            return new LastEverProjectionExpression(expression, false);
        }

        /// <summary>
        /// Lastever-value aggregation function.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static LastProjectionExpression Last(Expression expression)
        {
            return new LastProjectionExpression(expression);
        }

        /// <summary>
        /// First-value (windowed) aggregation function.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static FirstProjectionExpression First(string propertyName)
        {
            return new FirstProjectionExpression(GetPropExpr(propertyName));
        }

        /// <summary>
        /// First-value (ever) aggregation function.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static FirstEverProjectionExpression FirstEver(string propertyName)
        {
            return new FirstEverProjectionExpression(GetPropExpr(propertyName), false);
        }

        /// <summary>
        /// First-value (in window) aggregation function.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static FirstProjectionExpression First(Expression expression)
        {
            return new FirstProjectionExpression(expression);
        }

        /// <summary>
        /// First-value (ever) aggregation function.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static FirstEverProjectionExpression FirstEver(Expression expression)
        {
            return new FirstEverProjectionExpression(expression, false);
        }

        /// <summary>
        /// Mean deviation aggregation function.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static AvedevProjectionExpression Avedev(Expression expression)
        {
            return new AvedevProjectionExpression(expression, false);
        }

        /// <summary>
        /// Mean deviation function considering distinct values only.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static AvedevProjectionExpression AvedevDistinct(string propertyName)
        {
            return new AvedevProjectionExpression(GetPropExpr(propertyName), false);
        }

        /// <summary>
        /// Mean deviation function considering distinct values only.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static AvedevProjectionExpression AvedevDistinct(Expression expression)
        {
            return new AvedevProjectionExpression(expression, false);
        }

        /// <summary>
        /// Sum aggregation function.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static SumProjectionExpression Sum(string propertyName)
        {
            return new SumProjectionExpression(GetPropExpr(propertyName), false);
        }

        /// <summary>
        /// Sum aggregation function.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static SumProjectionExpression Sum(Expression expression)
        {
            return new SumProjectionExpression(expression, false);
        }

        /// <summary>
        /// Sum aggregation function considering distinct values only.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static SumProjectionExpression SumDistinct(string propertyName)
        {
            return new SumProjectionExpression(GetPropExpr(propertyName), true);
        }

        /// <summary>
        /// Sum aggregation function considering distinct values only.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static SumProjectionExpression SumDistinct(Expression expression)
        {
            return new SumProjectionExpression(expression, true);
        }

        /// <summary>
        /// Count aggregation function not counting values, equivalent to "Count(*)".
        /// </summary>
        /// <returns>expression</returns>
        public static CountStarProjectionExpression CountStar()
        {
            var expr = new CountStarProjectionExpression();
            expr.AddChild(new WildcardExpression());
            return expr;
        }

        /// <summary>
        /// Count aggregation function.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to count.</param>
        /// <returns>expression</returns>
        public static CountProjectionExpression Count(string propertyName)
        {
            return new CountProjectionExpression(GetPropExpr(propertyName), false);
        }

        /// <summary>
        /// Count aggregation function.
        /// </summary>
        /// <param name="expression">provides the values to count.</param>
        /// <returns>expression</returns>
        public static CountProjectionExpression Count(Expression expression)
        {
            return new CountProjectionExpression(expression, false);
        }

        /// <summary>
        /// Count aggregation function considering distinct values only.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to count.</param>
        /// <returns>expression</returns>
        public static CountProjectionExpression CountDistinct(string propertyName)
        {
            return new CountProjectionExpression(GetPropExpr(propertyName), true);
        }

        /// <summary>
        /// Count aggregation function considering distinct values only.
        /// </summary>
        /// <param name="expression">provides the values to count.</param>
        /// <returns>expression</returns>
        public static CountProjectionExpression CountDistinct(Expression expression)
        {
            return new CountProjectionExpression(expression, true);
        }

        /// <summary>
        /// Minimum aggregation function.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static MinProjectionExpression Min(string propertyName)
        {
            return new MinProjectionExpression(GetPropExpr(propertyName), false);
        }

        /// <summary>
        /// Minimum aggregation function.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static MinProjectionExpression Min(Expression expression)
        {
            return new MinProjectionExpression(expression, false);
        }

        /// <summary>
        /// Minimum aggregation function considering distinct values only.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static MinProjectionExpression MinDistinct(string propertyName)
        {
            return new MinProjectionExpression(GetPropExpr(propertyName), true);
        }

        /// <summary>
        /// Minimum aggregation function considering distinct values only.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static MinProjectionExpression MinDistinct(Expression expression)
        {
            return new MinProjectionExpression(expression, true);
        }

        /// <summary>
        /// Maximum aggregation function.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static MaxProjectionExpression Max(string propertyName)
        {
            return new MaxProjectionExpression(GetPropExpr(propertyName), false);
        }

        /// <summary>
        /// Maximum aggregation function.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static MaxProjectionExpression Max(Expression expression)
        {
            return new MaxProjectionExpression(expression, false);
        }

        /// <summary>
        /// Maximum aggregation function considering distinct values only.
        /// </summary>
        /// <param name="propertyName">name of the property providing the values to aggregate.</param>
        /// <returns>expression</returns>
        public static MaxProjectionExpression MaxDistinct(string propertyName)
        {
            return new MaxProjectionExpression(GetPropExpr(propertyName), true);
        }

        /// <summary>
        /// Maximum aggregation function considering distinct values only.
        /// </summary>
        /// <param name="expression">provides the values to aggregate.</param>
        /// <returns>expression</returns>
        public static MaxProjectionExpression MaxDistinct(Expression expression)
        {
            return new MaxProjectionExpression(expression, true);
        }

        /// <summary>
        /// Modulo.
        /// </summary>
        /// <param name="left">the expression providing left hand values</param>
        /// <param name="right">the expression providing right hand values</param>
        /// <returns>expression</returns>
        public static ArithmaticExpression Modulo(Expression left, Expression right)
        {
            return new ArithmaticExpression(left, "%", right);
        }

        /// <summary>
        /// Modulo.
        /// </summary>
        /// <param name="propertyLeft">the name of the property providing left hand values</param>
        /// <param name="propertyRight">the name of the property providing right hand values</param>
        /// <returns>expression</returns>
        public static ArithmaticExpression Modulo(string propertyLeft, string propertyRight)
        {
            return new ArithmaticExpression(
                new PropertyValueExpression(propertyLeft), "%", new PropertyValueExpression(propertyRight));
        }

        /// <summary>
        /// Subtraction.
        /// </summary>
        /// <param name="left">the expression providing left hand values</param>
        /// <param name="right">the expression providing right hand values</param>
        /// <returns>expression</returns>
        public static ArithmaticExpression Minus(Expression left, Expression right)
        {
            return new ArithmaticExpression(left, "-", right);
        }

        /// <summary>
        /// Subtraction.
        /// </summary>
        /// <param name="propertyLeft">the name of the property providing left hand values</param>
        /// <param name="propertyRight">the name of the property providing right hand values</param>
        /// <returns>expression</returns>
        public static ArithmaticExpression Minus(string propertyLeft, string propertyRight)
        {
            return new ArithmaticExpression(
                new PropertyValueExpression(propertyLeft), "-", new PropertyValueExpression(propertyRight));
        }

        /// <summary>
        /// Addition.
        /// </summary>
        /// <param name="left">the expression providing left hand values</param>
        /// <param name="right">the expression providing right hand values</param>
        /// <returns>expression</returns>
        public static ArithmaticExpression Plus(Expression left, Expression right)
        {
            return new ArithmaticExpression(left, "+", right);
        }

        /// <summary>
        /// Addition.
        /// </summary>
        /// <param name="propertyLeft">the name of the property providing left hand values</param>
        /// <param name="propertyRight">the name of the property providing right hand values</param>
        /// <returns>expression</returns>
        public static ArithmaticExpression Plus(string propertyLeft, string propertyRight)
        {
            return new ArithmaticExpression(
                new PropertyValueExpression(propertyLeft), "+", new PropertyValueExpression(propertyRight));
        }

        /// <summary>
        /// Multiplication.
        /// </summary>
        /// <param name="left">the expression providing left hand values</param>
        /// <param name="right">the expression providing right hand values</param>
        /// <returns>expression</returns>
        public static ArithmaticExpression Multiply(Expression left, Expression right)
        {
            return new ArithmaticExpression(left, "*", right);
        }

        /// <summary>
        /// Multiplication.
        /// </summary>
        /// <param name="propertyLeft">the name of the property providing left hand values</param>
        /// <param name="propertyRight">the name of the property providing right hand values</param>
        /// <returns>expression</returns>
        public static ArithmaticExpression Multiply(string propertyLeft, string propertyRight)
        {
            return new ArithmaticExpression(
                new PropertyValueExpression(propertyLeft), "*", new PropertyValueExpression(propertyRight));
        }

        /// <summary>
        /// Division.
        /// </summary>
        /// <param name="left">the expression providing left hand values</param>
        /// <param name="right">the expression providing right hand values</param>
        /// <returns>expression</returns>
        public static ArithmaticExpression Divide(Expression left, Expression right)
        {
            return new ArithmaticExpression(left, "/", right);
        }

        /// <summary>
        /// Division.
        /// </summary>
        /// <param name="propertyLeft">the name of the property providing left hand values</param>
        /// <param name="propertyRight">the name of the property providing right hand values</param>
        /// <returns>expression</returns>
        public static ArithmaticExpression Divide(string propertyLeft, string propertyRight)
        {
            return new ArithmaticExpression(
                new PropertyValueExpression(propertyLeft), "/", new PropertyValueExpression(propertyRight));
        }

        /// <summary>
        /// Concatenation.
        /// </summary>
        /// <param name="property">the name of property returning values to concatenate</param>
        /// <param name="properties">the names of additional properties returning values to concatenate</param>
        /// <returns>expression</returns>
        public static ConcatExpression Concat(string property, params string[] properties)
        {
            var concat = new ConcatExpression();
            concat.Children.Add(new PropertyValueExpression(property));
            concat.Children.AddAll(ToPropertyExpressions(properties));
            return concat;
        }

        /// <summary>
        /// Subquery.
        /// </summary>
        /// <param name="model">is the object model of the lookup</param>
        /// <returns>expression</returns>
        public static SubqueryExpression Subquery(EPStatementObjectModel model)
        {
            return new SubqueryExpression(model);
        }

        /// <summary>
        /// Subquery with in-clause, represents the syntax of "value in (select ... from ...)".
        /// </summary>
        /// <param name="property">is the name of the property that returns the value to match against the values returned by the lookup</param>
        /// <param name="model">is the object model of the lookup</param>
        /// <returns>expression</returns>
        public static SubqueryInExpression SubqueryIn(string property, EPStatementObjectModel model)
        {
            return new SubqueryInExpression(GetPropExpr(property), model, false);
        }

        /// <summary>
        /// Subquery with not-in-clause, represents the syntax of "value not in (select ... from ...)".
        /// </summary>
        /// <param name="property">is the name of the property that returns the value to match against the values returned by the lookup</param>
        /// <param name="model">is the object model of the lookup</param>
        /// <returns>expression</returns>
        public static SubqueryInExpression SubqueryNotIn(string property, EPStatementObjectModel model)
        {
            return new SubqueryInExpression(GetPropExpr(property), model, true);
        }

        /// <summary>
        /// Subquery with exists-clause, represents the syntax of "select * from ... where exists (select ... from ...)".
        /// </summary>
        /// <param name="model">is the object model of the lookup</param>
        /// <returns>expression</returns>
        public static SubqueryExistsExpression SubqueryExists(EPStatementObjectModel model)
        {
            return new SubqueryExistsExpression(model);
        }

        /// <summary>
        /// Subquery with in-clause, represents the syntax of "value in (select ... from ...)".
        /// </summary>
        /// <param name="expression">returns the value to match against the values returned by the lookup</param>
        /// <param name="model">is the object model of the lookup</param>
        /// <returns>expression</returns>
        public static SubqueryInExpression SubqueryIn(Expression expression, EPStatementObjectModel model)
        {
            return new SubqueryInExpression(expression, model, false);
        }

        /// <summary>
        /// Subquery with not-in-clause, represents the syntax of "value not in (select ... from ...)".
        /// </summary>
        /// <param name="expression">returns the value to match against the values returned by the lookup</param>
        /// <param name="model">is the object model of the lookup</param>
        /// <returns>expression</returns>
        public static SubqueryInExpression SubqueryNotIn(Expression expression, EPStatementObjectModel model)
        {
            return new SubqueryInExpression(expression, model, true);
        }

        /// <summary>
        /// Returns a time period expression for the specified parts.
        /// <para>
        /// Each part can be a null value in which case the part is left out.
        /// </para>
        /// </summary>
        /// <param name="days">day part</param>
        /// <param name="hours">hour part</param>
        /// <param name="minutes">minute part</param>
        /// <param name="seconds">seconds part</param>
        /// <param name="milliseconds">milliseconds part</param>
        /// <returns>time period expression</returns>
        public static TimePeriodExpression TimePeriod(
            double? days,
            double? hours,
            double? minutes,
            double? seconds,
            double? milliseconds)
        {
            Expression daysExpr = (days != null) ? Constant(days) : null;
            Expression hoursExpr = (hours != null) ? Constant(hours) : null;
            Expression minutesExpr = (minutes != null) ? Constant(minutes) : null;
            Expression secondsExpr = (seconds != null) ? Constant(seconds) : null;
            Expression millisecondsExpr = (milliseconds != null) ? Constant(milliseconds) : null;
            return new TimePeriodExpression(daysExpr, hoursExpr, minutesExpr, secondsExpr, millisecondsExpr);
        }

        /// <summary>
        /// Returns a time period expression for the specified parts.
        /// <para />Each part can be a null value in which case the part is left out.
        /// <para />Each object value may be a String value for an event property, or a number for a constant.
        /// </summary>
        /// <param name="days">day part</param>
        /// <param name="hours">hour part</param>
        /// <param name="minutes">minute part</param>
        /// <param name="seconds">seconds part</param>
        /// <param name="milliseconds">milliseconds part</param>
        /// <returns>time period expression</returns>
        public static TimePeriodExpression TimePeriod(
            int? days, 
            int? hours, 
            int? minutes, 
            int? seconds, 
            int? milliseconds)
        {
            Expression daysExpr = ConvertVariableNumeric(days);
            Expression hoursExpr = ConvertVariableNumeric(hours);
            Expression minutesExpr = ConvertVariableNumeric(minutes);
            Expression secondsExpr = ConvertVariableNumeric(seconds);
            Expression millisecondsExpr = ConvertVariableNumeric(milliseconds);
            return new TimePeriodExpression(daysExpr, hoursExpr, minutesExpr, secondsExpr, millisecondsExpr);
        }

        /// <summary>
        /// Returns a time period expression for the specified parts.
        /// <para>
        /// Each part can be a null value in which case the part is left out.
        /// </para>
        /// <para>
        /// Each object value may be a string value for an event property, or a number for a constant.
        /// </para>
        /// </summary>
        /// <param name="days">day part</param>
        /// <param name="hours">hour part</param>
        /// <param name="minutes">minute part</param>
        /// <param name="seconds">seconds part</param>
        /// <param name="milliseconds">milliseconds part</param>
        /// <returns>time period expression</returns>
        public static TimePeriodExpression TimePeriod(
            Object days,
            Object hours,
            Object minutes,
            Object seconds,
            Object milliseconds)
        {
            Expression daysExpr = ConvertVariableNumeric(days);
            Expression hoursExpr = ConvertVariableNumeric(hours);
            Expression minutesExpr = ConvertVariableNumeric(minutes);
            Expression secondsExpr = ConvertVariableNumeric(seconds);
            Expression millisecondsExpr = ConvertVariableNumeric(milliseconds);
            return new TimePeriodExpression(daysExpr, hoursExpr, minutesExpr, secondsExpr, millisecondsExpr);
        }

        /// <summary>
        /// Creates a wildcard parameter.
        /// </summary>
        /// <returns>parameter</returns>
        public static CrontabParameterExpression CrontabScheduleWildcard()
        {
            return new CrontabParameterExpression(ScheduleItemType.WILDCARD);
        }

        /// <summary>
        /// Creates a parameter of the given type and parameterized by a number.
        /// </summary>
        /// <param name="parameter">the constant parameter for the type</param>
        /// <param name="type">the type of crontab parameter</param>
        /// <returns>crontab parameter</returns>
        public static CrontabParameterExpression CrontabScheduleItem(int? parameter, ScheduleItemType type)
        {
            var param = new CrontabParameterExpression(type);
            if (parameter != null)
            {
                param.AddChild(Expressions.Constant(parameter));
            }
            return param;
        }

        /// <summary>
        /// Creates a frequency cron parameter.
        /// </summary>
        /// <param name="frequency">the constant for the frequency</param>
        /// <returns>cron parameter</returns>
        public static CrontabFrequencyExpression CrontabScheduleFrequency(int frequency)
        {
            return new CrontabFrequencyExpression(Constant(frequency));
        }

        /// <summary>
        /// Creates a range cron parameter.
        /// </summary>
        /// <param name="lowerBounds">the lower bounds</param>
        /// <param name="upperBounds">the upper bounds</param>
        /// <returns>crontab parameter</returns>
        public static CrontabRangeExpression CrontabScheduleRange(int lowerBounds, int upperBounds)
        {
            return new CrontabRangeExpression(Constant(lowerBounds), Constant(upperBounds));
        }

        /// <summary>
        /// Returns a list of expressions returning property values for the property names passed in.
        /// </summary>
        /// <param name="properties">is a list of property names</param>
        /// <returns>list of property value expressions</returns>
        internal static List<PropertyValueExpression> ToPropertyExpressions(params string[] properties)
        {
            var expr = new List<PropertyValueExpression>();
            foreach (string property in properties)
            {
                expr.Add(GetPropExpr(property));
            }
            return expr;
        }

        /// <summary>
        /// Returns an expression returning the propertyName value for the propertyName name passed in.
        /// </summary>
        /// <param name="propertyName">the name of the property returning property values</param>
        /// <returns>expression</returns>
        internal static PropertyValueExpression GetPropExpr(string propertyName)
        {
            return new PropertyValueExpression(propertyName);
        }

        private static Expression ConvertVariableNumeric(Object @object)
        {
            if (@object == null)
            {
                return null;
            }
            if (@object is string)
            {
                return Property(@object.ToString());
            }
            if (@object.IsNumber())
            {
                return Constant(@object);
            }
            throw new ArgumentException("Invalid object value, expecting string or numeric value");
        }
    }
} // end of namespace
