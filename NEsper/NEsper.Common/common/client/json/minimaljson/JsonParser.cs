///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

// ATTRIBUTION NOTICE
// ==================
// MinimalJson is a fast and minimal JSON parser and writer for Java. It's not an object mapper, but a bare-bones library that aims at being:
// - fast: high performance comparable with other state-of-the-art parsers (see below)
// - lightweight: object representation with minimal memory footprint (e.g. no HashMaps)
// - simple: reading, writing and modifying JSON with minimal code (short names, fluent style)
// Minimal JSON can be found at https://github.com/ralfstx/minimal-json.
// Minimal JSON is licensed under the MIT License, see https://github.com/ralfstx/minimal-json/blob/master/LICENSE


namespace com.espertech.esper.common.client.json.minimaljson
{
	/// <summary>
	/// A streaming parser for JSON text. The parser reports all events to a given handler.
	/// </summary>
	public class JsonParser {

	    private const int MAX_NESTING_LEVEL = 1000;
	    private const int MIN_BUFFER_SIZE = 10;
	    private const int DEFAULT_BUFFER_SIZE = 1024;

	    private readonly JsonHandler<object, object> handler;
	    private Reader reader;
	    private char[] buffer;
	    private int bufferOffset;
	    private int index;
	    private int fill;
	    private int line;
	    private int lineOffset;
	    private int current;
	    private StringBuilder captureBuffer;
	    private int captureStart;
	    private int nestingLevel;

	    /*
	     * |                      bufferOffset
	     *                        v
	     * [a|b|c|d|e|f|g|h|i|j|k|l|m|n|o|p|q|r|s|t]        < input
	     *                       [l|m|n|o|p|q|r|s|t|?|?]    < buffer
	     *                          ^               ^
	     *                       |  index           fill
	     */

	    /// <summary>
	    /// Creates a new JsonParser with the given handler. The parser will report all parser events to
	    /// this handler.
	    /// </summary>
	    /// <param name="handler">the handler to process parser events</param>

	    public JsonParser(JsonHandler<?, ?> handler) {
	        if (handler == null) {
	            throw new NullPointerException("handler is null");
	        }
	        this.handler = (JsonHandler<object, object>) handler;
	        handler.parser = this;
	    }

	    /// <summary>
	    /// Parses the given input string. The input must contain a valid JSON value, optionally padded
	    /// with whitespace.
	    /// </summary>
	    /// <param name="string">the input string, must be valid JSON</param>
	    /// <throws>ParseException if the input is not valid JSON</throws>
	    public void Parse(string string) {
	        if (string == null) {
	            throw new NullPointerException("string is null");
	        }
	        int bufferSize = Math.Max(MIN_BUFFER_SIZE, Math.Min(DEFAULT_BUFFER_SIZE, string.Length()));
	        try {
	            Parse(new StringReader(string), bufferSize);
	        } catch (IOException exception) {
	            // StringReader does not throw IOException
	            throw new RuntimeException(exception);
	        }
	    }

	    /// <summary>
	    /// Reads the entire input from the given reader and parses it as JSON. The input must contain a
	    /// valid JSON value, optionally padded with whitespace.
	    /// <para />Characters are read in chunks into a default-sized input buffer. Hence, wrapping a reader in an
	    /// additional <code>BufferedReader</code> likely won't improve reading performance.
	    /// </summary>
	    /// <param name="reader">the reader to read the input from</param>
	    /// <throws>IOException    if an I/O error occurs in the reader</throws>
	    /// <throws>ParseException if the input is not valid JSON</throws>
	    public void Parse(Reader reader) {
	        Parse(reader, DEFAULT_BUFFER_SIZE);
	    }

	    /// <summary>
	    /// Reads the entire input from the given reader and parses it as JSON. The input must contain a
	    /// valid JSON value, optionally padded with whitespace.
	    /// <para />Characters are read in chunks into an input buffer of the given size. Hence, wrapping a reader
	    /// in an additional <code>BufferedReader</code> likely won't improve reading performance.
	    /// </summary>
	    /// <param name="reader">the reader to read the input from</param>
	    /// <param name="buffersize">the size of the input buffer in chars</param>
	    /// <throws>IOException    if an I/O error occurs in the reader</throws>
	    /// <throws>ParseException if the input is not valid JSON</throws>
	    public void Parse(Reader reader, int buffersize) {
	        if (reader == null) {
	            throw new NullPointerException("reader is null");
	        }
	        if (buffersize <= 0) {
	            throw new ArgumentException("buffersize is zero or negative");
	        }
	        this.reader = reader;
	        buffer = new char[buffersize];
	        bufferOffset = 0;
	        index = 0;
	        fill = 0;
	        line = 1;
	        lineOffset = 0;
	        current = 0;
	        captureStart = -1;
	        Read();
	        SkipWhiteSpace();
	        ReadValue();
	        SkipWhiteSpace();
	        if (!IsEndOfText) {
	            throw Error("Unexpected character");
	        }
	    }

