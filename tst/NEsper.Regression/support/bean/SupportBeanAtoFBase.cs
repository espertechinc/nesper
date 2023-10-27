///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportBeanAtoFBase : SupportMarkerInterface
    {
        private string _id;

        public SupportBeanAtoFBase()
        {
        }

        public SupportBeanAtoFBase(string id)
        {
            _id = id;
        }

        public string Id {
            get => _id;
            set => _id = value;
        }

        public override string ToString()
        {
            return "Id=" + _id;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (SupportBeanAtoFBase) o;

            if (_id != null ? !_id.Equals(that._id) : that._id != null) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return _id != null ? _id.GetHashCode() : 0;
        }
    }
} // end of namespace