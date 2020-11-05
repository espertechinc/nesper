///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public class StreamJoinAnalysisResultRuntime
    {
        private bool[] _unidirectional;

        public bool IsPureSelfJoin { get; set; }

        public bool IsUnidirectionalAll {
            get {
                foreach (var ind in _unidirectional) {
                    if (!ind) {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool IsUnidirectional {
            get {
                foreach (var ind in _unidirectional) {
                    if (ind) {
                        return true;
                    }
                }

                return false;
            }
        }

        public int UnidirectionalStreamNumberFirst {
            get {
                for (var i = 0; i < _unidirectional.Length; i++) {
                    if (_unidirectional[i]) {
                        return i;
                    }
                }

                throw new IllegalStateException();
            }
        }

        public bool[] UnidirectionalNonDriving { get; set; }

        public NamedWindow[] NamedWindows { get; set; }

        public bool[] Unidirectional {
            get => _unidirectional;
            set => _unidirectional = value;
        }

        public Table[] Tables { get; set; }
    }
} // end of namespace