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
    ///     A clause to insert, update or delete to/from a named window based on a triggering event arriving and correlated to
    ///     the named window events to be updated.
    /// </summary>
    [Serializable]
    public class OnMergeClause : OnClause
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public OnMergeClause()
        {
            MatchItems = new List<OnMergeMatchItem>();
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="windowName">is the named window name</param>
        /// <param name="optionalAsName">is the as-provided name of the named window</param>
        /// <param name="matchItems">is the matched and non-matched action items</param>
        public OnMergeClause(
            string windowName,
            string optionalAsName,
            IList<OnMergeMatchItem> matchItems)
        {
            WindowName = windowName;
            OptionalAsName = optionalAsName;
            MatchItems = matchItems;
        }

        /// <summary>
        ///     Returns the name of the named window to update.
        /// </summary>
        /// <returns>named window name</returns>
        public string WindowName { get; set; }

        /// <summary>
        ///     Returns the as-provided name for the named window.
        /// </summary>
        /// <returns>name or null</returns>
        public string OptionalAsName { get; set; }

        /// <summary>
        ///     Returns all actions.
        /// </summary>
        /// <returns>actions</returns>
        public IList<OnMergeMatchItem> MatchItems { get; set; }

        /// <summary>
        ///     Returns an optional insert to executed without a match-clause. If set indicates there is no match-clause.
        /// </summary>
        /// <returns>insert</returns>
        public OnMergeMatchedInsertAction InsertNoMatch { get; set; }

        /// <summary>
        ///     Creates an on-update clause.
        /// </summary>
        /// <param name="windowName">is the named window name</param>
        /// <param name="optionalAsName">is the optional as-provided name</param>
        /// <param name="matchItems">is the matched and non-matched action items</param>
        /// <returns>on-update clause without assignments</returns>
        public static OnMergeClause Create(
            string windowName,
            string optionalAsName,
            IList<OnMergeMatchItem> matchItems)
        {
            return new OnMergeClause(windowName, optionalAsName, matchItems);
        }

        /// <summary>
        ///     Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="optionalWhereClause">where clause if present, or null</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
        public void ToEPL(
            TextWriter writer,
            Expression optionalWhereClause,
            EPStatementFormatter formatter)
        {
            formatter.BeginMerge(writer);
            writer.Write("merge ");
            writer.Write(WindowName);

            if (OptionalAsName != null)
            {
                writer.Write(" as ");
                writer.Write(OptionalAsName);
            }

            if (InsertNoMatch != null)
            {
                writer.Write(" ");
                InsertNoMatch.ToEPL(writer);
            }
            else
            {
                if (optionalWhereClause != null)
                {
                    formatter.BeginMergeWhere(writer);
                    writer.Write("where ");
                    optionalWhereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                }

                foreach (var item in MatchItems)
                {
                    item.ToEPL(writer, formatter);
                }
            }
        }

        /// <summary>
        ///     Add a new action to the list of actions.
        /// </summary>
        /// <param name="action">to add</param>
        /// <returns>clause</returns>
        public OnMergeClause AddAction(OnMergeMatchItem action)
        {
            MatchItems.Add(action);
            return this;
        }
    }
} // end of namespace