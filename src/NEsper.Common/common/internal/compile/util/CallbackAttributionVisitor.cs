///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.compile.util
{
    public interface CallbackAttributionVisitor<T>
    {
        T Accept(CallbackAttributionSubquery attribution);
        T Accept(CallbackAttributionStreamPattern attribution);
        T Accept(CallbackAttributionContextController attribution);
        T Accept(CallbackAttributionContextCondition attribution);
        T Accept(CallbackAttributionContextConditionPattern attribution);
        T Accept(CallbackAttributionNamedWindow attribution);
        T Accept(CallbackAttributionStream attribution);
        T Accept(CallbackAttributionDataflow attribution);
        T Accept(CallbackAttributionMatchRecognize attribution);
        T Accept(CallbackAttributionOutputRate attribution);
        T Accept(CallbackAttributionStreamGrouped attribution);
        T Accept(CallbackAttributionSubqueryGrouped attribution);
    }
} // end of namespace