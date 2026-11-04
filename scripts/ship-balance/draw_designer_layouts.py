"""Draw review schematics from the authored hull slots (not Unity screenshots).

Run with Python and Pillow. Output is persistent documentation under docs/.
Positions use native half-height grid rows and native armor image offsets;
slot silhouettes and labels are illustrative. No game files are modified.
"""
import hashlib
import json
import argparse
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'TIEconomyMod/ModFiles/TIShipHullTemplate.json'
LAYOUT = ROOT / 'TIEconomyMod/ModFiles/Config/ship-designer-layout.eeo'
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


def control_rectangles(row, layout):
    """Native 72-unit icons and recorded spinner rectangles, y down.

    Propellant has no native empty icon. Drive uses the complete spinner
    container prepared by the generic UI loader. Armor images shift 36 units.
    Bounds cover the controls/labels, not just their centre points.
    """
    tiles, controls = [], []
    slots = row['shipModuleSlots']
    tail = next(s['x'] for s in slots if s['moduleSlotType'] == 'TailArmor')
    nose = next(s['x'] for s in slots if s['moduleSlotType'] == 'NoseArmor')
    for index, s in enumerate(slots):
        kind = s['moduleSlotType']
        if kind == 'None':continue
        x, y = s['x'] * layout['ColumnPitch'], s['y'] * layout['HalfRowPitch']
        if kind.endswith('Armor'):
            y += 36
            if kind == 'LateralArmor' and (tail + nose) % 2:
                x += 36
        if kind != 'Propellant':
            tiles.append({'slot':index,'kind':kind,'bounds':[x-36,y-36,x+36,y+36]})
        if kind == 'Drive':
            controls.append({'slot':index,'kind':kind,'bounds':[x-36,y+38.5,x+36,y+58.5]})
        elif kind.endswith('Armor'):
            controls.append({'slot':index,'kind':kind,'bounds':[x-36,y+38.5,x+36,y+80.5]})
        elif kind == 'Propellant':
            style = layout['PropellantLabel']
            top = 46-style['OffsetY']-style['Height']/2
            bottom = max(56,46-style['OffsetY']+style['Height']/2)
            half_width=max(36,style['Width']/2)
            controls.append({'slot':index,'kind':kind,'bounds':[x-half_width,y+top,x+half_width,y+bottom]})
    return tiles,controls


def intersects(a,b):
    return min(a[2],b[2])-max(a[0],b[0]) > .001 and min(a[3],b[3])-max(a[1],b[1]) > .001


def verify_controls(row, layout):
    for s in row['shipModuleSlots']:
        if s['moduleSlotType']=='None':continue
        for rule in layout['ColumnRestrictions']:
            if rule['Hull'] in ('*',row['dataName']) and s['x'] in rule['Columns'] and s['moduleSlotType'] not in rule['AllowedTypes']:
                raise ValueError(f"{row['dataName']}: column schema rejects {s}")
        for cell in layout['ReservedCells']:
            if cell['Hull'] in ('*',row['dataName']) and (cell['X'],cell['Y'])==(s['x'],s['y']):
                raise ValueError(f"{row['dataName']}: reserved cell was filled: {s}")
    tiles,controls=control_rectangles(row,layout)
    for i,a in enumerate(tiles):
        for b in tiles[i+1:]:
            if intersects(a['bounds'],b['bounds']):raise ValueError(f"{row['dataName']}: icons {a['slot']}/{b['slot']} overlap")
    for control in controls:
        for tile in tiles:
            if control['slot']!=tile['slot'] and intersects(control['bounds'],tile['bounds']):
                raise ValueError(f"{row['dataName']}: {control['kind']} controls overlap slot {tile['slot']}")
    return {'hull':row['dataName'],'tiles':tiles,'controls':controls}


