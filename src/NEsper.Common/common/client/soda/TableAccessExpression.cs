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

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Table access expression.
    /// </summary>
    [Serializable]
    public class TableAccessExpression : ExpressionBase
    {
        private string _tableName;
        private IList<Expression> _keyExpressions;
        private string _optionalColumn;

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
        public TableAccessExpression(
            string tableName,
            IList<Expression> keyExpressions,
            string optionalColumn)
        {
            _tableName = tableName;
            _keyExpressions = keyExpressions;
            _optionalColumn = optionalColumn;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(_tableName);
            if (_keyExpressions != null && !_keyExpressions.IsEmpty()) {
                writer.Write("[");
                ExpressionBase.ToPrecedenceFreeEPL(_keyExpressions, writer);
                writer.Write("]");
            }

            if (_optionalColumn != null) {
                writer.Write(".");
                writer.Write(_optionalColumn);
            }
        }

        /// <summary>
        /// Returns the table name.
        /// </summary>
        /// <returns>table name</returns>
        public string TableName {
            get => _tableName;
            set => _tableName = value;
        }

        /// <summary>
        /// Sets the table name.
        /// </summary>
        /// <param name="tableName">table name</param>
        public TableAccessExpression SetTableName(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        /// <summary>
        /// Returns the primary key expressions.
        /// </summary>
        /// <returns>primary key expressions</returns>
        public IList<Expression> KeyExpressions {
            get => _keyExpressions;
            set => _keyExpressions = value;
        }

        /// <summary>
        /// Sets the primary key expressions.
        /// </summary>
        /// <param name="keyExpressions">primary key expressions</param>
        public TableAccessExpression SetKeyExpressions(IList<Expression> keyExpressions)
        {
            _keyExpressions = keyExpressions;
            return this;
        }

        /// <summary>
        /// Returns the optional table column name to access.
        /// </summary>
        /// <returns>table column name or null if accessing row</returns>
        public string OptionalColumn {
            get => _optionalColumn;
            set => _optionalColumn = value;
        }

        /// <summary>
        /// Sets the optional table column name to access.
        /// </summary>
        /// <param name="optionalColumn">table column name or null if accessing row</param>
        public TableAccessExpression SetOptionalColumn(string optionalColumn)
        {
            _optionalColumn = optionalColumn;
            return this;
        }
    }
} // end of namespace