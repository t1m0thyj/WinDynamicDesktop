import json
import os
import strutils
import terminal

let log = if isatty(stdout): stdout else: open("wdd-report.log", fmWrite)
defer: log.close()

var settings : JsonNode
try:
    let jsonText = readFile("settings.conf")
    settings = parseJson(jsonText)
except:
    discard

if settings != nil:
    settings["location"] = %* "redacted"
    settings["latitude"] = %* "0"
    settings["longitude"] = %* "0"
    log.writeLine "./settings.conf"
    log.writeLine pretty(settings)
else:
    log.writeLine "WARNING: Settings file not found or invalid"

if existsDir("themes"):
    for path in walkDirRec("themes", {pcFile, pcDir}):
        log.writeLine "./" & path.replace('\\', '/')
        if path.endsWith(".json"):
            log.writeLine readFile(path)
else:
    log.writeLine "WARNING: Themes directory not found"
