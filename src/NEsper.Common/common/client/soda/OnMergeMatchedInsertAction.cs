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

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     For use with on-merge clauses, inserts into a named window if matching rows are not found.
    /// </summary>
    public class OnMergeMatchedInsertAction : OnMergeMatchedAction
    {
        private IList<string> columnNames = EmptyList<string>.Instance;
        private Expression eventPrecedence;
        private string optionalStreamName;
        private IList<SelectClauseElement> selectList = EmptyList<SelectClauseElement>.Instance;
        private Expression whereClause;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="columnNames">insert-into column names, or empty list if none provided</param>
        /// <param name="selectList">select expression list</param>
        /// <param name="whereClause">optional condition or null</param>
        /// <param name="optionalStreamName">optionally a stream name for insert-into</param>
        public OnMergeMatchedInsertAction(
            IList<string> columnNames,
            IList<SelectClauseElement> selectList,
            Expression whereClause,
            string optionalStreamName)
            : this(columnNames, null, selectList, whereClause, optionalStreamName)
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="columnNames">insert-into column names, or empty list if none provided</param>
        /// <param name="selectList">select expression list</param>
        /// <param name="whereClause">optional condition or null</param>
        /// <param name="optionalStreamName">optionally a stream name for insert-into</param>
        /// <param name="eventPrecedence">event precedence or null</param>
        public OnMergeMatchedInsertAction(
            IList<string> columnNames,
            Expression eventPrecedence,
            IList<SelectClauseElement> selectList,
            Expression whereClause,
            string optionalStreamName)
        {
            this.columnNames = columnNames;
            this.eventPrecedence = eventPrecedence;
            this.selectList = selectList;
            this.whereClause = whereClause;
            this.optionalStreamName = optionalStreamName;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        public OnMergeMatchedInsertAction()
        {
        }

        /// <summary>
        ///     Returns the action condition, or null if undefined.
        /// </summary>
        /// <value>condition</value>
        public Expression WhereClause {
            get => whereClause;
            set => whereClause = value;
        }

        /// <summary>
        ///     Returns the insert-into column names, if provided.
        /// </summary>
        /// <value>column names</value>
        public IList<string> ColumnNames {
            get => columnNames;
            set => columnNames = value;
        }

        /// <summary>
        ///     Returns the select expressions.
        /// </summary>
        /// <value>expression list</value>
        public IList<SelectClauseElement> SelectList {
            get => selectList;
            set => selectList = value;
        }

        /// <summary>
        ///     Returns the insert-into stream name.
        /// </summary>
        /// <value>stream name</value>
        public string OptionalStreamName {
            get => optionalStreamName;
            set => optionalStreamName = value;
        }

        /// <summary>
        ///     Returns null when no event-precedence is specified for the insert-into,
        ///     or returns the expression returning the event-precedence
        /// </summary>
        /// <value>event-precedence expression</value>
        public Expression EventPrecedence {
            get => eventPrecedence;
            set => eventPrecedence = value;
        }

        public void ToEPL(TextWriter writer)
        {
            writer.Write("insert");
            if (optionalStreamName != null) {
                writer.Write(" into ");
                writer.Write(optionalStreamName);
            }

            if (columnNames.Count > 0) {
                writer.Write("(");
                var delimiterX = "";
                foreach (var name in columnNames) {
                    writer.Write(delimiterX);
                    writer.Write(name);
                    delimiterX = ", ";
                }

                writer.Write(")");
            }

            if (eventPrecedence != null) {
                writer.Write(" event-precedence(");
                eventPrecedence.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(")");
            }

            writer.Write(" select ");
            var delimiter = "";
            foreach (var element in selectList) {
                writer.Write(delimiter);
                element.ToEPLElement(writer);
                delimiter = ", ";
            }

            if (whereClause != null) {
                writer.Write(" where ");
                whereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }
    }
} // end of namespace