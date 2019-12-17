///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorFactoryImpl : ViewableActivatorFactory
    {
        public static readonly ViewableActivatorFactoryImpl INSTANCE = new ViewableActivatorFactoryImpl();

        private ViewableActivatorFactoryImpl()
        {
        }

        public ViewableActivatorFilter CreateFilter()
        {
            return new ViewableActivatorFilter();
        }

        public ViewableActivatorPattern CreatePattern()
        {
            return new ViewableActivatorPattern();
        }

        public ViewableActivatorNamedWindow CreateNamedWindow()
        {
            return new ViewableActivatorNamedWindow();
        }
    }
} // end of namespace