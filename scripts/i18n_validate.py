#!/usr/bin/env python3
import os

import polib

os.chdir(os.path.dirname(os.path.realpath(__file__)))

errors = 0
for filename in os.listdir("../src/locale"):
    if not filename.endswith(".mo"):
        continue
    for entry in polib.mofile(f"../src/locale/{filename}"):
        i = 0
        while f"{{{i}}}" in entry.msgid:
            if f"{{{i}}}" not in entry.msgstr:
                print(f"[{filename[:-3]}] {entry.msgid}\n\t{entry.msgstr}")
                errors += 1
                break
            i += 1
print(f"\n❌ {errors} problem(s) found" if errors else "✅ No problems found")