	    private void ReadValue() {
	        switch (current) {
	            case 'n':
	                ReadNull();
	                break;
	            case 't':
	                ReadTrue();
	                break;
	            case 'f':
	                ReadFalse();
	                break;
	            case '"':
	                ReadString();
	                break;
	            case '[':
	                ReadArray();
	                break;
	            case '{':
	                ReadObject();
	                break;
	            case '-':
	            case '0':
	            case '1':
	            case '2':
	            case '3':
	            case '4':
	            case '5':
	            case '6':
	            case '7':
	            case '8':
	            case '9':
	                ReadNumber();
	                break;
	            default:
	                throw Expected("value");
	        }
	    }

	    private void ReadArray() {
	        object array = handler.StartArray();
	        Read();
	        if (++nestingLevel > MAX_NESTING_LEVEL) {
	            throw Error("Nesting too deep");
	        }
	        SkipWhiteSpace();
	        if (ReadChar(']')) {
	            nestingLevel--;
	            handler.EndArray(array);
	            return;
	        }
	        do {
	            SkipWhiteSpace();
	            handler.StartArrayValue(array);
	            ReadValue();
	            handler.EndArrayValue(array);
	            SkipWhiteSpace();
	        } while (ReadChar(','));
	        if (!ReadChar(']')) {
	            throw Expected("',' or ']'");
	        }
	        nestingLevel--;
	        handler.EndArray(array);
	    }

	    private void ReadObject() {
	        object object = handler.StartObject();
	        Read();
	        if (++nestingLevel > MAX_NESTING_LEVEL) {
	            throw Error("Nesting too deep");
	        }
	        SkipWhiteSpace();
	        if (ReadChar('}')) {
	            nestingLevel--;
	            handler.EndObject(object);
	            return;
	        }
	        do {
	            SkipWhiteSpace();
	            handler.StartObjectName(object);
	            string name = ReadName();
	            handler.EndObjectName(object, name);
	            SkipWhiteSpace();
	            if (!ReadChar(':')) {
	                throw Expected("':'");
	            }
	            SkipWhiteSpace();
	            handler.StartObjectValue(object, name);
	            ReadValue();
	            handler.EndObjectValue(object, name);
	            SkipWhiteSpace();
	        } while (ReadChar(','));
	        if (!ReadChar('}')) {
	            throw Expected("',' or '}'");
	        }
	        nestingLevel--;
	        handler.EndObject(object);
	    }

	    private string ReadName() {
	        if (current != '"') {
	            throw Expected("name");
	        }
	        return ReadStringInternal();
	    }

	    private void ReadNull() {
	        handler.StartNull();
	        Read();
	        ReadRequiredChar('u');
	        ReadRequiredChar('l');
	        ReadRequiredChar('l');
	        handler.EndNull();
	    }

	    private void ReadTrue() {
	        handler.StartBoolean();
	        Read();
	        ReadRequiredChar('r');
	        ReadRequiredChar('u');
	        ReadRequiredChar('e');
	        handler.EndBoolean(true);
	    }

	    private void ReadFalse() {
	        handler.StartBoolean();
	        Read();
	        ReadRequiredChar('a');
	        ReadRequiredChar('l');
	        ReadRequiredChar('s');
	        ReadRequiredChar('e');
	        handler.EndBoolean(false);
	    }

	    private void ReadRequiredChar(char ch) {
	        if (!ReadChar(ch)) {
	            throw Expected("'" + ch + "'");
	        }
	    }

	    private void ReadString() {
	        handler.StartString();
	        handler.EndString(ReadStringInternal());
	    }

	    private string ReadStringInternal() {
	        Read();
	        StartCapture();
	        while (current != '"') {
	            if (current == '\\') {
	                PauseCapture();
	                ReadEscape();
	                StartCapture();
	            } else if (current < 0x20) {
	                throw Expected("valid string character");
	            } else {
	                Read();
	            }
	        }
	        string string = EndCapture();
	        Read();
	        return string;
	    }

