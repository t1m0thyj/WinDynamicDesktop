#!/usr/bin/env python3
import glob
import json
import os
import sys

from PIL import Image

os.chdir(os.path.dirname(os.path.realpath(__file__)))

img_width = int(sys.argv[1])
img_height = int(img_width * 9 / 16)
jpeg_quality = int(sys.argv[2]) if len(sys.argv) > 2 else 75

input_dir = "..\\themes"
output_dir = f"../src/resources/images"

for theme_dir in glob.glob(f"{input_dir}/**"):
    print(f"<- {theme_dir}")

    with open(f"{theme_dir}/theme.json", 'r') as fileobj:
        theme_config = json.load(fileobj)
    theme_name = os.path.basename(theme_dir)

    sunrise_image_id = theme_config["sunriseImageList"][len(theme_config["sunriseImageList"]) // 2]
    day_image_id = theme_config.get("dayHighlight") or theme_config["dayImageList"][len(theme_config["dayImageList"]) // 2]
    sunset_image_id = theme_config["sunsetImageList"][len(theme_config["sunsetImageList"]) // 2]
    night_image_id = theme_config.get("nightHighlight") or theme_config["nightImageList"][len(theme_config["nightImageList"]) // 2]

    day_image_filename = theme_config["imageFilename"].replace("*", str(day_image_id))
    night_image_filename = theme_config["imageFilename"].replace("*", str(night_image_id))
    sunrise_image_filename = None
    if sunrise_image_id != day_image_id and sunrise_image_id != night_image_id:
        sunrise_image_filename = theme_config["imageFilename"].replace("*", str(sunrise_image_id))
    sunset_image_filename = None
    if sunset_image_id != day_image_id and sunset_image_id != night_image_id:
        sunset_image_filename = theme_config["imageFilename"].replace("*", str(sunset_image_id))

    if sunrise_image_filename:
        img = Image.open(f"{theme_dir}/{sunrise_image_filename}")
        img.thumbnail((img_width, img_height))
        img.save(f"{output_dir}/{theme_name}_sunrise.jpg", quality=jpeg_quality)

    img = Image.open(f"{theme_dir}/{day_image_filename}")
    img.thumbnail((img_width, img_height))
    img.save(f"{output_dir}/{theme_name}_day.jpg", quality=jpeg_quality)

    if sunset_image_filename:
        img = Image.open(f"{theme_dir}/{sunset_image_filename}")
        img.thumbnail((img_width, img_height))
        img.save(f"{output_dir}/{theme_name}_sunset.jpg", quality=jpeg_quality)

    img = Image.open(f"{theme_dir}/{night_image_filename}")
    img.thumbnail((img_width, img_height))
    img.save(f"{output_dir}/{theme_name}_night.jpg", quality=jpeg_quality)

print(f"-> {output_dir}")
