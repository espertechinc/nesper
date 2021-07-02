#!/usr/bin/ruby

ARGV.each do |file|
	File.open(file, 'r') do |f|
		f.readlines.each do |line|
			if line.lstrip.start_with?("return GetRuleContexts") then
				puts("#if NET45")
				puts(line.sub(/\(\)/, '().ToArray()'))
				puts("#else")
				puts(line)
				puts("#endif")
			elsif line.include?("GetTokens(") then
				puts("#if NET45")
				puts(line.sub(/\);/, ').ToArray();'))
				puts("#else")
				puts(line)
				puts("#endif")
			else
				puts(line)
			end
		end
	end
end
