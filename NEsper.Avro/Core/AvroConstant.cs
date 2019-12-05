///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

namespace NEsper.Avro.Core
{
    public class AvroConstant
    {
        public static readonly string PROP_STRING_KEY = "avro.string";
        public static readonly string PROP_STRING_VALUE = "string";
        public static readonly string PROP_ARRAY_VALUE = "array";

        public static readonly JProperty PROP_STRING = TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE);
    }
} // end of namespace