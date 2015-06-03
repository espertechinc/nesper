#!/usr/bin/env python

import sys
import os, os.path
import re

files=(
	'./NEsper/NEsper/epl/join/base/ExecNodeQueryStrategy.cs',
	'./NEsper/NEsper/Properties/AssemblyInfo.cs',
	'./NEsper/NEsper/util/Version.cs',
	'./NEsper/NEsper.Scripting.Jurassic/Properties/AssemblyInfo.cs',
	'./NEsper/NEsper.Scripting.Noesis/Properties/AssemblyInfo.cs',
	'./NEsper/NEsper.Tests/Properties/AssemblyInfo.cs',
	'./NEsper.IO/NEsper.IO/Properties/AssemblyInfo.cs',
	'./NEsper.IO/NEsper.IO.Tests/Properties/AssemblyInfo.cs',
	'./NEsper.nuspec',
	'./NEsper.proj',
	'./NEsper.shfbproj'
)

if (len(sys.argv) < 2):
	print('Please supply a version number')
	sys.exit(-1)

# read the current version number from VERSION
with open('VERSION', 'r') as fh:
	version = fh.readline().strip()

new_version=sys.argv[1]

print(version)
for file in files:
	changed = False
	output = []

	with open(file, 'r') as fh:
		for line in fh:
			line = line.strip()
			if version in line:
				line = line.replace(version, new_version)
				changed = True
			output.append(line + '\n')
	
	# rewrite the file if output has changed
	if changed:
		print('rewrite ' + file)
		with open(file, 'w') as fh:
			fh.writelines(output)
			fh.close()

# write the new version to VERSION
with open('VERSION', 'w') as fh:
	fh.write(new_version)