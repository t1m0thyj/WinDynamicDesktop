#!/usr/bin/env python3
import glob
import json
import os
import sys

from PIL import Image

os.chdir(os.path.dirname(os.path.realpath(__file__)))

img_width = int(sys.argv[1]) if len(sys.argv) > 1 else 256
img_height = int(img_width * 9 / 16)
jpeg_quality = int(sys.argv[2]) if len(sys.argv) > 2 else 95

input_dir = "..\\themes"
output_dir = sys.argv[3] if len(sys.argv) > 3 else "../src/resources/images"

get_middle_item = lambda image_list: image_list[len(image_list) // 2]

for theme_dir in glob.glob(f"{input_dir}/**"):
    print(f"<- {theme_dir}")

    with open(f"{theme_dir}/theme.json", 'r') as fileobj:
        theme_config = json.load(fileobj)
    theme_name = os.path.basename(theme_dir)

    light_image_id = theme_config.get("dayHighlight") or get_middle_item(theme_config["dayImageList"])
    light_image_filename = theme_config["imageFilename"].replace("*", str(light_image_id))
    dark_image_id = theme_config.get("nightHighlight") or get_middle_item(theme_config["nightImageList"])
    dark_image_filename = theme_config["imageFilename"].replace("*", str(dark_image_id))

    img1 = Image.open(f"{theme_dir}/{light_image_filename}")
    img2 = Image.open(f"{theme_dir}/{dark_image_filename}")

    img2.paste(img1.crop((0, 0, img1.width // 2, img1.height)))
    img2.thumbnail((img_width, img_height))
    if jpeg_quality >= 0:
        img2.save(f"{output_dir}/{theme_name}_thumbnail.jpg", quality=jpeg_quality)
    else:
        img2.save(f"{output_dir}/{theme_name}_thumbnail.png")

print(f"-> {output_dir}")
