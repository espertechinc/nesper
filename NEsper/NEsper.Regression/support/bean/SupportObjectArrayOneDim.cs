///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportObjectArrayOneDim
    {
        private string _id;
        private object[] _arr;

        public object[] Arr {
            get => _arr;
            set => _arr = value;
        }
        public SupportObjectArrayOneDim()
        {
        }

        public SupportObjectArrayOneDim(
            string id,
            object[] arr)
        {
            _id = id;
            _arr = arr;
        }

        protected bool Equals(SupportObjectArrayOneDim other)
        {
            return _id == other._id && Arrays.DeepEquals(_arr, other._arr);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((SupportObjectArrayOneDim) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((_id != null ? _id.GetHashCode() : 0) * 397) ^ (_arr != null ? _arr.GetHashCode() : 0);
            }
        }
    }
} // end of namespace