///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// Represents a plug-in aggregation function.
    /// </summary>
    [Serializable]
    public class PlugInProjectionExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public PlugInProjectionExpression() {
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="functionName">the name of the function</param>
        /// <param name="isDistinct">true for distinct</param>
        public PlugInProjectionExpression(string functionName, bool isDistinct)
        {
            FunctionName = functionName;
            IsDistinct = isDistinct;
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="functionName">the name of the function</param>
        /// <param name="isDistinct">true for distinct</param>
        /// <param name="moreExpressions">provides aggregated values</param>
        public PlugInProjectionExpression(string functionName, bool isDistinct, params Expression[] moreExpressions)
        {
            FunctionName = functionName;
            IsDistinct = isDistinct;
            for (int i = 0; i < moreExpressions.Length; i++)
            {
                Children.Add(moreExpressions[i]);
            }
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            RenderAggregation(writer, FunctionName, IsDistinct, Children);
        }

        /// <summary>
        /// Returns the function name.
        /// </summary>
        /// <value>name of function</value>
        public string FunctionName { get; set; }

        /// <summary>
        /// Returns true for distinct.
        /// </summary>
        /// <value>boolean indicating distinct or not</value>
        public bool IsDistinct { get; set; }
    }
}
