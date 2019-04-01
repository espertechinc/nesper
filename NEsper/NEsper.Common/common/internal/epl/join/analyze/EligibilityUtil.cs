///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.analyze
{
    public class EligibilityUtil
    {
        public static EligibilityDesc VerifyInputStream(ExprNode expression, int indexedStream)
        {
            var visitor = new ExprNodeIdentifierCollectVisitor();
            expression.Accept(visitor);
            var inputStreamsRequired = visitor.StreamsRequired;
            if (inputStreamsRequired.Count > 1) { // multi-stream dependency no optimization (i.e. a+b=c)
                return new EligibilityDesc(Eligibility.INELIGIBLE, null);
            }

            if (inputStreamsRequired.Count == 1 && inputStreamsRequired.First() == indexedStream) {
                // self-compared no optimization
                return new EligibilityDesc(Eligibility.INELIGIBLE, null);
            }

            if (inputStreamsRequired.IsEmpty()) {
                return new EligibilityDesc(Eligibility.REQUIRE_NONE, null);
            }

            return new EligibilityDesc(Eligibility.REQUIRE_ONE, inputStreamsRequired.First());
        }
    }
} // end of namespace