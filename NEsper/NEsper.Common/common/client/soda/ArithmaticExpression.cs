///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Arithmatic expression for addition, subtraction, multiplication, division and modulo.
    /// </summary>
    [Serializable]
    public class ArithmaticExpression : ExpressionBase
    {
        private string @operator;

        /// <summary>
        /// Ctor.
        /// </summary>
        public ArithmaticExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="operator">can be any of '-', '+', '*', '/' or '%' (modulo).</param>
        public ArithmaticExpression(string @operator)
        {
            this.@operator = @operator;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="left">the left hand side</param>
        /// <param name="operator">can be any of '-', '+', '*', '/' or '%' (modulo).</param>
        /// <param name="right">the right hand side</param>
        public ArithmaticExpression(
            Expression left,
            string @operator,
            Expression right)
        {
            this.@operator = @operator;
            AddChild(left);
            AddChild(right);
        }

        /// <summary>
        /// Returns the arithmatic operator.
        /// </summary>
        /// <returns>operator</returns>
        public string Operator {
            get => @operator;
            set => this.@operator = value;
        }

        /// <summary>
        /// Add a constant to include in the computation.
        /// </summary>
        /// <param name="object">constant to add</param>
        /// <returns>expression</returns>
        public ArithmaticExpression Add(object @object)
        {
            this.Children.Add(new ConstantExpression(@object));
            return this;
        }

        /// <summary>
        /// Add an expression to include in the computation.
        /// </summary>
        /// <param name="expression">to add</param>
        /// <returns>expression</returns>
        public ArithmaticExpression Add(Expression expression)
        {
            this.Children.Add(expression);
            return this;
        }

        /// <summary>
        /// Add a property to include in the computation.
        /// </summary>
        /// <param name="propertyName">is the name of the property</param>
        /// <returns>expression</returns>
        public ArithmaticExpression Add(string propertyName)
        {
            this.Children.Add(new PropertyValueExpression(propertyName));
            return this;
        }

        public override ExpressionPrecedenceEnum Precedence {
            get {
                if (@operator.Equals("*") || @operator.Equals("/") || @operator.Equals("%")) {
                    return ExpressionPrecedenceEnum.MULTIPLY;
                }
                else {
                    return ExpressionPrecedenceEnum.ADDITIVE;
                }
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            string delimiter = "";
            foreach (Expression child in this.Children) {
                writer.Write(delimiter);
                child.ToEPL(writer, Precedence);
                delimiter = @operator;
            }
        }
    }
} // end of namespace