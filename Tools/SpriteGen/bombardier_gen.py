"""Procedural pixel-art generator for the Bombardier, kept as a template for future characters.

Draws the Idle (6), Run (4) and Shoot (8) sheets as 192px cells that match the existing slicing
(each frame is a full 192x192 cell, pivot centre) in the Tiny Swords blue palette, with 2px outlines.

Usage (needs Python 3 with numpy and Pillow), from the repo root:
    python Tools/SpriteGen/bombardier_gen.py [out_dir] [preview.png] [reference_sheet.png]

Defaults: writes the sheets straight into the Bombardier's Sprite folder, and a zoomed preview
(the archer first for scale, then every frame) to Tools/SpriteGen/out/preview.png.

For a new character: copy this file, change the drawing in frame() and the pose lists at the bottom,
and point the output at the new character's Sprite folder. Shapes are in design units, scaled by K
around the feet, so K sets the character's size without touching any coordinates.
"""
import sys
from pathlib import Path
import numpy as np
from PIL import Image

_HERE = Path(__file__).resolve().parent
_REPO = _HERE.parent.parent
_ARGS = sys.argv[1:]
OUT = _ARGS[0] if len(_ARGS) > 0 else str(_REPO / 'Assets/GameObjects/Player/Characters/Bombardier/Sprite')
PREVIEW = _ARGS[1] if len(_ARGS) > 1 else str(_HERE / 'out/preview.png')
REFERENCE = _ARGS[2] if len(_ARGS) > 2 else str(_REPO / 'Assets/GameObjects/Player/Characters/Archer/Sprite/Archer_Idle.png')
CELL = 192

def hexc(h, a=255):
    h = h.lstrip('#')
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)

# Tiny Swords palette (blue units), plus one darker blue/teal for shading
OUTLINE = hexc('161c2e')
BLUE    = hexc('485884')
BLUE_D  = hexc('353f66')
TEAL    = hexc('4697ac')
TEAL_D  = hexc('35768c')
HELM    = hexc('9cbeaa')
HELM_L  = hexc('d4edc2')
HELM_D  = hexc('688c8a')
SKIN    = hexc('efe1ab')
SKIN_D  = hexc('c8a876')
BROWN   = hexc('977b6b')
BROWN_D = hexc('5e5455')
BOMB    = hexc('405e86')
BOMB_D  = hexc('473e60')
BOMB_L  = hexc('6faaaf')
FUSE    = hexc('aa805f')
SPARK   = hexc('e0e07f')
WHITE   = hexc('ffffff')
SHADOW  = hexc('0f121a', 79)

# Shapes are described in design units and drawn K times bigger around the feet,
# while outlines and edge shading stay 1 pixel thick, so the extra size becomes extra detail
K = 1.2
PIVOT_X, PIVOT_Y = 96, 131
_PY, _PX = np.mgrid[0:CELL, 0:CELL]
XX = PIVOT_X + (_PX - PIVOT_X) / K
YY = PIVOT_Y + (_PY - PIVOT_Y) / K

def vline(x):
    """A one-pixel-thick vertical line at design x."""
    return np.abs(XX - x) < 1 / K

def pt(x, y):
    """A single pixel at a design position."""
    return (np.abs(XX - x) < 0.5 / K) & (np.abs(YY - y) < 0.5 / K)

def hline(y):
    """A one-pixel-thick horizontal line at design height y (scalar or per-column array)."""
    return np.abs(YY - y) < 1 / K

def ellipse(cx, cy, rx, ry):
    return ((XX - cx + 0.5) / rx) ** 2 + ((YY - cy + 0.5) / ry) ** 2 <= 1.0

def rect(x0, y0, x1, y1):
    return (XX >= x0) & (XX < x1 + 1) & (YY >= y0) & (YY < y1 + 1)

def round_rect(x0, y0, x1, y1, r_top, r_bottom):
    m = rect(x0, y0, x1, y1)
    for (cx, cy, r, top) in ((x0 + r_top, y0 + r_top, r_top, True), (x1 - r_top, y0 + r_top, r_top, True),
                             (x0 + r_bottom, y1 - r_bottom, r_bottom, False), (x1 - r_bottom, y1 - r_bottom, r_bottom, False)):
        if r <= 0:
            continue
        left = cx < (x0 + x1) / 2
        corner = (XX < cx if left else XX > cx) & (YY < cy if top else YY > cy)
        m &= ~(corner & ~ellipse(cx + 0.5, cy + 0.5, r + 0.5, r + 0.5))
    return m

