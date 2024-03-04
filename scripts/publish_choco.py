#!/usr/bin/env python3
import os
import subprocess
import sys
import tempfile

import requests
from dotenv import load_dotenv

os.chdir(os.path.dirname(os.path.realpath(__file__)))
load_dotenv()

chocolatey_repo = "https://push.chocolatey.org/"
nuspec_filename = "windynamicdesktop.nuspec"
script_filename = "tools/chocolateyInstall.ps1"

def installer_checksum(tag_name, filename):
    checksums = {}
    r = requests.get(f"https://github.com/t1m0thyj/WinDynamicDesktop/releases/download/{tag_name}/checksums.txt",
        allow_redirects=True)
    for line in r.text.splitlines():
        fst, snd = line.split()
        checksums[snd] = fst
    return checksums[filename]

def render_template(filename, replacers):
    with open(filename, 'r', encoding="utf-8") as fileobj:
        old_text = fileobj.read()
    new_text = old_text
    for key, value in replacers.items():
        new_text = new_text.replace("{{" + key + "}}", value)
    write_file(filename, new_text)
    return old_text

def write_file(filename, contents):
    with open(filename, 'w', encoding="utf-8") as fileobj:
        fileobj.write(contents)

release_tag = f"tags/{sys.argv[1]}" if len(sys.argv) > 1 else "latest"
r = requests.get(f"https://api.github.com/repos/t1m0thyj/WinDynamicDesktop/releases/{release_tag}")
response = r.json()
installer_url = next(a for a in response["assets"] if a["name"].endswith("x86_Setup.exe"))["browser_download_url"]
installer_url64 = next(a for a in response["assets"] if a["name"].endswith("x64_Setup.exe"))["browser_download_url"]
package_version = response["tag_name"].removeprefix("v")
replacers = {
    "installerChecksum": installer_checksum(response["tag_name"], os.path.basename(installer_url)),
    "installerUrl": installer_url,
    "installerChecksum64": installer_checksum(response["tag_name"], os.path.basename(installer_url64)),
    "installerUrl64": installer_url64,
    "packageVersion": package_version,
    "releaseNotes": "\n".join(response["body"].splitlines()),
    "releaseTagName": response["tag_name"]
}

old_nuspec = render_template(nuspec_filename, replacers)
old_script = render_template(script_filename, replacers)

subprocess.run(["choco", "pack", "--out", tempfile.gettempdir()])

write_file(nuspec_filename, old_nuspec)
write_file(script_filename, old_script)

nupkg_filename = f"windynamicdesktop.{package_version}.nupkg"
if os.getenv("CI") or input(f"Push {nupkg_filename}? (y/N) ").lower() == "y":
    subprocess.run(["choco", "push", os.path.join(tempfile.gettempdir(), nupkg_filename), "-s", chocolatey_repo, "-k",
        os.getenv("CHOCO_API_KEY")])
