#!/usr/bin/env python3
import os
import shutil

from dotenv import load_dotenv
from poeditor import POEditorAPI

os.chdir(os.path.dirname(os.path.realpath(__file__)))
load_dotenv()

mo_dir = "../i18n/mo"
po_dir = "../i18n/po"

if os.path.isdir(mo_dir):
    shutil.rmtree(mo_dir)
os.mkdir(mo_dir)

if os.path.isdir(po_dir):
    shutil.rmtree(po_dir)
os.mkdir(po_dir)

client = POEditorAPI(os.getenv("POEDITOR_TOKEN"))
projects = client.list_projects()
project_id = [proj for proj in projects if proj["name"] == "WinDynamicDesktop"][0]["id"]
languages = client.list_project_languages(project_id)
language_codes = [lang["code"] for lang in languages if lang["percentage"] >= 50]

for lc in language_codes:
    print(f"Downloading translation for {lc}")
    client.export(project_id, lc, "po", local_file=f"{po_dir}/{lc}.po")
    client.export(project_id, lc, "mo", local_file=f"{mo_dir}/{lc}.mo")
