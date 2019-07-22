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
    public class AnnotationHintAttribute : HintAttribute
    {
        private readonly AppliesTo applies;
        private readonly string model;
        private readonly string value;

        public AnnotationHintAttribute(
            string value,
            AppliesTo applies,
            string model)
        {
            this.value = value;
            this.applies = applies;
            this.model = model;
        }

        public override string Value => value;

        public override AppliesTo Applies => applies;

        public override string Model => model;

        public Type AnnotationType => typeof(HintAttribute);

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (AnnotationHintAttribute) o;

            if (!value?.Equals(that.value) ?? that.value != null) {
                return false;
            }

            if (applies != that.applies) {
                return false;
            }

            return model != null ? model.Equals(that.model) : that.model == null;
        }

        public override int GetHashCode()
        {
            var result = value != null ? value.GetHashCode() : 0;
            result = 31 * result + applies.GetHashCode();
            result = 31 * result + (model != null ? model.GetHashCode() : 0);
            return result;
        }
    }
} // end of namespace