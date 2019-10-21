import os

from poeditor import POEditorAPI

if not os.path.isdir("mo"):
    os.mkdir("mo")

if not os.path.isdir("po"):
    os.mkdir("po")

with open("poeditor_token", 'r') as fileobj:
    api_token = fileobj.readline().strip()

client = POEditorAPI(api_token)
projects = client.list_projects()
project_id = [p for p in projects if p["name"] == "WinDynamicDesktop"][0]["id"]
languages = client.list_project_languages(project_id)
language_codes = [l["code"] for l in languages]

for lc in language_codes:
    print(f"Downloading translation for {lc}")
    client.export(project_id, lc, "po", local_file=f"po/{lc}.po")
    client.export(project_id, lc, "mo", local_file=f"mo/{lc}.mo")
