///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createwindow
{
    /// <summary>
    /// Selected properties for a create-window expression in the model-after syntax.
    /// </summary>
    public class NamedWindowSelectedProps
    {
        private Type selectExpressionType;
        private string assignedName;
        private EventType fragmentType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="selectExpressionType">expression result type</param>
        /// <param name="assignedName">name of column</param>
        /// <param name="fragmentType">null if not a fragment, or event type of fragment if one was selected</param>
        public NamedWindowSelectedProps(
            Type selectExpressionType,
            string assignedName,
            EventType fragmentType)
        {
            this.selectExpressionType = selectExpressionType;
            this.assignedName = assignedName;
            this.fragmentType = fragmentType;
        }

        /// <summary>
        /// Returns the type of the expression result.
        /// </summary>
        /// <returns>type</returns>
        public Type SelectExpressionType {
            get => selectExpressionType;
        }

        /// <summary>
        /// Returns the assigned column name.
        /// </summary>
        /// <returns>name</returns>
        public string AssignedName {
            get => assignedName;
        }

        /// <summary>
        /// Returns the fragment type or null if not a fragment type.
        /// </summary>
        /// <returns>type</returns>
        public EventType FragmentType {
            get => fragmentType;
        }
    }
} // end of namespace