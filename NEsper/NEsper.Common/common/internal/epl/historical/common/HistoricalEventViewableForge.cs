///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.historical.common
{
    public interface HistoricalEventViewableForge : ScheduleHandleCallbackProvider
    {
        /// <summary>
        ///     Returns the a set of stream numbers of all streams that provide property values
        ///     in any of the parameter expressions to the stream.
        /// </summary>
        /// <value>set of stream numbers</value>
        SortedSet<int> RequiredStreams { get; }

        void Validate(StreamTypeService typeService, StatementBaseInfo @base, StatementCompileTimeServices services);

        CodegenExpression Make(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope);
    }
} // end of namespace