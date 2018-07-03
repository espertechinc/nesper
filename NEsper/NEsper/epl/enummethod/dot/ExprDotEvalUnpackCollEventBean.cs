///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class ExprDotEvalUnpackCollEventBean : ExprDotEval
    {
        private readonly EPType _typeInfo;
    
        public ExprDotEvalUnpackCollEventBean(EventType type) {
            _typeInfo = EPTypeHelper.CollectionOfSingleValue(type.UnderlyingType);
        }

        public object Evaluate(object target, EvaluateParams evalParams)
        {
            if (target == null)
            {
                return null;
            }
            if (target is ICollection<EventBean>)
                return ((ICollection<EventBean>)target).Select(e => e.Underlying).ToList();
            else if (target is ICollection<Object>)
                return ((ICollection<Object>)target).OfType<EventBean>().Select(e => e.Underlying).ToList();
            else if (target is ICollection)
                return ((ICollection)target).Cast<EventBean>().Select(e => e.Underlying).ToList();

            throw new ArgumentException("invalid value for target");
        }

        public EPType TypeInfo
        {
            get { return _typeInfo; }
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitUnderlyingEventColl();
        }
    }
}
