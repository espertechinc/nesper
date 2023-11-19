///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanObject
    {
        private object _five;
        private object _four;
        private object _one;
        private object _six;
        private object _three;
        private object _two;

        public SupportBeanObject()
        {
        }

        public SupportBeanObject(object one)
        {
            _one = one;
        }

        public object Five {
            get => _five;
            set => _five = value;
        }

        public object Four {
            get => _four;
            set => _four = value;
        }

        public object One {
            get => _one;
            set => _one = value;
        }

        public object Six {
            get => _six;
            set => _six = value;
        }

        public object Three {
            get => _three;
            set => _three = value;
        }

        public object Two {
            get => _two;
            set => _two = value;
        }
    }
} // end of namespace