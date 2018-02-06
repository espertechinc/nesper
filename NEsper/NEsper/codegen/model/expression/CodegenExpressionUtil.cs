///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

public class CodegenExpressionUtil
{
    public static void RenderConstant(TextWriter textWriter, object constant)
    {
        if (constant is string)
        {
            textWriter.Write('"');
            string seq = (string)constant;
            if (seq.IndexOf('\"') == -1)
            {
                textWriter.Write(constant);
            }
            else
            {
                AppendSequenceEscapeDQ(textWriter, seq);
            }
            textWriter.Write('"');
        }
        else if (constant is char[])
        {
            AppendSequenceEscapeDQ(textWriter, (char[])constant);
        }
        else if (constant == null)
        {
            textWriter.Write("null");
        }
        else if (constant is int[])
        {
            textWriter.Write("new int[] {");
            int[] nums = (int[])constant;
            string delimiter = "";
            foreach (int num in nums)
            {
                textWriter.Write(delimiter);
                textWriter.Write(num);
                delimiter = ",";
            }
            textWriter.Write("}");
        }
        else
        {
            textWriter.Write(constant);
        }
    }

    private static void AppendSequenceEscapeDQ(TextWriter textWriter, char[] seq)
    {
        for (int i = 0; i < seq.Length; i++)
        {
            char c = seq[i];
            if (c == '\"')
            {
                textWriter.Write('\\');
                textWriter.Write(c);
            }
            else
            {
                textWriter.Write(c);
            }
        }
    }

    private static void AppendSequenceEscapeDQ(TextWriter textWriter, string seq)
    {
        for (int i = 0; i < seq.Length; i++)
        {
            char c = seq[i];
            if (c == '\"')
            {
                textWriter.Write('\\');
                textWriter.Write(c);
            }
            else
            {
                textWriter.Write(c);
            }
        }
    }
}