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

using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.rettype;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapIterableScalar : ExprDotStaticMethodWrap
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _methodName;
        private readonly Type _componentType;

        public ExprDotStaticMethodWrapIterableScalar(String methodName, Type componentType)
        {
            _methodName = methodName;
            _componentType = componentType;
        }

        public EPType TypeInfo
        {
            get { return EPTypeHelper.CollectionOfSingleValue(_componentType); }
        }

        public ICollection<object> Convert(Object result)
        {
            if (result == null)
            {
                return null;
            }
            if (!(result is IEnumerable))
            {
                Log.Warn("Expected iterable-type input from method '" + _methodName + "' but received " + result.GetType());
                return null;
            }

            var asEnumerable = (IEnumerable) result;
            return asEnumerable.Cast<object>().ToList();
        }
    }
}