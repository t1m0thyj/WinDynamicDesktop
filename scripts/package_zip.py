import fnmatch
import os
import subprocess

from win32com.client import Dispatch

os.chdir(os.path.dirname(os.path.realpath(__file__)))

build_dir = "..\\src\\bin\\Release"
patterns_exclude = (
    "cef/*.pdb",
    "cef/*.xml",
    "cef/README.txt"
)
patterns_include = (
    "assets/**",
    "cef/**",
    "WinDynamicDesktop.exe",
    "WinDynamicDesktop.exe.config"
)


def should_include_file(name):
    name = name.replace("\\", "/").lstrip("./")

    for pattern in patterns_exclude:
        if fnmatch.fnmatch(name, pattern):
            return False

    for pattern in patterns_include:
        if fnmatch.fnmatch(name, pattern):
            return True

    return False


for root, dirs, files in os.walk(build_dir, topdown=False):
    rel_root = os.path.relpath(root, build_dir)

    for name in dirs:
        abs_name = os.path.join(root, name)
        rel_name = os.path.join(rel_root, name)

        if not os.listdir(abs_name):
            print("+ rm", rel_name.replace("\\", "/"))
            os.rmdir(abs_name)

    for name in files:
        abs_name = os.path.join(root, name)
        rel_name = os.path.join(rel_root, name)

        if not should_include_file(rel_name):
            print("+ rm", rel_name.replace("\\", "/"))
            os.unlink(abs_name)

app_version = Dispatch("Scripting.FileSystemObject").GetFileVersion(f"{build_dir}\\WinDynamicDesktop.exe")
subprocess.call(["7z", "a", f"..\\dist\\WinDynamicDesktop_Portable_{app_version}.zip", f"{build_dir}\\*"])
