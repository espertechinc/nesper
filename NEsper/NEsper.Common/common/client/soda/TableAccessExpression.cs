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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Table access expression.
    /// </summary>
    public class TableAccessExpression : ExpressionBase
    {
        private string tableName;
        private IList<Expression> keyExpressions;
        private string optionalColumn;
        private Expression optionalAggregate;

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
        public TableAccessExpression(
            string tableName,
            IList<Expression> keyExpressions,
            string optionalColumn,
            Expression optionalAggregate)
        {
            this.tableName = tableName;
            this.keyExpressions = keyExpressions;
            this.optionalColumn = optionalColumn;
            this.optionalAggregate = optionalAggregate;
        }

        public override ExpressionPrecedenceEnum Precedence {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(tableName);
            if (keyExpressions != null && !keyExpressions.IsEmpty()) {
                writer.Write("[");
                ExpressionBase.ToPrecedenceFreeEPL(keyExpressions, writer);
                writer.Write("]");
            }

            if (optionalColumn != null) {
                writer.Write(".");
                writer.Write(optionalColumn);
            }

            if (optionalAggregate != null) {
                writer.Write(".");
                optionalAggregate.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }

        /// <summary>
        /// Returns the table name.
        /// </summary>
        /// <returns>table name</returns>
        public string TableName {
            get => tableName;
        }

        /// <summary>
        /// Sets the table name.
        /// </summary>
        /// <param name="tableName">table name</param>
        public void SetTableName(string tableName)
        {
            this.tableName = tableName;
        }

        /// <summary>
        /// Returns the primary key expressions.
        /// </summary>
        /// <returns>primary key expressions</returns>
        public IList<Expression> KeyExpressions {
            get => keyExpressions;
        }

        /// <summary>
        /// Sets the primary key expressions.
        /// </summary>
        /// <param name="keyExpressions">primary key expressions</param>
        public void SetKeyExpressions(IList<Expression> keyExpressions)
        {
            this.keyExpressions = keyExpressions;
        }

        /// <summary>
        /// Returns the optional table column name to access.
        /// </summary>
        /// <returns>table column name or null if accessing row</returns>
        public string OptionalColumn {
            get => optionalColumn;
        }

        /// <summary>
        /// Sets the optional table column name to access.
        /// </summary>
        /// <param name="optionalColumn">table column name or null if accessing row</param>
        public void SetOptionalColumn(string optionalColumn)
        {
            this.optionalColumn = optionalColumn;
        }

        /// <summary>
        /// Returns the optional table column aggregation accessor to use.
        /// </summary>
        /// <returns>table column aggregation accessor</returns>
        public Expression OptionalAggregate {
            get => optionalAggregate;
        }

        /// <summary>
        /// Sets the optional table column aggregation accessor to use.
        /// </summary>
        /// <param name="optionalAggregate">table column aggregation accessor</param>
        public void SetOptionalAggregate(Expression optionalAggregate)
        {
            this.optionalAggregate = optionalAggregate;
        }
    }
} // end of namespace