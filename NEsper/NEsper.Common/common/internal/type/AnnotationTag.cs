///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.annotation;

namespace com.espertech.esper.common.@internal.type
{
    public class AnnotationTag : TagAttribute
    {
        private readonly string name;
        private readonly string value;

        public AnnotationTag(
            string name,
            string value)
        {
            this.name = name;
            this.value = value;
        }

        public override string Name {
            get { return name; }
        }

        public override string Value {
            get { return value; }
        }

        public Type AnnotationType {
            get { return typeof(TagAttribute); }
        }

        public override string ToString()
        {
            return "@Tag(name=\"" + name + "\", value=\"" + value + "\")";
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (AnnotationTag) o;

            if (name != null ? !name.Equals(that.name) : that.name != null) {
                return false;
            }

            return value != null ? value.Equals(that.value) : that.value == null;
        }

        public override int GetHashCode()
        {
            var result = name != null ? name.GetHashCode() : 0;
            result = 31 * result + (value != null ? value.GetHashCode() : 0);
            return result;
        }
    }
} // end of namespace