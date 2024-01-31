///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Validated stream specifications generally have expression nodes that are valid and event types exist.
    /// </summary>
    public interface StreamSpecCompiled : StreamSpec
    {
    }

    public class StreamSpecCompiledConstants
    {
        public static readonly StreamSpecCompiled[] EMPTY_STREAM_ARRAY = Array.Empty<StreamSpecCompiled>();
    }
}