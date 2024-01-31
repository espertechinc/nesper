///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    /// <summary>A clause to insert into zero, one or more streams based on criteria. </summary>
    public class OnInsertSplitStreamClause : OnClause
    {
        /// <summary>Ctor. </summary>
        public OnInsertSplitStreamClause()
        {
            Items = new List<OnInsertSplitStreamItem>();
        }

        /// <summary>Creates a split-stream on-insert clause from an indicator whether to consider the first of all where-clauses, and a list of items. </summary>
        /// <param name="isFirst">true for first where-clause, false for all where-clauses fire</param>
        /// <param name="items">is a list of insert-into, select and optional where-clauses</param>
        /// <returns>split-stream on-insert clause</returns>
        public static OnInsertSplitStreamClause Create(
            bool isFirst,
            List<OnInsertSplitStreamItem> items)
        {
            return new OnInsertSplitStreamClause(isFirst, items);
        }

        /// <summary>Creates an split-stream on-insert clause considering only the first where-clause that matches. </summary>
        /// <returns>split-stream on-insert clause</returns>
        public static OnInsertSplitStreamClause Create()
        {
            return new OnInsertSplitStreamClause(true, new List<OnInsertSplitStreamItem>());
        }

        /// <summary>Ctor. </summary>
        /// <param name="isFirst">indicator whether only the first where-clause is to match or all where-clauses.</param>
        /// <param name="items">tuples of insert-into, select and where-clauses.</param>
        public OnInsertSplitStreamClause(
            bool isFirst,
            List<OnInsertSplitStreamItem> items)
        {
            IsFirst = isFirst;
            Items = items;
        }

        /// <summary>Renders the clause in textual representation. </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">for NewLine-whitespace formatting</param>
        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            foreach (var item in Items) {
                item.InsertInto.ToEPL(writer, formatter, true);
                item.SelectClause.ToEPL(writer, formatter, false, false);
                if (item.PropertySelects != null) {
                    writer.Write(" from ");
                    ContainedEventSelect.ToEPL(writer, formatter, item.PropertySelects);
                    if (item.PropertySelectsStreamName != null) {
                        writer.Write(" as ");
                        writer.Write(item.PropertySelectsStreamName);
                    }
                }

                if (item.WhereClause != null) {
                    writer.Write(" where ");
                    item.WhereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                }
            }

            if (!IsFirst) {
                writer.Write(" output all");
            }
        }

        /// <summary>
        /// Returns true for firing the insert-into for only the first where-clause that matches, or false for firing the insert-into for all where-clauses that match.
        /// </summary>
        /// <value>indicator first or all</value>
        public bool IsFirst { get; set; }

        /// <summary>
        /// Returns a list of insert-into, select and where-clauses.
        /// </summary>
        /// <value>split-stream lines</value>
        public IList<OnInsertSplitStreamItem> Items { get; set; }

        /// <summary>
        /// Add a insert-into, select and where-clause.
        /// </summary>
        /// <param name="item">to add</param>
        public void AddItem(OnInsertSplitStreamItem item)
        {
            Items.Add(item);
        }
    }
}