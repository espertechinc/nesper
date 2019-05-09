///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Specification for creating a named window.
    /// </summary>
    [Serializable]
    public class CreateWindowDesc
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="windowName">the window name</param>
        /// <param name="viewSpecs">the view definitions</param>
        /// <param name="streamSpecOptions">options such as retain-union etc</param>
        /// <param name="insert">true for insert-INFO</param>
        /// <param name="insertFilter">optional filter expression</param>
        /// <param name="columns">list of columns, if using column syntax</param>
        /// <param name="asEventTypeName">Name of as event type.</param>
        public CreateWindowDesc(
            String windowName,
            IList<ViewSpec> viewSpecs,
            StreamSpecOptions streamSpecOptions,
            bool insert,
            ExprNode insertFilter,
            IList<ColumnDesc> columns,
            String asEventTypeName)
        {
            WindowName = windowName;
            ViewSpecs = viewSpecs;
            IsInsert = insert;
            InsertFilter = insertFilter;
            StreamSpecOptions = streamSpecOptions;
            Columns = columns;
            AsEventTypeName = asEventTypeName;
        }

        /// <summary>Returns the window name. </summary>
        /// <value>window name</value>
        public string WindowName { get; private set; }

        /// <summary>Returns the view specifications. </summary>
        /// <value>view specs</value>
        public IList<ViewSpec> ViewSpecs { get; private set; }

        /// <summary>Returns true for insert-from. </summary>
        /// <value>indicator to insert from another named window</value>
        public bool IsInsert { get; private set; }

        /// <summary>Returns the expression to filter insert-from events, or null if none supplied. </summary>
        /// <value>insert filter expression</value>
        public ExprNode InsertFilter { get; set; }

        /// <summary>Returns the window name to insert from. </summary>
        /// <value>window name to insert from</value>
        public string InsertFromWindow { get; set; }

        /// <summary>Returns the options for the stream such as unidirectional, retain-union etc. </summary>
        /// <value>stream options</value>
        public StreamSpecOptions StreamSpecOptions { get; private set; }

        /// <summary>Returns column names and types. </summary>
        /// <value>column descriptors</value>
        public IList<ColumnDesc> Columns { get; private set; }

        public string AsEventTypeName { get; private set; }
    }
}