///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.view
{
    /// <summary>Helper producing a repository of built-in views.</summary>
    public class ViewEnumHelper
    {
        private static readonly PluggableObjectCollection BUILTIN_VIEWS;

        static ViewEnumHelper()
        {
            BUILTIN_VIEWS = new PluggableObjectCollection();
            foreach (ViewEnum viewEnum in EnumHelper.GetValues<ViewEnum>())
            {
                BUILTIN_VIEWS.AddObject(
                    viewEnum.GetNamespace(), viewEnum.GetName(), viewEnum.GetFactoryType(), PluggableObjectType.VIEW);
            }
        }

        /// <summary>
        /// Returns a collection of plug-in views.
        /// </summary>
        /// <value>built-in view definitions</value>
        public static PluggableObjectCollection BuiltinViews
        {
            get { return BUILTIN_VIEWS; }
        }
    }
} // end of namespace
