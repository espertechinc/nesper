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
    ///     The "new" operator is useful to format an event or event property from a list of column names and expressions.
    ///     <para />
    ///     Useful with enumeration methods and with case-when clauses that return multiple result values, for example.
    ///     <para />
    ///     Column names are part of the state and the number of column names must match the number of sub-expressions to the
    ///     expression.
    /// </summary>
    [Serializable]
    public class NewOperatorExpression : ExpressionBase
    {
        private IList<string> columnNames;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public NewOperatorExpression()
        {
        }

        /// <summary>
        ///     Ctor.
        ///     <para />
        ///     The list of column names should match the number of expressions provided hereunder.
        /// </summary>
        /// <param name="columnNames">list of column names</param>
        public NewOperatorExpression(IList<string> columnNames)
        {
            this.columnNames = columnNames;
        }

        /// <summary>
        ///     Returns the column names.
        /// </summary>
        /// <returns>colum names</returns>
        public IList<string> ColumnNames
        {
            get => columnNames;
            set => columnNames = value;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.NEGATED;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("new{");
            var delimiter = "";
            for (var i = 0; i < Children.Count; i++)
            {
                writer.Write(delimiter);
                writer.Write(columnNames[i]);
                var expr = Children[i];

                var outputexpr = true;
                if (expr is PropertyValueExpression)
                {
                    var prop = (PropertyValueExpression) expr;
                    if (prop.PropertyName.Equals(columnNames[i]))
                    {
                        outputexpr = false;
                    }
                }

                if (outputexpr)
                {
                    writer.Write("=");
                    expr.ToEPL(writer, Precedence);
                }

                delimiter = ",";
            }

            writer.Write("}");
        }
    }
} // end of namespace