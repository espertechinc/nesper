///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.access
{
    /// <summary>
    ///     Coordinates between view factories and requested resource (by expressions) the
    ///     availability of view resources to expressions.
    /// </summary>
    public class ViewResourceDelegateDesc
    {
        public static readonly ViewResourceDelegateDesc[] SINGLE_ELEMENT_ARRAY = new ViewResourceDelegateDesc[] {
            new ViewResourceDelegateDesc(false, new SortedSet<int>()) // Collections.emptySortedSet()
        };
        
        public ViewResourceDelegateDesc(
            bool hasPrevious,
            ISet<int> priorRequests)
        {
            HasPrevious = hasPrevious;
            PriorRequests = priorRequests;
        }

        public bool HasPrevious { get; }

        public ISet<int> PriorRequests { get; }

        public CodegenExpression ToExpression()
        {
            return NewInstance(GetType(), Constant(HasPrevious), CodegenLegoRichConstant.ToExpression(PriorRequests));
        }

        public static bool HasPrior(ViewResourceDelegateDesc[] delegates)
        {
            foreach (var @delegate in delegates) {
                if (!@delegate.PriorRequests.IsEmpty()) {
                    return true;
                }
            }

            return false;
        }
        
        public static CodegenExpression ToExpression(ViewResourceDelegateDesc[] descs)
        {
            if (descs.Length == 1 && !descs[0].HasPrevious && descs[0].PriorRequests.IsEmpty()) {
                return PublicConstValue(typeof(ViewResourceDelegateDesc), "SINGLE_ELEMENT_ARRAY");
            }

            var values = descs
                .Select(_ => _.ToExpression())
                .ToArray();
            return NewArrayWithInit(typeof(ViewResourceDelegateDesc), values);
        }

    }
} // end of namespace