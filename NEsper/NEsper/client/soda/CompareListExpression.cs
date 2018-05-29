///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Represents a list-compare of the format "expression operator all/any (expressions)".
    /// </summary>
    [Serializable]
    public class CompareListExpression
        : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public CompareListExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="all">is all, false if any</param>
        /// <param name="operator">=, !=, &lt;, &gt;, &lt;=, &gt;=, &lt;&gt;</param>
        public CompareListExpression(bool all, String @operator)
        {
            IsAll = all;
            Operator = @operator;
        }

        /// <summary>
        /// Returns all flag, true for ALL and false for ANY.
        /// </summary>
        /// <value>indicator if all or any</value>
        public bool IsAll { get; private set; }

        /// <summary>Returns the operator. </summary>
        /// <value>operator</value>
        public string Operator { get; set; }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.RELATIONAL_BETWEEN_IN; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            Children[0].ToEPL(writer, Precedence);
            writer.Write(Operator);
            if (IsAll)
            {
                writer.Write("all(");
            }
            else
            {
                writer.Write("any(");
            }

            String delimiter = "";
            for (int i = 1; i < Children.Count; i++)
            {
                writer.Write(delimiter);
                Children[i].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }
            writer.Write(')');
        }
    }
}
