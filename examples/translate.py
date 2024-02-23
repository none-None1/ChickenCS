"""
Usage:
python translate.py 0 chicken.chn minichicken.mch # Translate from Chicken to MiniChicken
python translate.py 1 minichicken.mch chicken.chn # Translate from MiniChicken to Chicken
"""
def chicken_to_minichicken(code: str) -> str:
    res = []
    code = code.lower()
    for l in code.split("\n"):
        res.append(str(l.count("chicken")))
    return " ".join(res)
def minichicken_to_chicken(code: str) -> str:
    res = []
    for n in code.split():
        res.append(" ".join("chicken" for _ in range(int(n))))
    return "\n".join(res)
import sys
_,mode,infn,outfn=sys.argv
code=open(infn).read()
if mode=='0':
    code=chicken_to_minichicken(code)
else:
    code=minichicken_to_chicken(code)
open(outfn,'w').write(code)