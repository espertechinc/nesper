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
                this.Children.Add(moreExpressions[i]);
            }
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ExpressionBase.RenderAggregation(writer, functionName, isDistinct, this.Children);
        }

        /// <summary>
        /// Returns the function name.
        /// </summary>
        /// <returns>name of function</returns>
        public string FunctionName
        {
            get => functionName;
        }

        /// <summary>
        /// Sets the function name.
        /// </summary>
        /// <param name="functionName">name of function</param>
        public void SetFunctionName(string functionName)
        {
            this.functionName = functionName;
        }

        /// <summary>
        /// Returns true for distinct.
        /// </summary>
        /// <returns>boolean indicating distinct or not</returns>
        public bool IsDistinct
        {
            get => isDistinct;
        }

        /// <summary>
        /// Set to true for distinct.
        /// </summary>
        /// <param name="distinct">indicating distinct or not</param>
        public void SetDistinct(bool distinct)
        {
            isDistinct = distinct;
        }
    }
} // end of namespace