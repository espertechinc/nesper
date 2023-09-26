///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.epl.streamtype;

namespace com.espertech.esper.common.client.hook.datetimemethod
{
    /// <summary>
    ///     Context for use with the date-time method extension API
    /// </summary>
    public class DateTimeMethodValidateContext
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="footprintFound">actual footprint chosen</param>
        /// <param name="streamTypeService">event type information</param>
        /// <param name="currentMethod">information on the current method</param>
        /// <param name="currentParameters">parameters</param>
        /// <param name="statementRawInfo">EPL statement information</param>
        public DateTimeMethodValidateContext(
            DotMethodFP footprintFound,
            StreamTypeService streamTypeService,
            DatetimeMethodDesc currentMethod,
            IList<ExprNode> currentParameters,
            StatementRawInfo statementRawInfo)
        {
            FootprintFound = footprintFound;
            StreamTypeService = streamTypeService;
            CurrentMethod = currentMethod;
            CurrentParameters = currentParameters;
            StatementRawInfo = statementRawInfo;
        }

        /// <summary>
        ///     Returns the actual footprint chosen.
        /// </summary>
        /// <value>footprint</value>
        public DotMethodFP FootprintFound { get; }

        /// <summary>
        ///     Returns event type information.
        /// </summary>
        /// <value>type info</value>
        public StreamTypeService StreamTypeService { get; }

        /// <summary>
        ///     Returns the date-time method information
        /// </summary>
        /// <value>current method</value>
        public DatetimeMethodDesc CurrentMethod { get; }

        /// <summary>
        ///     Returns the parameters to the date-time method.
        /// </summary>
        /// <value>parameter expressions</value>
        public IList<ExprNode> CurrentParameters { get; }

        /// <summary>
        ///     Returns EPL statement information.
        /// </summary>
        /// <value>statement info</value>
        public StatementRawInfo StatementRawInfo { get; }
    }
} // end of namespace