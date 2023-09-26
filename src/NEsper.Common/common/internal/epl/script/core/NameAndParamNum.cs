///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class NameAndParamNum
    {
        private static readonly NameAndParamNum[] EMPTY_ARRAY = Array.Empty<NameAndParamNum>();

        public NameAndParamNum(
            string name,
            int paramNum)
        {
            Name = name;
            ParamNum = paramNum;
        }

        public string Name { get; }

        public int ParamNum { get; }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (NameAndParamNum)o;

            if (ParamNum != that.ParamNum) {
                return false;
            }

            return Name.Equals(that.Name);
        }

        public override int GetHashCode()
        {
            var result = Name.GetHashCode();
            result = 31 * result + ParamNum;
            return result;
        }

        public static NameAndParamNum[] ToArray(IList<NameAndParamNum> pathScripts)
        {
            if (pathScripts.IsEmpty()) {
                return EMPTY_ARRAY;
            }

            return pathScripts.ToArray();
        }

        public override string ToString()
        {
            return Name + " (" + ParamNum + " parameters)";
        }
    }
} // end of namespace