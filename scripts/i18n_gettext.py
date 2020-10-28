#!/usr/bin/env python3
import glob
import os
import re
import time
from collections import OrderedDict

os.chdir(os.path.dirname(os.path.realpath(__file__)))

app_name = "WinDynamicDesktop"
exclude_patterns = [app_name + r'$', r'\w+://', r'\w+\d+$', r'\W+$']
pot_data = OrderedDict()


def add_to_pot_data(msgid, filename, lineno):
    if any([re.match(pattern, msgid) for pattern in exclude_patterns]):
        return
    elif msgid not in pot_data:
        pot_data[msgid] = [(filename, lineno)]
    else:
        pot_data[msgid].append((filename, lineno))


for filename in glob.glob("../src/**/*.cs", recursive=True):
    with open(filename, 'r', encoding="utf8") as cs_file:
        if not filename.endswith(".Designer.cs"):
            msg_history = []

            for i, line in enumerate(cs_file):
                if msg_history:
                    match = re.search(r'^\s*"(.+?)"(\)?)', line)

                    if match:
                        msg_history[0] += match.group(1)

                        if match.group(2):
                            add_to_pot_data(msg_history[0], filename[3:], msg_history[1])
                            msg_history = []

                for match in re.finditer(r'(?:_|Localization\.GetTranslation)\("(.+?)"\)', line):
                    add_to_pot_data(match.group(1), filename[3:], i + 1)

                if not msg_history:
                    match = re.search(r'(?:_|Localization\.GetTranslation)\("(.+)"\s', line)

                    if match:
                        msg_history = [match.group(1), i + 1]
        else:
            for i, line in enumerate(cs_file):
                match = re.search(r'\.Text = "(.+)";$', line)

                if match:
                    add_to_pot_data(match.group(1), filename[3:], i + 1)

pot_lines = [
    "# SOME DESCRIPTIVE TITLE",
    "# Copyright (C) YEAR ORGANIZATION",
    "# FIRST AUTHOR <EMAIL@ADDRESS>, YEAR.",
    "#",
    "msgid \"\"",
    "msgstr \"\"",
    "\"Project-Id-Version: {}\\n\"".format(app_name),
    "\"POT-Creation-Date: {}\\n\"".format(time.strftime("%Y-%m-%d %H:%M%z")),
    "\"PO-Revision-Date: YEAR-MO-DA HO:MI+ZONE\\n\"",
    "\"Last-Translator: FULL NAME <EMAIL@ADDRESS>\\n\"",
    "\"Language-Team: LANGUAGE <LL@li.org>\\n\"",
    "\"MIME-Version: 1.0\\n\"",
    "\"Content-Type: text/plain; charset=UTF-8\\n\"",
    "\"Content-Transfer-Encoding: 8bit\\n\""
]

for msgid, locs in pot_data.items():
    pot_lines.append("")
    pot_lines.append("#: {}".format(" ".join(["{}:{}".format(filename, lineno) for filename, lineno in locs])))

    if "\\n" in msgid:
        msgid_lines = msgid.split("\\n")

        for i in range(len(msgid_lines) - 1):
            msgid_lines[i] += "\\n"

        pot_lines.append("msgid \"\"")
        pot_lines.extend(["\"{}\"".format(line) for line in msgid_lines])
    else:
        pot_lines.append("msgid \"{}\"".format(msgid))

    pot_lines.append("msgstr \"\"")

with open("../i18n/messages.pot", 'w', encoding="utf8") as pot_file:
    for line in pot_lines:
        print(line, file=pot_file)
