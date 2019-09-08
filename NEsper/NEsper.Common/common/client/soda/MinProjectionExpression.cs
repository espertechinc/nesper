///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Minimum of the (distinct) values returned by an expression.
    /// </summary>
    [Serializable]
    public class MinProjectionExpression : ExpressionBase
    {
        private bool distinct;
        private bool ever;

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
            this.distinct = isDistinct;
        }

        /// <summary>
        /// Ctor - for use to create an expression tree, without inner expression
        /// </summary>
        /// <param name="isDistinct">true if distinct</param>
        /// <param name="isEver">ever-indicator</param>
        public MinProjectionExpression(
            bool isDistinct,
            bool isEver)
        {
            this.distinct = isDistinct;
            this.ever = isEver;
        }

        /// <summary>
        /// Ctor - adds the expression to project.
        /// </summary>
        /// <param name="expression">returning values to project</param>
        /// <param name="isDistinct">true if distinct</param>
        public MinProjectionExpression(
            Expression expression,
            bool isDistinct)
        {
            this.distinct = isDistinct;
            this.Children.Add(expression);
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            string name;
            if (this.Children.Count > 1)
            {
                name = "fmin";
            }
            else
            {
                if (ever)
                {
                    name = "minever";
                }
                else
                {
                    name = "min";
                }
            }

            ExpressionBase.RenderAggregation(writer, name, distinct, this.Children);
        }

        /// <summary>
        /// Returns true if the projection considers distinct values only.
        /// </summary>
        /// <returns>true if distinct</returns>
        public bool IsDistinct
        {
            get => distinct;
        }

        /// <summary>
        /// Returns true if the projection considers distinct values only.
        /// </summary>
        /// <returns>true if distinct</returns>
        public bool Distinct {
            get => distinct;
            set => distinct = value;
        }

        /// <summary>
        /// Set the distinct flag indicating the projection considers distinct values only.
        /// </summary>
        /// <param name="distinct">true for distinct, false for not distinct</param>
        public MinProjectionExpression SetDistinct(bool distinct)
        {
            this.distinct = distinct;
            return this;
        }

        /// <summary>
        /// Returns true for max-ever
        /// </summary>
        /// <returns>indicator for "ever"</returns>
        public bool IsEver {
            get => ever;
            set => ever = value;
        }

        /// <summary>
        /// Set to true for max-ever
        /// </summary>
        /// <param name="ever">indicator for "ever"</param>
        public MinProjectionExpression SetEver(bool ever)
        {
            this.ever = ever;
            return this;
        }
    }
} // end of namespace