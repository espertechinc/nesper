///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.events.bean
{
    public class BeanInstantiatorByNewInstanceFastClass : BeanInstantiator
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly FastClass _fastClass;

        public BeanInstantiatorByNewInstanceFastClass(FastClass fastClass)
        {
            _fastClass = fastClass;
        }

        public Object Instantiate()
        {
            try
            {
                return _fastClass.NewInstance();
            }
            catch (TargetInvocationException e)
            {
                var message = string.Format("Unexpected exception encountered invoking newInstance on class '{0}': {1}",
                    _fastClass.TargetType.FullName,
                    e.InnerException.Message);
                Log.Error(message, e);
                return null;
            }
        }
    }
}
