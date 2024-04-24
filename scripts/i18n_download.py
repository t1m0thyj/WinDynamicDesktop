#!/usr/bin/env python3
import os
import shutil
import sys

from dotenv import load_dotenv
from poeditor import POEditorAPI

os.chdir(os.path.dirname(os.path.realpath(__file__)))
load_dotenv()

file_type = sys.argv[1] if len(sys.argv) > 1 else "mo"
output_dir = sys.argv[2] if len(sys.argv) > 2 else "../src/locale"

if len(sys.argv) > 2 and os.path.isdir(output_dir):
    shutil.rmtree(output_dir)
os.makedirs(output_dir, exist_ok=True)

client = POEditorAPI(os.getenv("POEDITOR_API_TOKEN"))
projects = client.list_projects()
project_id = [proj for proj in projects if proj["name"] == "WinDynamicDesktop"][0]["id"]
languages = client.list_project_languages(project_id)

for lang in languages:
    language_code = lang["code"]
    output_file = f"{output_dir}/{language_code}.{file_type}"
    if lang["percentage"] < 50 and not os.path.isfile(output_file):
        continue
    print(f"Downloading translation for {language_code}")
    client.export(project_id, language_code, file_type, local_file=output_file)