def thick_line(x0, y0, x1, y1, w=1):
    m = np.zeros((CELL, CELL), bool)
    steps = int(max(abs(x1 - x0), abs(y1 - y0)) * 2) + 1
    for t in np.linspace(0, 1, steps):
        x, y = x0 + (x1 - x0) * t, y0 + (y1 - y0) * t
        m |= rect(round(x - w / 2), round(y - w / 2), round(x - w / 2) + w - 1, round(y - w / 2) + w - 1)
    return m

def shift(m, dx, dy):
    out = np.zeros_like(m)
    ys = slice(max(dy, 0), CELL + min(dy, 0))
    yd = slice(max(-dy, 0), CELL + min(-dy, 0))
    xs = slice(max(dx, 0), CELL + min(dx, 0))
    xd = slice(max(-dx, 0), CELL + min(-dx, 0))
    out[ys, xs] = m[yd, xd]
    return out

def dilate(m):
    return m | shift(m, 1, 0) | shift(m, -1, 0) | shift(m, 0, 1) | shift(m, 0, -1)

def edge(m, dx, dy, depth=1):
    """Pixels of m within `depth` of its edge on the side opposite (dx, dy)."""
    px = max(1, int(depth * K + 0.5))
    return m & ~shift(m, int(np.sign(dx)) * px, int(np.sign(dy)) * px)

class Canvas:
    def __init__(self):
        self.px = np.zeros((CELL, CELL, 4), np.uint8)

    def fill(self, mask, colour):
        self.px[mask] = colour

    def part(self, mask, base, width=2):
        """Outlined shape. Main shapes get Tiny Swords' 2px outline, small details 1px so they stay readable."""
        grown = dilate(mask)
        if width > 1:
            grown = dilate(grown)
        self.fill(grown & ~mask, OUTLINE)
        self.fill(mask, base)

def draw_bomb(c, cx, cy, r, lit, spark_frame):
    body = ellipse(cx, cy, r, r)
    c.part(body, BOMB)
    c.fill(edge(body, 1, -1, 2), BOMB_D)
    c.fill(body & ellipse(cx + r * 0.35, cy - r * 0.35, max(r * 0.3, 1), max(r * 0.3, 1)), BOMB_L)
    neck = rect(cx - 1, cy - r - 2, cx + 1, cy - r)
    c.part(neck, BOMB_L, 1)
    if r >= 5:
        fuse = rect(cx + 1, cy - r - 5, cx + 1, cy - r - 3) | rect(cx, cy - r - 5, cx, cy - r - 5)
        c.part(fuse, FUSE, 1)
        if lit:
            sx, sy = cx + (1 if spark_frame % 2 else 0), cy - r - 7
            star = rect(sx - 1, sy, sx + 1, sy) | rect(sx, sy - 1, sx, sy + 1)
            star |= rect(sx - 2, sy, sx + 2, sy) if spark_frame % 2 else rect(sx, sy - 2, sx, sy + 2)
            c.fill(star, SPARK)
            c.fill(rect(sx, sy, sx, sy), WHITE)

SKIN_DD = hexc('b08c64')

def curve(xc, r, depth):
    """Vertical offset of a band wrapped round a cylinder seen from above: deepest in the middle."""
    return depth * np.sqrt(np.clip(1 - ((XX - xc + 0.5) / r) ** 2, 0, 1))

def draw_hand(c, x, y):
    hand = ellipse(x, y, 4.5, 4.5)
    c.part(hand, SKIN)
    c.fill(edge(hand, 1, -1, 2), SKIN_D)
    c.fill(edge(hand, 1, -1, 1) & (YY > y), SKIN_DD)
    c.fill(hand & hline(y + 1) & (XX > x - 2) & (XX < x + 3), SKIN_D)

AX, AY = 96, 114  # body anchor (centre of the trunk)
HALF = 16         # half width of the trunk

