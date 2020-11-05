///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// An item in a for-clause for controlling delivery of result events to listeners and subscribers.
    /// </summary>
    [Serializable]
    public class ForClauseItem
    {
        /// <summary>Ctor. <para /> Must set a keyword and optionally add expressions. </summary>
        public ForClauseItem()
        {
            Expressions = new List<Expression>();
        }

        /// <summary>Ctor. </summary>
        /// <param name="keyword">the delivery keyword</param>
        public ForClauseItem(ForClauseKeyword keyword)
            : this()
        {
            Keyword = keyword;
        }

        /// <summary>Returns the for-clause keyword. </summary>
        /// <value>keyword</value>
        public ForClauseKeyword? Keyword { get; set; }

        /// <summary>Returns for-clause expressions. </summary>
        /// <value>expressions</value>
        public IList<Expression> Expressions { get; set; }

        /// <summary>Creates a for-clause with no expressions. </summary>
        /// <param name="keyword">keyword to use</param>
        /// <returns>for-clause</returns>
        public static ForClauseItem Create(ForClauseKeyword keyword)
        {
            return new ForClauseItem(keyword);
        }

        /// <summary>Renders the clause in textual representation. </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            if (Keyword == null)
            {
                return;
            }

            writer.Write("for ");
            writer.Write(Keyword.GetValueOrDefault().GetName());
            if (Expressions.Count == 0)
            {
                return;
            }

            writer.Write("(");
            string delimiter = "";
            foreach (Expression child in Expressions)
            {
                writer.Write(delimiter);
                child.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                delimiter = ", ";
            }

            writer.Write(")");
        }
    }
}