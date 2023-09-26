///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.bean.instantiator
{
    public class BeanInstantiatorByCtor : BeanInstantiator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConstructorInfo _ctor;

        public BeanInstantiatorByCtor(ConstructorInfo ctor)
        {
            _ctor = ctor;
        }

        #region BeanInstantiator Members

        public object Instantiate()
        {
            try {
                return _ctor.Invoke(Array.Empty<object>());
            }
            catch (TargetInvocationException e) {
                var message = "Unexpected exception encountered invoking constructor '" +
                              _ctor.Name +
                              "' on class '" +
                              _ctor.DeclaringType.FullName +
                              "': " +
                              e.InnerException.Message;
                Log.Error(message, e);
                return null;
            }
            catch (MemberAccessException ex) {
                return Handle(ex);
            }
        }

        #endregion

        private object Handle(Exception e)
        {
            var message = "Unexpected exception encountered invoking newInstance on class '" +
                          _ctor.DeclaringType.FullName +
                          "': " +
                          e.Message;
            Log.Error(message, e);
            return null;
        }
    }
}