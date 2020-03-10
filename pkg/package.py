import hashlib
import os.path
import subprocess
import sys

import requests

nuspec_filename = "windynamicdesktop.nuspec"
script_filename = "tools/chocolateyInstall.ps1"

def render_template(filename, replacers):
    with open(filename, 'r', encoding="utf8") as fileobj:
        old_text = fileobj.read()
    new_text = old_text
    for key, value in replacers.items():
        new_text = new_text.replace("{{" + key + "}}", value)
    write_file(filename, new_text)
    return old_text

def sha256_checksum(filename, block_size=65536):
    sha256 = hashlib.sha256()
    with open(filename, 'rb') as f:
        for block in iter(lambda: f.read(block_size), b''):
            sha256.update(block)
    return sha256.hexdigest()

def write_file(filename, contents):
    with open(filename, 'w', encoding="utf8") as fileobj:
        fileobj.write(contents)

r = requests.get("https://api.github.com/repos/t1m0thyj/WinDynamicDesktop/releases/latest")
response = r.json()
installerUrl = response["assets"][1]["browser_download_url"]
replacers = {
    "installerChecksum": sha256_checksum("../dist/" + os.path.basename(installerUrl)),
    "installerUrl": installerUrl,
    "packageVersion": response["tag_name"][1:],
    "releaseNotes": response["body"]
}

old_nuspec = render_template(nuspec_filename, replacers)
old_script = render_template(script_filename, replacers)

subprocess.call(["choco", "pack"])

write_file(nuspec_filename, old_nuspec)
write_file(script_filename, old_script)
