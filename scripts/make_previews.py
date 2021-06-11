#!/usr/bin/env python3
import glob
import json
import os
import sys

from PIL import Image

os.chdir(os.path.dirname(os.path.realpath(__file__)))

img_width = int(sys.argv[1]) if len(sys.argv) > 1 else 1920
img_height = int(img_width * 9 / 16)
jpeg_quality = int(sys.argv[2]) if len(sys.argv) > 2 else 75

input_dir = "..\\themes"
output_dir = sys.argv[3] if len(sys.argv) > 3 else "../src/resources/images"

get_middle_item = lambda image_list: image_list[len(image_list) // 2] if image_list else -1

for theme_dir in glob.glob(f"{input_dir}/**"):
    print(f"<- {theme_dir}")

    with open(f"{theme_dir}/theme.json", 'r') as fileobj:
        theme_config = json.load(fileobj)
    theme_name = os.path.basename(theme_dir)

    sunrise_image_id = get_middle_item(theme_config.get("sunriseImageList"))
    day_image_id = theme_config.get("dayHighlight") or get_middle_item(theme_config["dayImageList"])
    sunset_image_id = get_middle_item(theme_config.get("sunsetImageList"))
    night_image_id = theme_config.get("nightHighlight") or get_middle_item(theme_config["nightImageList"])

    image_filenames = {
        "day": theme_config["imageFilename"].replace("*", str(day_image_id)),
        "night": theme_config["imageFilename"].replace("*", str(night_image_id))
    }
    if sunrise_image_id != -1 and sunrise_image_id != day_image_id and sunrise_image_id != night_image_id:
        image_filenames["sunrise"] = theme_config["imageFilename"].replace("*", str(sunrise_image_id))
    if sunset_image_id != -1 and sunset_image_id != day_image_id and sunset_image_id != night_image_id:
        image_filenames["sunset"] = theme_config["imageFilename"].replace("*", str(sunset_image_id))

    for phase, filename in image_filenames.items():
        img = Image.open(f"{theme_dir}/{filename}")
        img.thumbnail((img_width, img_height))
        if jpeg_quality >= 0:
            img.save(f"{output_dir}/{theme_name}_{phase}.jpg", quality=jpeg_quality)
        else:
            img.save(f"{output_dir}/{theme_name}_{phase}.png")

print(f"-> {output_dir}")
