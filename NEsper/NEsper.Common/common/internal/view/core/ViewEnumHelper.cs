///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1.spec;

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    ///     Helper producing a repository of built-in views.
    /// </summary>
    public class ViewEnumHelper
    {
        static ViewEnumHelper()
        {
            BuiltinViews = new PluggableObjectCollection();
            foreach (var viewEnum in ViewEnum.Values) {
                BuiltinViews.AddObject(
                    viewEnum.Namespace, viewEnum.Name, viewEnum.FactoryClass, PluggableObjectType.VIEW);
            }
        }

        /// <summary>
        ///     Returns a collection of plug-in views.
        /// </summary>
        /// <value>built-in view definitions</value>
        public static PluggableObjectCollection BuiltinViews { get; }
    }
} // end of namespace