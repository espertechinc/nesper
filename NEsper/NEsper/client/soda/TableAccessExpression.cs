///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Table access expression.
    /// </summary>
    [Serializable]
    public class TableAccessExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public TableAccessExpression()
        {
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="tableName">the table name</param>
        /// <param name="keyExpressions">the list of key expressions for each table primary key in the same order as declared</param>
        /// <param name="optionalColumn">optional column name</param>
        /// <param name="optionalAggregate">optional aggregation function</param>
        public TableAccessExpression(string tableName, IList<Expression> keyExpressions, string optionalColumn, Expression optionalAggregate)
        {
            TableName = tableName;
            KeyExpressions = keyExpressions;
            OptionalColumn = optionalColumn;
            OptionalAggregate = optionalAggregate;
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(TableName);
            if (KeyExpressions != null && !KeyExpressions.IsEmpty()) {
                writer.Write("[");
                ToPrecedenceFreeEPL(KeyExpressions, writer);
                writer.Write("]");
            }
            if (OptionalColumn != null) {
                writer.Write(".");
                writer.Write(OptionalColumn);
            }
            if (OptionalAggregate != null) {
                writer.Write(".");
                OptionalAggregate.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }

        /// <summary>
        /// Returns the table name.
        /// </summary>
        /// <value>table name</value>
        public string TableName { get; set; }

        /// <summary>
        /// Returns the primary key expressions.
        /// </summary>
        /// <value>primary key expressions</value>
        public IList<Expression> KeyExpressions { get; set; }

        /// <summary>
        /// Returns the optional table column name to access.
        /// </summary>
        /// <value>table column name or null if accessing row</value>
        public string OptionalColumn { get; set; }

        /// <summary>
        /// Returns the optional table column aggregation accessor to use.
        /// </summary>
        /// <value>table column aggregation accessor</value>
        public Expression OptionalAggregate { get; set; }
    }
}
