"""Original deck and tabletop artwork. Requires Pillow and NumPy; Unity uses the baked PNGs."""
from pathlib import Path
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFont

out=Path(__file__).resolve().parents[1]/'Assets/_Project/Resources/Art'
out.mkdir(parents=True,exist_ok=True)
random=np.random.default_rng(731)
W,H=1024,1430
font_path=Path('C:/Windows/Fonts/georgiab.ttf')
if not font_path.exists(): raise SystemExit('Install Georgia Bold or update the font path before regenerating.')
font=lambda size:ImageFont.truetype(str(font_path),size)

def paper():
    noise=random.normal(0,.65,(H,W,1))
    return Image.fromarray(np.clip(np.array([247,244,231])+noise,0,255).astype('uint8'),'RGB')

def pip(draw,cx,cy,size,suit,color,flip=False):
    points=[]
    for i in range(160):
        t=i*math.pi*2/160
        x=16*math.sin(t)**3
        y=-(13*math.cos(t)-5*math.cos(2*t)-2*math.cos(3*t)-math.cos(4*t))
        if suit=='spade':y=-y
        if flip:x,y=-x,-y
        points.append((cx+x*size/34,cy+y*size/34))
    draw.polygon(points,fill=color)
    if suit=='spade':
        direction=-1 if flip else 1
        draw.polygon([(cx-size*.09,cy+direction*size*.10),(cx+size*.09,cy+direction*size*.10),(cx+size*.17,cy+direction*size*.47),(cx-size*.17,cy+direction*size*.47)],fill=color)

def index(rank,suit,color):
    layer=Image.new('RGBA',(170,270))
    d=ImageDraw.Draw(layer)
    d.text((85,2),rank,font=font(112),anchor='mt',fill=color)
    pip(d,85,183,82,suit,color)
    return layer

for rank,suit,color,name in [('A','spade','#18202a','ace-spades'),('7','heart','#b72335','seven-hearts')]:
    im=paper();d=ImageDraw.Draw(im)
    d.rounded_rectangle((25,25,W-26,H-26),radius=37,outline='#ded8c5',width=2)
    corner=index(rank,suit,color)
    im.paste(corner,(31,34),corner)
    rotated=corner.rotate(180)
    im.paste(rotated,(W-201,H-304),rotated)
    if rank=='A':
        pip(d,W/2,H*.49,405,suit,color)
        d.text((W/2,H*.72),'H A T D O N',font=font(34),anchor='mt',fill=color)
        d.text((W/2,H*.76),'PLAYING CARDS',font=font(20),anchor='mt',fill='#817b6a')
        d.line((W*.37,H*.70,W*.63,H*.70),fill='#b7a47a',width=3)
    else:
        for x in [.30,.70]:
            for y in [.25,.50,.75]:pip(d,W*x,H*y,142,suit,color,flip=y>.5)
        pip(d,W*.5,H*.375,142,suit,color)
    im.save(out/(name+'.png'),optimize=True)

im=paper();d=ImageDraw.Draw(im)
d.rounded_rectangle((38,38,W-39,H-39),radius=35,fill='#172e45')
for inset in [59,72,94]:d.rounded_rectangle((inset,inset,W-inset-1,H-inset-1),radius=28,outline='#b8aa7c',width=3 if inset==72 else 2)
for y in range(115,H-115,44):
    for x in range(115,W-115,44):
        d.line([(x,y-16),(x+16,y),(x,y+16),(x-16,y),(x,y-16)],fill='#657b87',width=2)
        d.ellipse((x-2,y-2,x+2,y+2),fill='#b8aa7c')
d.rounded_rectangle((W*.27,H*.31,W*.73,H*.69),radius=90,fill='#172e45',outline='#c8b789',width=5)
for cy in [H*.42,H*.58]:
    pip(d,W*.5,cy,125,'spade','#c8b789',flip=cy>H*.5)
d.line((W*.35,H*.5,W*.65,H*.5),fill='#c8b789',width=3)
im.save(out/'deck-back.png',optimize=True)

n=1024;y,x=np.mgrid[0:n,0:n]
grain=np.sin(y*.13+np.sin(x*.009)*3)+.55*np.sin(y*.65+np.sin(x*.015)*5)+.25*np.sin(y*2.7+x*.03)
grain+=random.normal(0,.6,(n,n))
wood=np.clip(np.array([91,53,32])+grain[:,:,None]*np.array([5.2,3.3,2.1]),0,255).astype('uint8')
Image.fromarray(wood).save(out/'walnut-grain.png',optimize=True)
fibres=random.normal(0,1.8,(n,n))+.45*np.sin(x*math.pi)+.45*np.sin(y*math.pi)
felt=np.clip(np.array([25,74,66])+fibres[:,:,None],0,255).astype('uint8')
Image.fromarray(felt).save(out/'green-felt.png',optimize=True)
print('Generated five original deck/table textures.')
