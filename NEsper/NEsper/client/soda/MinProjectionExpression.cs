///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Minimum of the (distinct) values returned by an expression.
    /// </summary>
    [Serializable]
    public class MinProjectionExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public MinProjectionExpression()
        {
        }
    
        /// <summary>
        /// Ctor - for use to create an expression tree, without inner expression
        /// </summary>
        /// <param name="isDistinct">true if distinct</param>
        public MinProjectionExpression(bool isDistinct)
        {
            this.IsDistinct = isDistinct;
        }
    
        /// <summary>
        /// Ctor - for use to create an expression tree, without inner expression
        /// </summary>
        /// <param name="isDistinct">true if distinct</param>
        /// <param name="isEver">ever-indicator</param>
        public MinProjectionExpression(bool isDistinct, bool isEver)
        {
            this.IsDistinct = isDistinct;
            this.IsEver = isEver;
        }
    
        /// <summary>
        /// Ctor - adds the expression to project.
        /// </summary>
        /// <param name="expression">returning values to project</param>
        /// <param name="isDistinct">true if distinct</param>
        public MinProjectionExpression(Expression expression, bool isDistinct)
        {
            this.IsDistinct = isDistinct;
            this.Children.Add(expression);
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            string name;
            if (this.Children.Count > 1) {
                name = "fmin";
            }
            else {
                if (IsEver) {
                    name = "minever";
                }
                else {
                    name = "min";
                }
            }
            ExpressionBase.RenderAggregation(writer, name, IsDistinct, this.Children);
        }

        /// <summary>
        /// Returns true if the projection considers distinct values only.
        /// </summary>
        /// <value>true if distinct</value>
        public bool IsDistinct { get; set; }

        /// <summary>
        /// Returns true for max-ever
        /// </summary>
        /// <value>indicator for "ever"</value>
        public bool IsEver { get; set; }
    }
}
