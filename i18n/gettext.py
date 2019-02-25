import glob
import re

with open("messages.pot", 'w') as potfile:
    for filename in glob.glob("../src/*.cs"):
        msg_data = []

        with open(filename, 'r') as csfile:
            if not filename.endswith(".Designer.cs"):
                msg_history = []

                for i, line in enumerate(csfile):
                    if msg_history:
                        match = re.search(r'^\s*"(.+?)"(\)?)', line)

                        if match:
                            msg_history[1] += match.group(1)

                            if match.group(2):
                                msg_data.append(tuple(msg_history))
                                msg_history = []

                    for match in re.finditer(r'_\("(.+?)"\)', line):
                        msg_data.append((i + 1, match.group(1)))

                    if not msg_history:
                        match = re.search(r'_\("(.+)"[^)]', line)

                        if match:
                            msg_history = [i + 1, match.group(1)]
            else:
                for i, line in enumerate(csfile):
                    match = re.search(r'\.Text = "(.+)";$', line)

                    if match:
                        msg_data.append((i + 1, match.group(1)))

        for num, msgid in msg_data:
            linez = [
                "#: {}:{}".format(filename[3:], num),
                "msgid=\"{}\"".format(msgid),
                "msgstr=\"\"",
                ""
            ]

            for line in linez:
                print(line)
                print(line, file=potfile)
