///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.@internal.util
{
    public class FileUtil
    {
        public static string ReadTextFile(string file)
        {
            return File.ReadAllText(file);
        }

        public static string LinesToText(IList<string> lines)
        {
            StringWriter writer = new StringWriter();
            foreach (string line in lines) {
                writer.Write(line);
                writer.Write(Environment.NewLine);
            }

            return writer.ToString();
        }

        private static void ReadFile(
            TextReader reader,
            IList<string> list)
        {
            string text;
            // repeat until all lines is read
            while ((text = reader.ReadLine()) != null) {
                list.Add(text);
            }
        }
    }
} // end of namespace