def draw_layout(row, layout):
    image = Image.new('RGB', (1600, 1370), BG)
    d = ImageDraw.Draw(image)
    fonts = {size: ImageFont.truetype(str(FONT), size) for size in (14,17,20,21,23,38)}
    title = 'Alien Mothership' if row['dataName'] == 'AlienMothership' else row['dataName']
    slots = row['shipModuleSlots']
    counts = {kind: sum(s['moduleSlotType'] == kind for s in slots) for kind in COLORS}
    d.text((42, 24), title + ' / designer slot layout', font=fonts[38], fill='#eefaff')
    d.text((44, 82), f"{counts['Utility']} utilities   |   {counts['HullHardPoint']} hull cells   |   "
           f"{counts['NoseHardPoint']} nose cells", font=fonts[23], fill='#a6c1cb')
    d.text((44, 121), 'JSON-derived layout + native control rectangles; silhouettes and input values are illustrative.',
           font=fonts[20], fill='#829ca6')
    step, size, x0, y0 = 150, 130, 115, 235
    tail = next(s['x'] for s in slots if s['moduleSlotType'] == 'TailArmor')
    nose = next(s['x'] for s in slots if s['moduleSlotType'] == 'NoseArmor')
    # iconSize is the destination cell width; the visible contour has an inset.
    # Native SetupDesignerLayout offsets armor images by iconSize/2 downward.
    for x in range(10):
        d.text((x0 + x * step, 157), str(x), font=fonts[17], fill='#69848d', anchor='mm')
    for y in range(layout['Rows']+1):
        d.text((24, y0 + y * step / 2), str(y), font=fonts[17], fill='#69848d', anchor='mm')
    for s in slots:
        kind = s['moduleSlotType']
        cx, cy = x0 + s['x'] * step, y0 + s['y'] * step / 2
        scale=step/layout['ColumnPitch']
        if kind.endswith('Armor'):
            cy += 36*scale
            if kind == 'LateralArmor' and (tail + nose) % 2:
                cx += 36*scale
        color = COLORS.get(kind, '#9ab6d3')
        if kind == 'Propellant':
            # Native EmptySlotIconName returns an empty resource for fuel;
            # depict its real label/count control rather than inventing a tile.
            d.text((cx,cy-20),f"({s['x']},{s['y']})",fill='#869da7',font=fonts[17],anchor='mm')
            draw_spinner(d,fonts,kind,cx,cy,scale,layout)
            continue
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
        if kind == 'Drive' or kind.endswith('Armor'):
            draw_spinner(d,fonts,kind,cx,cy,scale,layout)
    d.line((44, 1135, 1555, 1135), fill='#34505e', width=2)
    d.text((44, 1165), 'Coordinates are JSON (x,y). Armor images include the native 36-unit downward offset.',
           font=fonts[20], fill='#a6c1cb')
    d.text((44, 1200), 'Amber outlines mark count/label bounds: those areas cannot be filled by another tile.',
           font=fonts[20], fill='#a6c1cb')
    d.text((44, 1235), 'Drive, fuel and armor inputs are shown. Text/mesh previews and catalog panels are outside this diagram.',
           font=fonts[20], fill='#a6c1cb')
    d.text((44, 1270), 'Native control geometry is checked against tiles; live Unity layout/font behavior still needs manual confirmation.',
           font=fonts[17], fill='#829ca6')
    d.text((44, 1310), 'Source: TIShipHullTemplate.json / '+row['dataName']+' / SHA256 '+
           hashlib.sha256(SOURCE.read_bytes()).hexdigest()[:16], font=fonts[17], fill='#69848d')
    path = OUT / (row['dataName'].lower() + '-designer-layout.png')
    image.save(path)
    print(path)


def draw_spinner(d,fonts,kind,cx,cy,scale,layout):
    offset=46 if kind=='Propellant' else 48.5
    input_y=cy+offset*scale
    if kind=='Propellant':
        style=layout['PropellantLabel'];top=46-style['OffsetY']-style['Height']/2
        bottom=max(56,46-style['OffsetY']+style['Height']/2)
    elif kind=='Drive':top,bottom=38.5,58.5
    else:top,bottom=38.5,80.5
    bounds=(cx-36*scale,cy+top*scale,cx+36*scale,cy+bottom*scale)
    d.rectangle(bounds,fill='#202622',outline='#967a42',width=2)
    if kind=='Propellant':
        label_y=cy+(46-layout['PropellantLabel']['OffsetY'])*scale
        d.multiline_text((cx,label_y),'Propellant\nTanks',fill='#b8dce9',font=fonts[21],anchor='mm',align='center',spacing=0)
    d.rectangle((cx-36*scale,input_y-10*scale,cx+36*scale,input_y+10*scale),fill='#142833',outline='#648c9f',width=2)
    for sign,x in [('−',cx-26*scale),('+',cx+26*scale)]:
        d.rectangle((x-10*scale,input_y-10*scale,x+10*scale,input_y+10*scale),fill='#264452',outline='#648c9f',width=2)
        d.text((x,input_y),sign,font=fonts[23],fill='#d0e7ec',anchor='mm')
    d.text((cx,input_y),'0',font=fonts[20],fill='#d0e7ec',anchor='mm')
    if kind.endswith('Armor'):
        d.text((cx,cy+69.5*scale),'Max: 0',font=fonts[20],fill='#b8dce9',anchor='mm')
    if kind=='Drive':
        d.multiline_text((cx,cy+64*scale+14),'Drive-count\nclearance',font=fonts[14],fill='#b39760',anchor='mm',align='center',spacing=0)


if __name__ == '__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--verify-only',action='store_true',help='Check bounds without creating images or render surfaces.')
    args=parser.parse_args()
    layout=json.loads(LAYOUT.read_text(encoding='utf-8'))
    OUT.mkdir(parents=True, exist_ok=True)
    checked=[]
    for hull in json.loads(SOURCE.read_text(encoding='utf-8-sig')):
        if hull['dataName'] in ('Lancer','Dreadnought','Titan','AlienDreadnought','AlienTitan','AlienMothership'):
            checked.append(verify_controls(hull,layout))
            if not args.verify_only:draw_layout(hull,layout)
    if not args.verify_only:
        target=ROOT/'docs/ship-balance-research/alien-capital-expansion/designer-control-clearance-0.11.2.json'
        target.write_text(json.dumps({'method':'Recorded native 72-unit icon and spinner rectangles; JSON propellant label dimensions; no rendering claim.',
            'sourceHullSha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest().upper(),
            'sourceLayoutSha256':hashlib.sha256(LAYOUT.read_bytes()).hexdigest().upper(),'hulls':checked},indent=2)+'\n',encoding='utf-8')
    print('PASS: native control rectangles are clear of all foreign tiles on six capital layouts; no images created.' if args.verify_only else 'PASS: six diagrams include native spinner/label bounds.')
