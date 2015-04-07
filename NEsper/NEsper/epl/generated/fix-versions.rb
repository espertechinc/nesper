#!/usr/bin/env ruby

File.open('/cygdrive/d/src/Espertech/NEsper-tip/NEsper/NEsper/epl/generated/template.cs') do |f|
	inContextMethod = false
	lines = f.readlines
	lines.each do |line|
		line = line.rstrip!
		if (/GetRuleContexts\<[A-Za-z_]+\>\(\)/ =~ line) then
			puts("#if NET45")
			puts(line[0...-1] + ".ToArray();")
			puts("#else")
			puts(line)
			puts("#endif")
			inContextMethod = false
		elsif (/return GetTokens\(/ =~ line) then
			puts("#if NET45")
			puts(line[0...-3] + ".ToArray(); }")
			puts("#else")
			puts(line)
			puts("#endif")
			inContextMethod = false
		else
			puts(line)
			match = /public [A-Za-z]+Context\[\] [A-Za-z_]+\(\)/ =~ line
			inContextMethod = match != nil
		end
	end
end