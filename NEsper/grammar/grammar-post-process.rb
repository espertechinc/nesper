#!/usr/bin/ruby

ARGV.each do |file|
	modified = false
	output = []
	input = []
	
	File.open(file, 'r') do |f|
		input = f.readlines
	end
	
	input.each do |line|
		if line.lstrip.start_with?("return GetRuleContexts") then
			output.push("#if NET45")
			output.push(line.sub(/\(\)/, '().ToArray()'))
			output.push("#else")
			output.push(line)
			output.push("#endif")
			modified = true
		elsif line.include?("GetTokens(") then
			output.push("#if NET45")
			output.push(line.sub(/\);/, ').ToArray();'))
			output.push("#else")
			output.push(line)
			output.push("#endif")
			modified = true
		else
			output.push(line)
		end
	end
	
	if modified then
		File.open(file, 'w') do |f|
			output.each do |line|
				f.puts(line)
			end
		end
	end
end
