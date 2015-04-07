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

namespace com.espertech.esper.client.soda
{
    /// <summary>A clause to insert, Update or delete to/from a named window based on a triggering event arriving and correlated to the named window events to be updated. </summary>
    public class OnMergeClause : OnClause
    {
        /// <summary>Ctor. </summary>
        public OnMergeClause()
        {
            MatchItems = new List<OnMergeMatchItem>();
        }

        /// <summary>Ctor. </summary>
        /// <param name="windowName">is the named window name</param>
        /// <param name="optionalAsName">is the as-provided name of the named window</param>
        /// <param name="matchItems">is the matched and non-matched action items</param>
        public OnMergeClause(String windowName,
                             String optionalAsName,
                             IList<OnMergeMatchItem> matchItems)
        {
            WindowName = windowName;
            OptionalAsName = optionalAsName;
            MatchItems = matchItems;
        }

        /// <summary>Returns the name of the named window to Update. </summary>
        /// <value>named window name</value>
        public string WindowName { get; set; }

        /// <summary>Returns the as-provided name for the named window. </summary>
        /// <value>name or null</value>
        public string OptionalAsName { get; set; }

        /// <summary>Returns all actions. </summary>
        /// <value>actions</value>
        public IList<OnMergeMatchItem> MatchItems { get; set; }

        /// <summary>Creates an on-Update clause. </summary>
        /// <param name="windowName">is the named window name</param>
        /// <param name="optionalAsName">is the optional as-provided name</param>
        /// <param name="matchItems">is the matched and non-matched action items</param>
        /// <returns>on-Update clause without assignments</returns>
        public static OnMergeClause Create(String windowName,
                                           String optionalAsName,
                                           IList<OnMergeMatchItem> matchItems)
        {
            return new OnMergeClause(windowName, optionalAsName, matchItems);
        }

        /// <summary>Renders the clause in textual representation. </summary>
        /// <param name="writer">to output to</param>
        /// <param name="optionalWhereClause">where clause if present, or null</param>
        /// <param name="formatter">for NewLine-whitespace formatting</param>
        public void ToEPL(TextWriter writer,
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

            if (optionalWhereClause != null)
            {
                formatter.BeginMergeWhere(writer);
                writer.Write("where ");
                optionalWhereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            foreach (OnMergeMatchItem item in MatchItems)
            {
                item.ToEPL(writer, formatter);
            }
        }

        /// <summary>Add a new action to the list of actions. </summary>
        /// <param name="action">to add</param>
        /// <returns>clause</returns>
        public OnMergeClause AddAction(OnMergeMatchItem action)
        {
            MatchItems.Add(action);
            return this;
        }
    }
}