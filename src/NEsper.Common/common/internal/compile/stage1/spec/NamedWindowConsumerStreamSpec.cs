///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.contained;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Specification for use of an existing named window.
    /// </summary>
    public class NamedWindowConsumerStreamSpec : StreamSpecBase,
        StreamSpecCompiled
    {
        private int namedWindowConsumerId = -1;

        public NamedWindowConsumerStreamSpec(
            NamedWindowMetaData namedWindow,
            string optionalAsName,
            ViewSpec[] viewSpecs,
            IList<ExprNode> filterExpressions,
            StreamSpecOptions streamSpecOptions,
            PropertyEvaluatorForge optPropertyEvaluator)
            : base(optionalAsName, viewSpecs, streamSpecOptions)
        {
            NamedWindow = namedWindow;
            FilterExpressions = filterExpressions;
            OptPropertyEvaluator = optPropertyEvaluator;
        }

        /// <summary>
        ///     Returns list of filter expressions onto the named window, or no filter expressions if none defined.
        /// </summary>
        /// <returns>list of filter expressions</returns>
        public IList<ExprNode> FilterExpressions { get; }

        public PropertyEvaluatorForge OptPropertyEvaluator { get; }

        public int NamedWindowConsumerId {
            get => namedWindowConsumerId;
            set => namedWindowConsumerId = value;
        }

        public NamedWindowMetaData NamedWindow { get; }
    }
} // end of namespace