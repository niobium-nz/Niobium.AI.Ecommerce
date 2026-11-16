"""Run affected downstream contract tests with the kit's verified offline libraries."""
import argparse
import json
from pathlib import Path
import runpy
import sys
import unittest

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--bundle", type=Path, required=True)
args = parser.parse_args()
repo = Path(__file__).resolve().parents[2]
support = args.bundle.resolve() / "agent-files/offline-validation-support/validate.py"
# Loading this helper disables network and subprocess actions in this process.
helper = runpy.run_path(str(support))
with helper["dependencies"]():
    tests = repo / "agents/ecommerce-discovery-looper/tests"
    sys.path.insert(0, str(tests))
    suite = unittest.defaultTestLoader.discover(str(tests), pattern="test_*.py")
    result = unittest.TextTestRunner(verbosity=1).run(suite)
    print(json.dumps({"consumer": "ecommerce-discovery-looper", "tests": result.testsRun,
                      "failures": len(result.failures), "errors": len(result.errors),
                      "scope": "offline contract and mock reference tests, not live host execution"}))
    raise SystemExit(0 if result.wasSuccessful() and result.testsRun > 0 and not result.skipped else 1)
