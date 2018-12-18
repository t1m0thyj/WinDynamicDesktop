import json
import os
import sys
from PIL import Image

theme_name = sys.argv[1]

with open(theme_name + ".json", 'r') as fileobj:
    theme_config = json.load(fileobj)


def get_middle_image_filename(theme_config, image_list):
    image_id = theme_config[image_list][(len(theme_config[image_list]) + 1) // 2]
    return theme_config["imageFilename"].replace("*", str(image_id))


day_image_filename = get_middle_image_filename(theme_config, "dayImageList")
night_image_filename = get_middle_image_filename(theme_config, "nightImageList")
output_filename = theme_name.lower() + "_thumbnail.png"

img1 = Image.open(day_image_filename)
img2 = Image.open(night_image_filename)
w, h = img1.size
img2.paste(img1.crop((0, 0, w // 2, h)))
h2 = 300
w2 = w * h2 // h
img2.thumbnail((w2, h2))
img2.save(output_filename)
os.startfile(output_filename)
