from pathlib import Path
import sys
import xml.etree.ElementTree as ET


results_dir = Path("tests/AkarangaBank.Web.Tests/TestResults")
reports = list(results_dir.glob("*/coverage.cobertura.xml")) if results_dir.exists() else []
if not reports:
    print("No Cobertura coverage report was produced.", file=sys.stderr)
    sys.exit(1)

covered = 0
valid = 0
for report in reports:
    root = ET.parse(report).getroot()
    covered += int(root.attrib.get("lines-covered", "0"))
    valid += int(root.attrib.get("lines-valid", "0"))

if valid == 0:
    print("Coverage reports contain no executable lines.", file=sys.stderr)
    sys.exit(1)

percentage = covered / valid * 100
print(f"Combined line coverage: {percentage:.2f}% ({covered}/{valid} lines); minimum: 60.00%")
if percentage < 60:
    sys.exit(1)
