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
    /// <summary>As part of on-merge, this represents a single "matched" or "not matched" entry. </summary>
    [Serializable]
    public class OnMergeMatchItem : OnClause
    {
        /// <summary>Ctor. </summary>
        public OnMergeMatchItem() {
            Actions = new List<OnMergeMatchedAction>();
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="matched">true for matched, false for not-matched</param>
        /// <param name="optionalCondition">an optional additional filter</param>
        /// <param name="actions">one or more actions to take</param>
        public OnMergeMatchItem(bool matched, Expression optionalCondition, IList<OnMergeMatchedAction> actions)
        {
            IsMatched = matched;
            OptionalCondition = optionalCondition;
            Actions = actions;
        }
    
        /// <summary>Renders the clause in textual representation. </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">for NewLine-whitespace formatting</param>
        public void ToEPL(TextWriter writer, EPStatementFormatter formatter)
        {
            formatter.BeginMergeWhenMatched(writer);
            if (IsMatched) {
                writer.Write("when matched");
            }
            else {
                writer.Write("when not matched");
            }
            if (OptionalCondition != null)
            {
                writer.Write(" and ");
                OptionalCondition.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
            foreach (OnMergeMatchedAction action in Actions) {
                formatter.BeginMergeAction(writer);
                action.ToEPL(writer);
            }
        }

        /// <summary>Returns true for matched, and false for not-matched. </summary>
        /// <value>matched or not-matched indicator</value>
        public bool IsMatched { get; set; }

        /// <summary>Returns the condition to apply or null if none is provided. </summary>
        /// <value>condition</value>
        public Expression OptionalCondition { get; set; }

        /// <summary>Returns all actions. </summary>
        /// <value>actions</value>
        public IList<OnMergeMatchedAction> Actions { get; set; }
    }
}
