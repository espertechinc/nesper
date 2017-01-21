///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Specification for use of an existing table.
    /// </summary>
    [Serializable]
    public class TableQueryStreamSpec
        : StreamSpecBase
        , StreamSpecCompiled
    {
        public TableQueryStreamSpec(string optionalStreamName, ViewSpec[] viewSpecs, StreamSpecOptions streamSpecOptions, string tableName, IList<ExprNode> filterExpressions)
            : base(optionalStreamName, viewSpecs, streamSpecOptions)
        {
            TableName = tableName;
            FilterExpressions = filterExpressions;
        }

        public string TableName { get; private set; }

        public IList<ExprNode> FilterExpressions { get; private set; }
    }
}