	    private void ReadEscape() {
	        Read();
	        switch (current) {
	            case '"':
	            case '/':
	            case '\\':
	                captureBuffer.Append((char) current);
	                break;
	            case 'b':
	                captureBuffer.Append('\b');
	                break;
	            case 'f':
	                captureBuffer.Append('\f');
	                break;
	            case 'n':
	                captureBuffer.Append('\n');
	                break;
	            case 'r':
	                captureBuffer.Append('\r');
	                break;
	            case 't':
	                captureBuffer.Append('\t');
	                break;
	            case 'u':
	                char[] hexChars = new char[4];
	                for (int i = 0; i < 4; i++) {
	                    Read();
	                    if (!IsHexDigit) {
	                        throw Expected("hexadecimal digit");
	                    }
	                    hexChars[i] = (char) current;
	                }
	                captureBuffer.Append((char) int?.ParseInt(new string(hexChars), 16));
	                break;
	            default:
	                throw Expected("valid escape sequence");
	        }
	        Read();
	    }

	    private void ReadNumber() {
	        handler.StartNumber();
	        StartCapture();
	        ReadChar('-');
	        int firstDigit = current;
	        if (!ReadDigit()) {
	            throw Expected("digit");
	        }
	        if (firstDigit != '0') {
	            while (ReadDigit()) {
	            }
	        }
	        ReadFraction();
	        ReadExponent();
	        handler.EndNumber(EndCapture());
	    }

	    private bool ReadFraction() {
	        if (!ReadChar('.')) {
	            return false;
	        }
	        if (!ReadDigit()) {
	            throw Expected("digit");
	        }
	        while (ReadDigit()) {
	        }
	        return true;
	    }

	    private bool ReadExponent() {
	        if (!ReadChar('e') && !ReadChar('E')) {
	            return false;
	        }
	        if (!ReadChar('+')) {
	            ReadChar('-');
	        }
	        if (!ReadDigit()) {
	            throw Expected("digit");
	        }
	        while (ReadDigit()) {
	        }
	        return true;
	    }

	    private bool ReadChar(char ch) {
	        if (current != ch) {
	            return false;
	        }
	        Read();
	        return true;
	    }

	    private bool ReadDigit() {
	        if (!IsDigit) {
	            return false;
	        }
	        Read();
	        return true;
	    }

	    private void SkipWhiteSpace() {
	        while (IsWhiteSpace) {
	            Read();
	        }
	    }

	    private void Read() {
	        if (index == fill) {
	            if (captureStart != -1) {
	                captureBuffer.Append(buffer, captureStart, fill - captureStart);
	                captureStart = 0;
	            }
	            bufferOffset += fill;
	            fill = reader.Read(buffer, 0, buffer.Length);
	            index = 0;
	            if (fill == -1) {
	                current = -1;
	                index++;
	                return;
	            }
	        }
	        if (current == '\n') {
	            line++;
	            lineOffset = bufferOffset + index;
	        }
	        current = buffer[index++];
	    }

	    private void StartCapture() {
	        if (captureBuffer == null) {
	            captureBuffer = new StringBuilder();
	        }
	        captureStart = index - 1;
	    }

	    private void PauseCapture() {
	        int end = current == -1 ? index : index - 1;
	        captureBuffer.Append(buffer, captureStart, end - captureStart);
	        captureStart = -1;
	    }

	    private string EndCapture() {
	        int start = captureStart;
	        int end = index - 1;
	        captureStart = -1;
	        if (captureBuffer.Length() > 0) {
	            captureBuffer.Append(buffer, start, end - start);
	            string captured = captureBuffer.ToString();
	            captureBuffer.Length = 0;
	            return captured;
	        }
	        return new string(buffer, start, end - start);
	    }

	    Location GetLocation() {
	        int offset = bufferOffset + index - 1;
	        int column = offset - lineOffset + 1;
	        return new Location(offset, line, column);
	    }

	    private ParseException Expected(string expected) {
	        if (IsEndOfText) {
	            return Error("Unexpected end of input");
	        }
	        return Error("Expected " + expected);
	    }

	    private ParseException Error(string message) {
	        return new ParseException(message, Location);
	    }

	    private bool IsWhiteSpace() {
	        return current == ' ' || current == '\t' || current == '\n' || current == '\r';
	    }

	    private bool IsDigit() {
	        return current >= '0' && current <= '9';
	    }

	    private bool IsHexDigit() {
	        return current >= '0' && current <= '9'
	            || current >= 'a' && current <= 'f'
	            || current >= 'A' && current <= 'F';
	    }

	    private bool IsEndOfText() {
	        return current == -1;
	    }

	}
} // end of namespace
