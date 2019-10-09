import glob
import os
import subprocess
import sys

LOCALE_MAPPING = {
    "cs": "cs_CZ",
    "de_DE": "de_DE",
    "el": "el_GR",
    "es_ES": "es_ES",
    "fr": "fr_FR",
    "it": "it_IT",
    "pl": "pl_PL",
    "ro": "ro_RO",
    "ru": "ru_RU",
    "tr": "tr_TR",
    "zh_Hans_CN": "zh_CN"
}

sys.path.append(os.path.join(os.path.dirname(sys.executable), "Tools", "i18n"))

from msgfmt import make

subprocess.run(["powershell.exe", "zanata-cli/bin/zanata-cli", "pull", "-B"])

for filename in glob.glob("../src/locale/*.mo"):
    os.remove(filename)

for name, name2 in LOCALE_MAPPING.items():
    make(f"po/{name}.po", f"../src/locale/{name2}.mo")
