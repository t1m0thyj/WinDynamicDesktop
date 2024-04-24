#!/usr/bin/env python3
import os

import polib

os.chdir(os.path.dirname(os.path.realpath(__file__)))

errors = 0
with open("../src/Localization.cs", 'r', encoding="utf-8") as fileobj:
    l10n_src = fileobj.read()
for filename in os.listdir("../src/locale"):
    if not filename.endswith(".mo"):
        continue
    language_code = filename[:-3]
    if f"\"{language_code}\"" not in l10n_src[:l10n_src.index("};")]:
        print(f"[{language_code}] ERROR: Language missing in Localization.cs")
        errors += 1
    for entry in polib.mofile(f"../src/locale/{filename}"):
        i = 0
        while f"{{{i}}}" in entry.msgid:
            if f"{{{i}}}" not in entry.msgstr:
                print(f"[{language_code}] {entry.msgid}\n\t{entry.msgstr}")
                errors += 1
                break
            i += 1
print(f"\n❌ {errors} problem(s) found" if errors else "✅ No problems found")
input()
