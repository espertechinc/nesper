///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    [Serializable]
    public class ExprChainedSpec
    {
        public ExprChainedSpec(
            string name,
            IList<ExprNode> parameters,
            bool property)
        {
            Name = name;
            Parameters = parameters;
            IsProperty = property;
        }

        public string Name { get; set; }

        public IList<ExprNode> Parameters { get; set; }

        public bool IsProperty { get; }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (ExprChainedSpec) o;

            if (Name != null ? !Name.Equals(that.Name) : that.Name != null) {
                return false;
            }

            return ExprNodeUtilityCompare.DeepEquals(Parameters, that.Parameters);
        }

        public override int GetHashCode()
        {
            var result = 0;
            result = 31 * result + (Name != null ? Name.GetHashCode() : 0);
            result = 31 * result + (Parameters != null ? Parameters.GetHashCode() : 0);
            return result;
        }

        public override string ToString()
        {
            return "ExprChainedSpec{" +
                   "name='" +
                   Name +
                   '\'' +
                   ", parameters=" +
                   Parameters +
                   '}';
        }
    }
} // end of namespace