#!/usr/bin/env python3
import os
import subprocess
import sys

import requests
from dotenv import load_dotenv

os.chdir(os.path.dirname(os.path.realpath(__file__)))
load_dotenv()

chocolatey_repo = "https://push.chocolatey.org/"
nuspec_filename = "windynamicdesktop.nuspec"
script_filename = "tools/chocolateyInstall.ps1"

def installer_checksum(filename):
    checksums = {}
    with open("../dist/checksums.txt", 'r') as fileobj:
        for line in fileobj:
            fst, snd = line.split()
            checksums[snd] = fst
    return checksums[filename]

def render_template(filename, replacers):
    with open(filename, 'r', encoding="utf8") as fileobj:
        old_text = fileobj.read()
    new_text = old_text
    for key, value in replacers.items():
        new_text = new_text.replace("{{" + key + "}}", value)
    write_file(filename, new_text)
    return old_text

def write_file(filename, contents):
    with open(filename, 'w', encoding="utf8") as fileobj:
        fileobj.write(contents)

r = requests.get("https://api.github.com/repos/t1m0thyj/WinDynamicDesktop/releases/latest")
response = r.json()
installer_url = next(a for a in response["assets"] if a["name"].endswith("Setup.exe"))["browser_download_url"]
package_version = sys.argv[1] if len(sys.argv) > 1 else response["tag_name"][1:]
replacers = {
    "installerChecksum": installer_checksum(os.path.basename(installer_url)),
    "installerUrl": installer_url,
    "packageVersion": package_version,
    "releaseNotes": "\n".join(response["body"].splitlines()),
    "releaseTagName": response["tag_name"]
}

old_nuspec = render_template(nuspec_filename, replacers)
old_script = render_template(script_filename, replacers)

subprocess.run(["choco", "pack", "--out", "../dist"])

write_file(nuspec_filename, old_nuspec)
write_file(script_filename, old_script)

nupkg_filename = f"../dist/windynamicdesktop.{package_version}.nupkg"
if input(f"Push {nupkg_filename}? (y/N) ").lower() == "y":
    subprocess.run(["choco", "push", nupkg_filename, "-s", chocolatey_repo, "-k", os.getenv("CHOCO_APIKEY")])