def frame(p):
    c = Canvas()
    dy = p.get('dy', 0)
    lean = p.get('lean', 0)
    bx, by = AX + lean, AY + dy
    hx, hy = bx + 2 + p.get('head_dx', 0), by - 20 + p.get('head_dy', 0)   # face centre

    c.px[ellipse(AX, 131, 21, 6.5)] = SHADOW

    # legs: short dark stubs (foreshortened from above)
    for (fx, fy), lx in zip(p.get('feet', [(-6, 0), (6, 0)]), (bx - 6, bx + 6)):
        fx, fy = AX + fx, 130 + fy
        c.fill(thick_line(lx, by + 6, fx, fy, 4), OUTLINE)
        c.fill(rect(fx - 2, fy - 1, fx + 2, fy), OUTLINE)

    hb = p.get('hand_back', (-18, 0))
    hf = p.get('hand_front', (17, 0))
    bomb = p.get('bomb', 'held')

    def front_hand():
        x, y = bx + hf[0], by + hf[1]
        if bomb != 'none':
            draw_bomb(c, x + p.get('bomb_dx', 5), y - 3, 7, bomb == 'lit', p.get('spark', 0))
        draw_hand(c, x, y)

    draw_hand(c, bx + hb[0], by + hb[1])

    # trunk: slim squarish tunic, short because we look down on it
    sq = p.get('squash', 0)
    w = HALF + sq
    trunk = round_rect(bx - w, by - 10 + sq, bx + w, by + 7, 4, 2)
    trunk |= round_rect(bx - w - 1, by + 1, bx + w + 1, by + 8, 1, 2)
    c.part(trunk, BLUE)
    c.fill(edge(trunk, 1, 0, 4), BLUE_D)
    c.fill(edge(trunk, 0, -1, 2), BLUE_D)
    seam = hline(by + 4 + curve(bx, w + 1, 3)) & trunk & ~edge(trunk, 0, -1, 1)
    c.fill(seam, OUTLINE)
    for cx in (bx - 8, bx + 13):
        c.fill(vline(cx) & trunk & (YY > by + 5.5 + curve(bx, w + 1, 3)), BLUE_D)
    c.fill(vline(bx + 12) & trunk & (YY > by - 6) & (YY < by + 1), BLUE_D)

    # tabard: teal panel with a pointed hem, light trim and a bomb badge
    tx = bx + 3
    tab = round_rect(tx - 7, by - 10 + sq, tx + 7, by + 7, 1, 2)
    tab |= rect(tx - 4, by + 8, tx + 4, by + 9) | rect(tx - 2, by + 10, tx + 2, by + 11)
    c.part(tab, TEAL)
    c.fill(edge(tab, 1, 0, 3), TEAL_D)
    c.fill(tab & rect(tx + 5, by - 8, tx + 5, by + 6), HELM)
    c.fill(tab & rect(tx - 5, by - 8, tx - 5, by + 6), HELM_D)
    for sx in range(tx - 3, tx + 4, 2):
        c.fill(tab & pt(sx, by + 9), HELM)
    badge = ellipse(tx, by - 1, 3.5, 3.5)
    c.part(badge, BOMB_D, 1)
    c.fill(badge & rect(tx + 1, by - 3, tx + 1, by - 2), BOMB_L)
    c.fill(rect(tx + 1, by - 6, tx + 1, by - 5), OUTLINE)
    c.fill(rect(tx + 2, by - 7, tx + 2, by - 7), SPARK)

    # belt, curving round the body, with a buckle
    bc = by + 1 + curve(bx, w + 1, 2)
    belt = (YY >= bc) & (YY <= bc + 2) & trunk
    c.part(belt, BROWN, 1)
    c.fill(belt & (YY >= bc + 2), BROWN_D)
    for sx in (bx - 5, bx + 10, bx + 14):
        sy = by + 2 + 2 * np.sqrt(max(0, 1 - ((sx - bx + 0.5) / (w + 1)) ** 2))
        c.fill(belt & pt(sx, sy), SKIN_D)
    buckle = rect(tx - 2, by + 2, tx + 2, by + 6)
    c.part(buckle, SKIN_D, 1)
    c.fill(rect(tx - 1, by + 3, tx + 1, by + 5), BROWN_D)
    c.fill(rect(tx, by + 4, tx, by + 4), SKIN_D)

    # pouch on the hip
    pouch = round_rect(bx - 15, by + 1, bx - 8, by + 8, 1, 2)
    c.part(pouch, BROWN)
    c.fill(edge(pouch, 1, 0, 2), BROWN_D)
    c.fill(pouch & (YY <= by + 3), BROWN_D)
    c.fill(pouch & hline(by + 1), BROWN)
    c.fill(pouch & vline(bx - 12) & (YY < by + 4), BROWN)
    c.fill(rect(bx - 12, by + 4, bx - 12, by + 4), SKIN_D)

    # teal collar: a rounder ring, since we see it from above
    collar = ellipse(bx + 2, by - 10, 14, 4.5)
    c.part(collar, TEAL)
    c.fill(edge(collar, 0, -1, 2), TEAL_D)
    c.fill(edge(collar, 1, 0, 3), TEAL_D)
    for fx in (bx - 3, bx + 9):
        c.fill(collar & vline(fx) & (YY > by - 10), TEAL_D)
    c.fill(collar & hline(by - 13) & (XX > bx + 4) & (XX < bx + 13), HELM)

    # face: three skin tones, darkest right under the brim and on the shadow side
    # chainmail coif hanging under the helmet, framing the face like the archer's hood
    coif = ellipse(hx + 1, hy, 17, 11.5)
    c.part(coif, HELM_D)
    links = (_PY % 3 == 1) & (((_PX + (_PY // 3) * 2) % 4) < 2)
    c.fill(coif & links & ~edge(coif, 1, 0, 4), HELM)
    c.fill(coif & links & edge(coif, 1, 0, 4), HELM_D)
    c.fill(edge(coif, 1, 0, 4) & ~links, OUTLINE)
    face = ellipse(hx + 4, hy + 1, 12.5, 10)
    c.part(face, SKIN)
    brim_rx, brim_ry = 23, 8.5
    mx = hx + 3                                    # helmet sits a little right of the face centre
    cy = hy - 11                                   # brim centre
    brim_low = cy + curve(mx, brim_rx, brim_ry)
    c.fill(face & (YY < brim_low + 3), SKIN_D)
    c.fill(face & (YY < brim_low + 1), SKIN_DD)
    c.fill(edge(face, 1, 0, 3), SKIN_D)
    c.fill(edge(face, 1, 0, 1), SKIN_DD)
    c.fill(edge(face, 0, -1, 2) & (XX < hx + 2), SKIN_D)
    blink = p.get('blink', False)
    for ex in (hx + 3, hx + 11):
        c.fill(rect(ex, hy + 2, ex + 1, hy + 2) if blink else rect(ex, hy + 1, ex + 1, hy + 3), OUTLINE)
    stache = ellipse(hx + 4.5, hy + 7.5, 4, 1.8) | ellipse(hx + 11.5, hy + 7.5, 4, 1.8)
    c.part(stache, BROWN, 1)
    c.fill(edge(stache, 0, -1, 1), BROWN_D)
    nose = ellipse(hx + 8, hy + 5, 2.5, 2)
    c.part(nose, SKIN, 1)
    c.fill(edge(nose, 1, -1, 1), SKIN_D)
    c.fill(pt(hx + 8.5, hy + 4), WHITE)

    # kettle helmet seen from a bit above: round brim, low dome, riveted band curving round it
    brim = ellipse(mx, cy, brim_rx, brim_ry)
    c.part(brim, HELM)
    c.fill(edge(brim, 0, -1, 2), HELM_D)
    c.fill(edge(brim, 1, 0, 4), HELM_D)
    c.fill(brim & ~ellipse(mx, cy, brim_rx - 2, brim_ry - 1.5) & (XX > mx + 6) & (YY > cy), HELM_L)
    c.fill(brim & pt(mx + 18, cy + 5), WHITE)
    dr = 14
    base = cy + 2 + curve(mx, dr, 3)               # where the dome meets the brim
    dome = ellipse(mx, cy - 3, dr, 13) & (YY < base)
    c.part(dome, HELM)
    c.fill(edge(dome, 1, 0, 4), HELM_D)
    band = dome & (YY >= base - 4)
    c.fill(band, TEAL)
    c.fill(band & edge(dome, 1, 0, 4), TEAL_D)
    c.fill(dome & hline(base - 5), OUTLINE)
    for rx in (mx - 8, mx - 1, mx + 6, mx + 11):
        ry = int(round(cy + 2 + 3 * np.sqrt(max(0, 1 - ((rx - mx + 0.5) / dr) ** 2)) - 2))
        c.fill(pt(rx, ry), HELM_L)
        c.fill(pt(rx, ry + 1), TEAL_D)
    c.fill(dome & rect(mx + 2, cy - 15, mx + 2, cy - 6), HELM_D)
    c.fill(dome & rect(mx + 3, cy - 15, mx + 3, cy - 6), HELM_L)
    c.fill(dome & ellipse(mx + 8, cy - 9, 2.5, 3.5), HELM_L)
    c.fill(rect(mx + 8, cy - 11, mx + 8, cy - 10), WHITE)
    knob = ellipse(mx + 2, cy - 16, 2.5, 2)
    c.part(knob, HELM_D, 1)
    c.fill(rect(mx + 3, cy - 17, mx + 3, cy - 17), HELM_L)

    front_hand()

    if p.get('smear'):
        cx0, cy0 = hx + 6, hy - 6
        arc = ellipse(cx0, cy0, 30, 28) & ~ellipse(cx0, cy0, 28.5, 26.5) & (YY < by + hf[1] - 6) & (XX > hx + 4)
        arc &= ~dilate(brim | dome | knob)
        c.fill(arc, WHITE)

    return c.px

def sheet(poses):
    img = np.zeros((CELL, CELL * len(poses), 4), np.uint8)
    for i, p in enumerate(poses):
        img[:, i * CELL:(i + 1) * CELL] = frame(p)
    return Image.fromarray(img, 'RGBA')

idle = [
    dict(),
    dict(),
    dict(dy=1, squash=1),
    dict(dy=1, squash=1, head_dy=1),
    dict(dy=1, squash=1, head_dy=1, blink=True),
    dict(head_dy=1),
]

run = [
    dict(dy=0,  lean=1, feet=[(-10, -1), (9, -2)], hand_back=(-16, -2), hand_front=(15, 2)),
    dict(dy=-3, lean=1, feet=[(-4, -4), (5, -1)],  hand_back=(-18, 0),  hand_front=(18, -1)),
    dict(dy=0,  lean=1, feet=[(-2, -1), (6, -4)],  hand_back=(-20, 2),  hand_front=(20, -3)),
    dict(dy=-3, lean=1, feet=[(-7, -1), (7, -4)],  hand_back=(-18, 0),  hand_front=(18, -1)),
]

# ShotFired fires on frame 5 (0.6s), so the bomb leaves the hand there
shoot = [
    dict(hand_front=(17, 0), bomb='held'),
    dict(hand_front=(13, -8), bomb='lit', spark=0, head_dx=-1),
    dict(hand_front=(-21, -12), bomb='lit', spark=1, bomb_dx=-5, lean=-1, head_dx=-1, hand_back=(-16, 4)),
    dict(hand_front=(-23, -28), bomb='lit', spark=0, bomb_dx=-5, lean=-2, head_dx=-1, squash=1, hand_back=(-14, 6)),
    dict(hand_front=(7, -44), bomb='lit', spark=1, hand_back=(-17, 2)),
    dict(hand_front=(28, -20), bomb='none', smear=True, lean=2, head_dx=1, squash=-1, hand_back=(-20, -2)),
    dict(hand_front=(24, -5), bomb='none', lean=1, head_dx=1, hand_back=(-19, 0)),
    dict(hand_front=(18, 0), bomb='held'),
]

sheets = {'Idle': sheet(idle), 'Run': sheet(run), 'Shoot': sheet(shoot)}
for name, img in sheets.items():
    img.save(f"{OUT}/Bombardier_{name}.png")

# preview: one row per animation, the archer first for scale
archer = Image.open(REFERENCE) if Path(REFERENCE).exists() else None
Path(PREVIEW).parent.mkdir(parents=True, exist_ok=True)
rows = []
for name, img in sheets.items():
    n = img.width // CELL
    row = Image.new('RGBA', ((n + 1) * 110, 110), (60, 140, 90, 255))
    if archer:
        row.alpha_composite(archer.crop((40, 40, 150, 150)), (0, 0))
    for i in range(n):
        row.alpha_composite(img.crop((i * CELL + 41, 40, i * CELL + 151, 150)), ((i + 1) * 110, 0))
    rows.append(row)
w = max(r.width for r in rows)
prev = Image.new('RGBA', (w, 110 * len(rows)), (60, 140, 90, 255))
for i, r in enumerate(rows):
    prev.alpha_composite(r, (0, i * 110))
prev.resize((prev.width * 2, prev.height * 2), Image.NEAREST).save(PREVIEW)
