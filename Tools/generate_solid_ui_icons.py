"""Original filled SVG icons, with matching supersampled Unity UI textures. No strokes."""
from pathlib import Path
from math import sin,cos,radians
from PIL import Image,ImageDraw
root=Path(__file__).resolve().parents[1]
source=root/'Tools/UIIcons';target=root/'Assets/_Project/Resources/UIIcons'
source.mkdir(exist_ok=True);target.mkdir(exist_ok=True)
def ring(cx,cy,rx,ry,start,end,thickness):
 outer=[(cx+rx*cos(radians(a)),cy+ry*sin(radians(a))) for a in range(start,end+1,3)]
 inner=[(cx+(rx-thickness)*cos(radians(a)),cy+(ry-thickness)*sin(radians(a))) for a in range(end,start-1,-3)]
 return outer+inner
icons=[
 ('move',[(1,[(48,5),(63,22),(54,22),(54,42),(74,42),(74,33),(91,48),(74,63),(74,54),(54,54),(54,74),(63,74),(48,91),(33,74),(42,74),(42,54),(22,54),(22,63),(5,48),(22,33),(22,42),(42,42),(42,22),(33,22)])]),
 ('depth',[(.38,[(13,25),(44,8),(75,25),(44,43)]),(1,[(13,53),(44,36),(75,53),(44,71)]),(.65,[(13,65),(44,82),(75,65),(75,77),(44,94),(13,77)]),(1,[(78,6),(90,20),(84,20),(84,43),(73,43),(73,20),(66,20)])]),
 ('rotate',[(.45,[(29,37),(48,26),(67,37),(48,48)]),(.75,[(29,41),(44,50),(44,72),(29,63)]),(1,[(52,50),(67,41),(67,63),(52,72)]),(1,ring(48,48,43,31,20,290,10)),(1,[(68,6),(92,16),(76,34)])]),
 ('twist',[(.72,[(38,29),(67,40),(56,69),(27,58)]),(1,ring(48,48,43,43,5,275,11)),(1,[(39,2),(62,8),(43,26)])])]
for name,shapes in icons:
 svg=['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 96 96">']
 image=Image.new('RGBA',(768,768));draw=ImageDraw.Draw(image)
 for opacity,points in shapes:
  svg.append('<polygon fill="white" fill-opacity="%s" points="%s"/>'%(opacity,' '.join(f'{x:.3f},{y:.3f}' for x,y in points)))
  draw.polygon([(round(x*8),round(y*8)) for x,y in points],fill=(255,255,255,round(opacity*255)))
 svg.append('</svg>')
 (source/f'{name}.svg').write_text('\n'.join(svg)+'\n',encoding='utf-8')
 image.resize((256,256),Image.Resampling.LANCZOS).save(target/f'{name}.png')
print('Generated four filled SVG sources and matching transparent 256px PNGs.')
