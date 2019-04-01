///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>A clause to delete from a named window based on a triggering event arriving and correlated to the named window events to be deleted. </summary>
    [Serializable]
    public abstract class OnClause 
    {
        /// <summary>Ctor. </summary>
        public OnClause() {
        }
    
        /// <summary>Creates an on-delete clause for deleting from a named window. </summary>
        /// <param name="windowName">is the named window name</param>
        /// <param name="asName">is the as-provided name of the named window</param>
        /// <returns>on-delete clause</returns>
        public static OnDeleteClause CreateOnDelete(String windowName, String asName)
        {
            return OnDeleteClause.Create(windowName, asName);
        }
    
        /// <summary>Creates a split-stream clause. </summary>
        /// <returns>split-stream on-insert clause</returns>
        public static OnInsertSplitStreamClause CreateOnInsertSplitStream()
        {
            return OnInsertSplitStreamClause.Create();
        }
    
        /// <summary>Creates an on-select clause for selecting from a named window. </summary>
        /// <param name="windowName">is the named window name</param>
        /// <param name="asName">is the as-provided name of the named window</param>
        /// <returns>on-select clause</returns>
        public static OnSelectClause CreateOnSelect(String windowName, String asName)
        {
            return OnSelectClause.Create(windowName, asName);
        }
    
        /// <summary>Creates an on-Update clause for updating a named window. </summary>
        /// <param name="windowName">is the named window name</param>
        /// <param name="asName">is the as-provided name of the named window</param>
        /// <param name="expression">expression</param>
        /// <returns>on-Update clause</returns>
        public static OnUpdateClause CreateOnUpdate(String windowName, String asName, Expression expression)
        {
            return OnUpdateClause.Create(windowName, asName).AddAssignment(expression);
        }
    
        /// <summary>Creates an on-set clause for setting variable values. </summary>
        /// <param name="expression">is the assignment expression</param>
        /// <returns>on-set clause</returns>
        public static OnSetClause CreateOnSet(Expression expression)
        {
            return OnSetClause.Create(expression);
        }
    
    }
}
