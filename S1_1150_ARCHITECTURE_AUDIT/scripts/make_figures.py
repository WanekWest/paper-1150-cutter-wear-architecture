from pathlib import Path
import csv,json
from reportlab.pdfgen import canvas
from reportlab.lib.colors import HexColor,black,white
import pypdfium2 as pdfium
import reportlab
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
pdfmetrics.registerFont(TTFont('Helvetica',str(Path(reportlab.__file__).parent/'fonts/Vera.ttf')))
W=Path(__file__).resolve().parents[1];F=W/'working/figures';F.mkdir(parents=True,exist_ok=True)
rows=list(csv.DictReader((W/'qa/scientific_audit/scenario_accounting.csv').open()))
blue=HexColor('#205C85');orange=HexColor('#A95420');gray=HexColor('#666666');light=HexColor('#E6EDF2')
labels=['Normal','Accelerated','Chipping','False alarm','Degradation','Mixed','Missing data','Replacement']
def base(n,h):
    c=canvas.Canvas(str(F/f'1150_Figure_{n}.pdf'),pagesize=(468,h));c.setTitle(f'1150 Figure {n}');c.setAuthor('Brovchenko, Mazakov and Kiba');c.setFont('Helvetica',9);return c
def txt(c,x,y,t,size=9,color=black):c.setFillColor(color);c.setFont('Helvetica',size);c.drawString(x,y,t)
def box(c,x,y,w,h,title,lines):
    c.setStrokeColor(blue);c.setFillColor(light);c.roundRect(x,y,w,h,5,fill=1);txt(c,x+9,y+h-17,title,10,blue)
    for i,l in enumerate(lines):txt(c,x+9,y+h-33-12*i,l,8.7)
def arrow(c,x,y,x2,y2):
    c.setStrokeColor(gray);c.line(x,y,x2,y2)
    if y==y2:c.line(x2,y2,x2-5,y2+3);c.line(x2,y2,x2-5,y2-3)
    else:c.line(x2,y2,x2-3,y2+5);c.line(x2,y2,x2+3,y2+5)
c=base(1,290)
box(c,12,185,137,74,'Observation records',['Values / missing markers','Channels, epochs and keys','Quality and configuration'])
box(c,166,185,137,74,'Record audit',['Key and mask predicates','Availability denominators','Source / formula checks'])
box(c,320,185,137,74,'Bounded output',['Value and validity separate','Quality descriptors','Evidence references'])
arrow(c,149,223,166,223);arrow(c,303,223,320,223)
box(c,12,65,211,80,'Computational provenance',['Retained inputs -> audit -> derived results','SHA-256 identity and executable workflow','Does not establish physical calibration'])
box(c,246,65,211,80,'Metrological evidence',['Measurand, reference and calibration','Measurement model and uncertainty','Not established for this synthetic case'])
arrow(c,115,185,115,145);arrow(c,350,185,350,145)
txt(c,12,28,'Separate evidence objects; no calibrated wear measurement is inferred.',9)
c.save()
c=base(2,260)
txt(c,12,240,'Channel availability and mean quality',11,blue)
left=102;right=441;bottom=32;top=218
c.setStrokeColor(gray)
for i in range(6):
    v=i/5;x=left+(right-left)*v;c.setStrokeColor(HexColor('#CCCCCC'));c.line(x,bottom,x,top);txt(c,x-5,bottom-15,f'{v:.1f}',8)
for i,(r,label) in enumerate(zip(rows,labels)):
    y=top-12-i*23;txt(c,8,y-3,label,9)
    a=float(r['channel_availability']);q=float(r['mean_quality_including_missing']);c.setStrokeColor(gray);c.line(left+q*(right-left),y,left+a*(right-left),y)
    c.setFillColor(blue);c.circle(left+a*(right-left),y,3.4,fill=1,stroke=0)
    c.setFillColor(orange);c.rect(left+q*(right-left)-3,y-3,6,6,fill=1,stroke=0)
txt(c,153,3,'Circle: availability     Square: mean quality',9)
c.save()
c=base(3,460)
txt(c,12,440,'(a) Marginal missingness: configured, derived and observed',11,blue)
for panel in [0,1]:
    bot=270 if panel==0 else 50;top=415 if panel==0 else 205;left=40;right=455;maxv=.36 if panel==0 else .20
    for j in range(5):
        v=maxv*j/4;y=bot+(top-bot)*v/maxv;c.setStrokeColor(HexColor('#CCCCCC'));c.line(left,y,right,y);txt(c,7,y-3,f'{v:.2f}',8)
    for i,r in enumerate(rows):
        x=left+22+i*51
        if panel==0:
            vals=[float(r['configured_drop_probability']),float(r['expected_channel_missing_fraction']),float(r['observed_missing_fraction'])]
            for k,(val,color) in enumerate(zip(vals,[gray,blue,orange])):
                c.setFillColor(color);c.rect(x-12+k*8,bot,6,(top-bot)*val/maxv,stroke=0,fill=1)
        else:
            vals=[int(r['all_channels_missing_epochs'])/int(r['epochs']),float(r['expected_all_missing_probability']),float(r['independent_mask_all_missing_probability'])]
            for k,(val,color) in enumerate(zip(vals,[orange,blue,gray])):
                xx=x-7+k*7;yy=bot+(top-bot)*val/maxv;c.setFillColor(color)
                if k==0:c.circle(xx,yy,3.2,fill=1,stroke=0)
                elif k==1:c.rect(xx-2.5,yy-2.5,5,5,fill=1,stroke=0)
                else:c.setStrokeColor(color);c.line(xx-3,yy,xx+3,yy);c.line(xx,yy-3,xx,yy+3)
        txt(c,x-4,bot-13,str(i+1),8)
txt(c,42,243,'Gray: stage m    Blue: 2m - m^2    Orange: observed',8.5)
txt(c,12,225,'(b) Complete six-channel outage: shared versus independent masks',10,blue)
txt(c,42,15,'Circle: observed    Square: shared-drop theory    Cross: independent',8.5)
c.save()
for n in [1,2,3]:
    pdf=pdfium.PdfDocument(str(F/f'1150_Figure_{n}.pdf'));im=pdf[0].render(scale=300/72).to_pil();im.save(F/f'1150_Figure_{n}.png',dpi=(300,300));pdf.close()
(F/'FIGURE_SOURCES.json').write_text(json.dumps({'1':{'source':'manuscript_content.py evidence contract and audit_retained_data.py implemented predicates','type':'conceptual diagram'},'2':{'source':'qa/scientific_audit/scenario_accounting.csv','columns':['channel_availability','mean_quality_including_missing']},'3':{'source':'qa/scientific_audit/scenario_accounting.csv','scenario_numbers':dict(enumerate(labels,1))}},indent=2),encoding='utf-8')
print('Three vector PDFs and three 300-dpi PNGs generated.')
