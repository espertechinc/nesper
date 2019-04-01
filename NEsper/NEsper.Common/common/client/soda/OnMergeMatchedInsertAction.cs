///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// <summary>For use with on-merge clauses, inserts into a named window if matching rows are not found. </summary>
    public class OnMergeMatchedInsertAction : OnMergeMatchedAction
    {
        /// <summary>Ctor. </summary>
        /// <param name="columnNames">insert-into column names, or empty list if none provided</param>
        /// <param name="selectList">select expression list</param>
        /// <param name="whereClause">optional condition or null</param>
        /// <param name="optionalStreamName">optionally a stream name for insert-into</param>
        public OnMergeMatchedInsertAction(IList<string> columnNames,
                                          IList<SelectClauseElement> selectList,
                                          Expression whereClause,
                                          String optionalStreamName)
        {
            ColumnNames = columnNames;
            SelectList = selectList;
            WhereClause = whereClause;
            OptionalStreamName = optionalStreamName;
        }

        /// <summary>Ctor. </summary>
        public OnMergeMatchedInsertAction()
        {
            ColumnNames = new List<string>();
            SelectList = new List<SelectClauseElement>();
        }

        /// <summary>Returns the action condition, or null if undefined. </summary>
        /// <value>condition</value>
        public Expression WhereClause { get; set; }

        /// <summary>Returns the insert-into column names, if provided. </summary>
        /// <value>column names</value>
        public IList<string> ColumnNames { get; set; }

        /// <summary>Returns the select expressions. </summary>
        /// <value>expression list</value>
        public IList<SelectClauseElement> SelectList { get; set; }

        /// <summary>Returns the insert-into stream name. </summary>
        /// <value>stream name</value>
        public string OptionalStreamName { get; set; }

        #region OnMergeMatchedAction Members

        public void ToEPL(TextWriter writer)
        {
            writer.Write("then insert");
            if (OptionalStreamName != null)
            {
                writer.Write(" into ");
                writer.Write(OptionalStreamName);
            }

            string delimiter;
            if (ColumnNames.Count > 0)
            {
                writer.Write("(");
                delimiter = "";
                foreach (String name in ColumnNames)
                {
                    writer.Write(delimiter);
                    writer.Write(name);
                    delimiter = ", ";
                }
                writer.Write(")");
            }
            writer.Write(" select ");
            delimiter = "";
            foreach (SelectClauseElement element in SelectList)
            {
                writer.Write(delimiter);
                element.ToEPLElement(writer);
                delimiter = ", ";
            }
            if (WhereClause != null)
            {
                writer.Write(" where ");
                WhereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }

        #endregion
    }
}