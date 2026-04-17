
import re
import os
import sys

# Default to build_warnings_v4.log if not specified
log_file = r"..\build_warnings_v4.log"
if len(sys.argv) > 1:
    log_file = sys.argv[1]

if not os.path.exists(log_file):
    print(f"File not found: {log_file}")
    # Try looking in current dir
    log_file = os.path.basename(log_file)
    if not os.path.exists(log_file):
        print(f"File not found in current dir either: {log_file}")
        exit(1)

print(f"Analyzing: {log_file}")
print(f"File size: {os.path.getsize(log_file)} bytes")

# Pattern to capture warning code (e.g. CS0649, SYSLIB0050)
pattern = re.compile(r"warning\s+([A-Z0-9]+):")

warning_codes = []

try:
    with open(log_file, "r", encoding="utf-16") as f:
        for line in f:
            match = pattern.search(line)
            if match:
                warning_codes.append(match.group(1))
except Exception as e:
    # Try utf-8 if utf-16 fails
    try:
        with open(log_file, "r", encoding="utf-8") as f:
            for line in f:
                match = pattern.search(line)
                if match:
                    warning_codes.append(match.group(1))
    except Exception as e2:
        print(f"Error reading file: {e2}")

from collections import Counter
# Count and print top 20
counts = Counter(warning_codes)
print(f"Found {len(warning_codes)} warnings.")
print("Top warnings:")
for code, count in counts.most_common(20):
    print(f"{code}: {count}")
