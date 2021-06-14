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
    /// Represents a plug-in aggregation function.
    /// </summary>
    [Serializable]
    public class PlugInProjectionExpression : ExpressionBase
    {
        private string functionName;
        private bool isDistinct;

        /// <summary>
        /// Ctor.
        /// </summary>
        public PlugInProjectionExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="functionName">the name of the function</param>
        /// <param name="isDistinct">true for distinct</param>
        public PlugInProjectionExpression(
            string functionName,
            bool isDistinct)
        {
            this.functionName = functionName;
            this.isDistinct = isDistinct;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="functionName">the name of the function</param>
        /// <param name="isDistinct">true for distinct</param>
        /// <param name="moreExpressions">provides aggregated values</param>
        public PlugInProjectionExpression(
            string functionName,
            bool isDistinct,
            params Expression[] moreExpressions)
        {
            this.functionName = functionName;
            this.isDistinct = isDistinct;
            for (int i = 0; i < moreExpressions.Length; i++)
            {
                Children.Add(moreExpressions[i]);
            }
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            RenderAggregation(writer, functionName, isDistinct, Children);
        }

        /// <summary>
        /// Returns the function name.
        /// </summary>
        /// <returns>name of function</returns>
        public string FunctionName {
            get => functionName;
            set => functionName = value;
        }

        /// <summary>
        /// Sets the function name.
        /// </summary>
        /// <param name="functionName">name of function</param>
        public PlugInProjectionExpression SetFunctionName(string functionName)
        {
            this.functionName = functionName;
            return this;
        }

        /// <summary>
        /// Returns true for distinct.
        /// </summary>
        /// <returns>boolean indicating distinct or not</returns>
        public bool IsDistinct {
            get => isDistinct;
            set => isDistinct = value;
        }

        /// <summary>
        /// Set to true for distinct.
        /// </summary>
        /// <param name="distinct">indicating distinct or not</param>
        public PlugInProjectionExpression SetDistinct(bool distinct)
        {
            isDistinct = distinct;
            return this;
        }
    }
} // end of namespace