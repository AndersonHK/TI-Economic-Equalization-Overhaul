"""Draw review schematics from the authored hull slots (not Unity screenshots).

Run with Python and Pillow. Output is persistent documentation under docs/.
Positions use native half-height grid rows and native armor image offsets;
slot silhouettes and labels are illustrative. No game files are modified.
"""
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'TIEconomyMod/ModFiles/TIShipHullTemplate.json'
OUT = ROOT / 'docs/ship-balance-research/figures'
FONT = Path('C:/Windows/Fonts/segoeui.ttf')
BG = '#0d171c'
COLORS = {'Utility': '#b8e5ec', 'HullHardPoint': '#79d6c1',
          'NoseHardPoint': '#eac180'}
LABELS = {'Utility': 'Utility', 'HullHardPoint': 'Heavy hull',
          'NoseHardPoint': 'Nose', 'Drive': 'Drive', 'PowerPlant': 'Reactor',
          'Radiator': 'Radiator', 'TailArmor': 'Tail armor',
          'LateralArmor': 'Side armor', 'NoseArmor': 'Nose armor',
          'Propellant': 'Propellant'}


def draw_layout(row):
    image = Image.new('RGB', (1600, 1060), BG)
    d = ImageDraw.Draw(image)
    fonts = {size: ImageFont.truetype(str(FONT), size) for size in (17, 20, 23, 38)}
    title = 'Alien Mothership' if row['dataName'] == 'AlienMothership' else row['dataName']
    slots = row['shipModuleSlots']
    counts = {kind: sum(s['moduleSlotType'] == kind for s in slots) for kind in COLORS}
    d.text((42, 24), title + ' / designer slot layout', font=fonts[38], fill='#eefaff')
    d.text((44, 82), f"{counts['Utility']} utilities   |   {counts['HullHardPoint']} hull cells   |   "
           f"{counts['NoseHardPoint']} nose cells", font=fonts[23], fill='#a6c1cb')
    d.text((44, 121), 'JSON-derived schematic; labels and silhouettes are illustrative.',
           font=fonts[20], fill='#829ca6')
    step, size, x0, y0 = 150, 130, 115, 235
    tail = next(s['x'] for s in slots if s['moduleSlotType'] == 'TailArmor')
    nose = next(s['x'] for s in slots if s['moduleSlotType'] == 'NoseArmor')
    # iconSize is the destination cell width; the visible contour has an inset.
    # Native SetupDesignerLayout offsets armor images by iconSize/2 downward.
    for x in range(10):
        d.text((x0 + x * step, 157), str(x), font=fonts[17], fill='#69848d', anchor='mm')
    for y in range(8):
        d.text((24, y0 + y * step / 2), str(y), font=fonts[17], fill='#69848d', anchor='mm')
    for s in slots:
        kind = s['moduleSlotType']
        cx, cy = x0 + s['x'] * step, y0 + s['y'] * step / 2
        if kind.endswith('Armor'):
            cy += step / 2
            if kind == 'LateralArmor' and (tail + nose) % 2:
                cx += step / 2
        color = COLORS.get(kind, '#9ab6d3')
        if kind == 'Utility':
            points = [(0,0),(.8,0),(1,.2),(1,1),(.2,1),(0,.8)]
        elif kind == 'HullHardPoint':
            points = [(.28,0),(.72,0),(1,.28),(1,.72),(.72,1),(.28,1),(0,.72),(0,.28)]
        elif kind == 'NoseHardPoint':
            points = [(.15,0),(1,.08),(1,.92),(.15,1),(0,.85),(0,.15)]
        elif kind.endswith('Armor'):
            points = [(.5,0),(1,.37),(.8,1),(.2,1),(0,.37)]
        else:
            points = [(.14,0),(.86,0),(1,.14),(1,.86),(.86,1),(.14,1),(0,.86),(0,.14)]
        xy = [(round(cx+(x-.5)*size),round(cy+(y-.5)*size)) for x,y in points]
        d.polygon(xy, fill='#13242c')
        d.line(xy+[xy[0]], fill=color, width=4)
        label = LABELS[kind]
        if kind == 'HullHardPoint' and row['dataName'] == 'Dreadnought':
            label = 'Hull'
        d.text((cx,cy-6), label, fill=color, font=fonts[20], anchor='mm')
        d.text((cx,cy+20), f"({s['x']},{s['y']})", fill='#869da7', font=fonts[17], anchor='mm')
    d.line((44, 885, 1555, 885), fill='#34505e', width=2)
    d.text((44, 906), 'Coordinates in each slot are JSON (x,y). Armor icons include the native display offset.',
           font=fonts[20], fill='#a6c1cb')
    d.text((44, 941), 'Placement grid only: counters, installed equipment and other designer controls are omitted.',
           font=fonts[20], fill='#a6c1cb')
    d.text((44, 984), 'Source: TIShipHullTemplate.json / '+row['dataName']+' / SHA256 '+
           hashlib.sha256(SOURCE.read_bytes()).hexdigest()[:16], font=fonts[17], fill='#69848d')
    path = OUT / (row['dataName'].lower() + '-designer-layout.png')
    image.save(path)
    print(path)


if __name__ == '__main__':
    OUT.mkdir(parents=True, exist_ok=True)
    for hull in json.loads(SOURCE.read_text(encoding='utf-8-sig')):
        if hull['dataName'] in ('Dreadnought', 'Titan', 'AlienMothership'):
            draw_layout(hull)
