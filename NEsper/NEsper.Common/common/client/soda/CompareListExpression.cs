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
    ///     Represents a list-compare of the format "expression operator all/any (expressions)".
    /// </summary>
    [Serializable]
    public class CompareListExpression : ExpressionBase
    {
        private string @operator;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public CompareListExpression()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="all">is all, false if any</param>
        /// <param name="operator">=, !=, &amp;lt;, &amp;gt;, &amp;lt;=, &amp;gt;=, &amp;lt;&amp;gt;</param>
        public CompareListExpression(
            bool all,
            string @operator)
        {
            IsAll = all;
            this.@operator = @operator;
        }

        /// <summary>
        ///     Returns all flag, true for ALL and false for ANY.
        /// </summary>
        /// <returns>indicator if all or any</returns>
        public bool IsAll { get; private set; }

        /// <summary>
        ///     Returns all flag, true for ALL and false for ANY.
        /// </summary>
        /// <returns>indicator if all or any</returns>
        public bool All => IsAll;

        /// <summary>
        ///     Returns the operator.
        /// </summary>
        /// <returns>operator</returns>
        public string Operator
        {
            get => @operator;
            set => @operator = value;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.RELATIONAL_BETWEEN_IN;

        /// <summary>
        ///     Sets all flag, true for ALL and false for ANY.
        /// </summary>
        /// <param name="all">indicator if all or any</param>
        public void SetAll(bool all)
        {
            IsAll = all;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            Children[0].ToEPL(writer, Precedence);
            writer.Write(@operator);
            if (IsAll)
            {
                writer.Write("all(");
            }
            else
            {
                writer.Write("any(");
            }

            var delimiter = "";
            for (var i = 1; i < Children.Count; i++)
            {
                writer.Write(delimiter);
                Children[i].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }

            writer.Write(')');
        }
    }
} // end of namespace