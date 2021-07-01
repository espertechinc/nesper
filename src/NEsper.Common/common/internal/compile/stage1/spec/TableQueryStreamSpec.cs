///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Specification for use of an existing table.
    /// </summary>
    public class TableQueryStreamSpec : StreamSpecBase,
        StreamSpecCompiled
    {
        private readonly TableMetaData table;
        private IList<ExprNode> filterExpressions;

        public TableQueryStreamSpec(
            string optionalStreamName,
            ViewSpec[] viewSpecs,
            StreamSpecOptions streamSpecOptions,
            TableMetaData table,
            IList<ExprNode> filterExpressions)
            : base(optionalStreamName, viewSpecs, streamSpecOptions)
        {
            this.table = table;
            this.filterExpressions = filterExpressions;
        }

        public TableMetaData Table {
            get => table;
        }

        public IList<ExprNode> FilterExpressions {
            get => filterExpressions;
        }
    }
} // end of namespace