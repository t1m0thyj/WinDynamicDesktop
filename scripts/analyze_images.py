import glob
import json
import sys
from PIL import Image

theme_name = sys.argv[1]

with open(theme_name + ".json", 'r') as fileobj:
    theme_config = json.load(fileobj)

image_data = []
print("Analyzing images", end="", flush=True)

for i, filename in enumerate(glob.glob(theme_config["imageFilename"])):
    img = Image.open(filename)
    image_data.append(0)
    print('.', end="", flush=True)

    for r, g, b in img.getdata():
        # Brightness approximation from https://stackoverflow.com/a/596241/5504760
        image_data[i] += (2*r + 3*g + b) / 6
    
print()
print("Darkest image:", image_data.index(min(image_data)) + 1)
print("Lightest image:", image_data.index(max(image_data)) + 1)
