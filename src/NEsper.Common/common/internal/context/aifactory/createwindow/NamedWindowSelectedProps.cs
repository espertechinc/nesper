///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.context.aifactory.createwindow
{
    /// <summary>
    ///     Selected properties for a create-window expression in the model-after syntax.
    /// </summary>
    public class NamedWindowSelectedProps
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="selectExpressionType">expression result type</param>
        /// <param name="assignedName">name of column</param>
        /// <param name="fragmentType">null if not a fragment, or event type of fragment if one was selected</param>
        public NamedWindowSelectedProps(
            Type selectExpressionType,
            string assignedName,
            EventType fragmentType)
        {
            SelectExpressionType = selectExpressionType;
            AssignedName = assignedName;
            FragmentType = fragmentType;
        }

        /// <summary>
        ///     Returns the type of the expression result.
        /// </summary>
        /// <value>type</value>
        public Type SelectExpressionType { get; }

        /// <summary>
        ///     Returns the assigned column name.
        /// </summary>
        /// <value>name</value>
        public string AssignedName { get; }

        /// <summary>
        ///     Returns the fragment type or null if not a fragment type.
        /// </summary>
        /// <value>type</value>
        public EventType FragmentType { get; }
    }
} // end of namespace