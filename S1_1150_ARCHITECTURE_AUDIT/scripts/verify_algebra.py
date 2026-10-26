"""Exhaustive finite probability calculation, not stochastic data generation."""
from itertools import product
from math import prod,exp,isclose
import json
from pathlib import Path
checks=[]
for m in [0,.02,.025,.03,.04,.08,.18,.5,1]:
    p=joint=allmissing=total=0.
    for bits in product([0,1],repeat=7):
        prob=prod(m if b else 1-m for b in bits);mask=[bits[0] or b for b in bits[1:]];total+=prob;p+=prob*mask[0];joint+=prob*(mask[0] and mask[1]);allmissing+=prob*all(mask)
    expectedp=2*m-m*m
    checks.append(dict(m=m,total=isclose(total,1,abs_tol=1e-12),marginal=isclose(p,expectedp,abs_tol=1e-12),covariance=isclose(joint-p*p,m*(1-m)**3,abs_tol=1e-12),all_missing=isclose(allmissing,m+(1-m)*m**6,abs_tol=1e-12)))
sigmoid=lambda z:1/(1+exp(-z));signchecks=[]
for z,q,gamma,omega,u in product([-3,0,3],[0,.55,1],[0,.4,2],[0,1],[0,.3,1]):
    delta=sigmoid(z-omega*u)-sigmoid(z-gamma*(1-q)-omega*u);assert delta>=-1e-14;signchecks.append(delta)
result=dict(probability_checks=checks,sign_grid_cases=len(signchecks),minimum_delta=min(signchecks),all_pass=all(all(v for k,v in r.items() if k!='m') for r in checks),interpretation='Finite enumeration verifies the stated identities at selected parameters; algebra in the manuscript supplies their general derivation. No simulated observations are produced.')
assert result['all_pass']
p=Path(__file__).resolve().parents[1]/'qa/ALGEBRA_CHECKS.json';p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(result,indent=2),encoding='utf-8');print(json.dumps(result))
