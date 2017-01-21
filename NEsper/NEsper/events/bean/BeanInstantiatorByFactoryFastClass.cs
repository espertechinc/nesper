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
    public class BeanInstantiatorByFactoryFastClass : BeanInstantiator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly FastMethod _method;

        public BeanInstantiatorByFactoryFastClass(FastMethod method)
        {
            _method = method;
        }

        #region BeanInstantiator Members

        public Object Instantiate()
        {
            try
            {
                return _method.Invoke(null, null);
            }
            catch (TargetInvocationException e)
            {
                string message =
                    string.Format(
                        "Unexpected exception encountered invoking factory method '{0}' on class '{1}': {2}",
                        _method.Name,
                        _method.Target.DeclaringType.Name,
                        e.InnerException.Message);
                Log.Error(message, e);
                return null;
            }
        }

        #endregion
    }
}