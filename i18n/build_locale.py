import glob
import json
import os
import sys
import urllib.request

LOCALE_MAPPING = {
    "cs": "cs_CZ",
    "de_DE": "de_DE",
    "el": "el_GR",
    "es_ES": "es_ES",
    "fr": "fr_FR",
    "it": "it_IT",
    "mk": "mk_MK",
    "pl": "pl_PL",
    "ro": "ro_RO",
    "ru": "ru_RU",
    "tr": "tr_TR",
    "zh_Hans_CN": "zh_CN"
}

sys.path.append(os.path.join(os.path.dirname(sys.executable), "Tools", "i18n"))

from msgfmt import make

if not os.path.isdir("po"):
    os.mkdir("po")

with urllib.request.urlopen("https://translate.zanata.org/rest/project/windynamicdesktop/") as fileobj:
    iteration_slug = json.load(fileobj)["iterations"][0]["id"]

for name in LOCALE_MAPPING.keys():
    url_name = name.replace("_", "-")
    print(f"Downloading translation for {url_name}")
    urllib.request.urlretrieve(f"https://translate.zanata.org/rest/file/translation/windynamicdesktop/{iteration_slug}/{url_name}/po?docId=messages", f"po/{name}.po")

for filename in glob.glob("../src/locale/*.mo"):
    os.remove(filename)

for name, name2 in LOCALE_MAPPING.items():
    make(f"po/{name}.po", f"../src/locale/{name2}.mo")
