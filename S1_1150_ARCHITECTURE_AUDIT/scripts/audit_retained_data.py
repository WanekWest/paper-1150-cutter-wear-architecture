"""Deterministic audit of retained 1150 records; no simulation or refitting.
Portable Python 3.10+, standard library only. Input root must contain data/ and src/.
"""
from pathlib import Path
from collections import Counter,defaultdict
import argparse,csv,hashlib,json,math,statistics

def read(p):
    with p.open(encoding='utf-8-sig',newline='') as f:yield from csv.DictReader(f)
def save(p,rows):
    p.parent.mkdir(parents=True,exist_ok=True)
    with p.open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
def main():
    ap=argparse.ArgumentParser();ap.add_argument('--root',type=Path,required=True);ap.add_argument('--out',type=Path,required=True);a=ap.parse_args();a.out.mkdir(parents=True,exist_ok=True)
    data=a.root/'data/synthetic';cfg=json.loads((data/'configs/experiment_config.json').read_text(encoding='utf-8-sig'));sensors=json.loads((data/'configs/sensor_config.json').read_text(encoding='utf-8-sig'))['SensorChannels'];channels={s['ChannelId'] for s in sensors}
    scenarios={s['ScenarioId']:s for s in cfg['Scenarios']};latent={};units=defaultdict(set);counts=Counter();errors=Counter();hashes=set()
    for r in read(data/'generated/latent_state.csv'):
        k=(r['scenario_id'],r['unit_id'],r['step'])
        if k in latent:errors['duplicate_latent_key']+=1
        latent[k]=r;units[k[0]].add(k[1]);counts[k[0]]+=1;hashes.add(r['config_hash'])
    events={}
    for r in read(data/'generated/events_and_labels.csv'):
        k=(r['scenario_id'],r['unit_id'],r['step'])
        if k in events:errors['duplicate_event_key']+=1
        events[k]=r
    obskeys=set();eventmasks=defaultdict(set);stats=defaultdict(lambda:dict(n=0,missing=0,qsum=0.,available_qsum=0.));features=defaultdict(list)
    for r in read(data/'generated/observations.csv'):
        k=(r['scenario_id'],r['unit_id'],r['step']);ch=r['channel'];kk=(*k,ch)
        if kk in obskeys:errors['duplicate_observation_key']+=1
        obskeys.add(kk);eventmasks[k].add(ch)
        if k not in latent:errors['orphan_observation_key']+=1
        elif r['config_hash']!=latent[k]['config_hash']:errors['observation_config_disagreement']+=1
        if ch not in channels:errors['unknown_channel']+=1
        missing=r['missing_flag'].lower()=='true';q=float(r['quality_score']);finite=bool(r['feature_value']) and math.isfinite(float(r['feature_value']))
        if missing==finite:errors['missing_value_flag_disagreement']+=1
        if missing and (q!=0 or r['quality_flag']!='missing'):errors['missing_quality_disagreement']+=1
        if not math.isfinite(q) or not 0<=q<=1:errors['quality_out_of_range']+=1
        s=stats[(k[0],ch)];s['n']+=1;s['missing']+=missing;s['qsum']+=q;s['available_qsum']+=q if not missing else 0
        hashes.add(r['config_hash'])
        if missing:eventmasks[k].add('MISSING_'+ch)
    for k in latent:
        if not channels.issubset(eventmasks[k]):errors['incomplete_channel_schema']+=1
        if k not in events:errors['missing_event_key']+=1
    for k in events:
        if k not in latent:errors['orphan_event_key']+=1
    sourcepath=a.root/'src/WearVision.SyntheticStand.Core/SyntheticExperimentGenerator.cs';code=sourcepath.read_text(encoding='utf-8-sig')
    sourcechecks={
        'two_stage_missingness': 'sensorDrop || rng.NextDouble() < scenario.MissingnessRate' in code,
        'quality_is_zero_when_missing': 'var quality = missing ? 0.0' in code,
        'availability_named_traceability': '1.0 - x.MissingRate' in code and 'traceability_completeness' in code,
        'latent_wear_used_in_observed_proxy': 'var observedProxy = Clamp(wear + rng.Normal' in code,
        'six_channel_generation_not_deployable_sensor_fit': 'channel.WearSensitivity * wear' in code,
    }
    accounting=[];channelrows=[]
    for sc,c in scenarios.items():
        rr=[v for (s,ch),v in stats.items() if s==sc];n=sum(v['n'] for v in rr);mis=sum(v['missing'] for v in rr);m=c['MissingnessRate'];ks=[k for k in latent if k[0]==sc];allmissing=sum(all('MISSING_'+ch in eventmasks[k] for ch in channels) for k in ks)
        paircov=[]
        ordered=sorted(channels)
        for i,c1 in enumerate(ordered):
            for c2 in ordered[i+1:]:
                p1=stats[(sc,c1)]['missing']/len(ks);p2=stats[(sc,c2)]['missing']/len(ks)
                both=sum('MISSING_'+c1 in eventmasks[k] and 'MISSING_'+c2 in eventmasks[k] for k in ks)/len(ks)
                paircov.append(both-p1*p2)
        accounting.append(dict(scenario=sc,units=len(units[sc]),epochs=counts[sc],channel_records=n,missing_records=mis,observed_missing_fraction=mis/n,configured_drop_probability=m,expected_channel_missing_fraction=2*m-m*m,all_channels_missing_epochs=allmissing,expected_all_missing_probability=m+(1-m)*m**6,independent_mask_all_missing_probability=(2*m-m*m)**6,observed_mean_pair_covariance=statistics.mean(paircov),expected_pair_covariance=m*(1-m)**3,mean_quality_including_missing=sum(v['qsum'] for v in rr)/n,channel_availability=1-mis/n))
        for (s,ch),v in stats.items():
            if s==sc:channelrows.append(dict(scenario=sc,channel=ch,n=v['n'],missing=v['missing'],availability=1-v['missing']/v['n'],mean_quality=v['qsum']/v['n']))
    save(a.out/'scenario_accounting.csv',accounting);save(a.out/'channel_accounting.csv',channelrows)
    kpi={r['scenario_id']:r for r in read(data/'generated/paper01/kpi_summary.csv')};kpi_checks=[]
    for r in accounting:
        old=kpi[r['scenario']];d=abs(r['channel_availability']-float(old['traceability_completeness']));q=abs(r['mean_quality_including_missing']-float(old['mean_data_quality']))
        kpi_checks.append(dict(scenario=r['scenario'],availability_matches_archived_traceability_metric=d<1e-12,quality_matches_archived_quality=q<1e-12,absolute_availability_error=d,absolute_quality_error=q))
    save(a.out/'archived_kpi_reconciliation.csv',kpi_checks)
    ext=data/'reviewer_repair_extended/paper01_measurement_tests';aggregates=[]
    specifications=[('multi_seed_data_quality_sensitivity.csv','quality_level',['traceable_score','uncertainty_proxy']),('channel_ablation_metrics.csv','channel_removed',['traceable_score_delta']),('bias_drift_noise_missingness_sensitivity.csv','missingness',['diagnostic_rmse_proxy']),('baseline_without_quality_layer.csv','baseline',['delta_vs_full']),('baseline_without_event_layer.csv','baseline',['delta_vs_full']),('traceability_stress_test.csv',None,['traceability_pass_rate'])]
    inventory=[]
    for fn,key,columns in specifications:
        rr=list(read(ext/fn));inventory.append(dict(file=fn,rows=len(rr),columns=';'.join(rr[0])))
        key=key if key in rr[0] else None
        groups=defaultdict(list)
        for r in rr:groups[r[key] if key else 'all'].append(r)
        for group,values in groups.items():
            for col in columns:
                if col not in rr[0]:continue
                vv=[float(x[col]) for x in values];aggregates.append(dict(file=fn,group=group,metric=col,n=len(vv),mean=statistics.mean(vv),sample_sd=statistics.stdev(vv) if len(vv)>1 else 0,min=min(vv),max=max(vv)))
    save(a.out/'retained_summaries_recomputed.csv',aggregates);save(a.out/'retained_extension_inventory.csv',inventory)
    quality=list(read(ext/'baseline_without_quality_layer.csv'));negative=sum(float(r['delta_vs_full'])<0 for r in quality)
    summary=dict(scope='Deterministic reanalysis of retained records; no new synthetic generation, model fit or LockedTest/T6 access.',latent_epochs=len(latent),component_units=sum(map(len,units.values())),observation_records=len(obskeys),event_records=len(events),scenarios=len(scenarios),channels=len(channels),unique_config_hashes=sorted(hashes),structural_errors=dict(errors),source_checks=sourcechecks,quality_ablation_negative_deltas=negative,quality_ablation_records=len(quality),quality_layer_sign_boundary='Under frozen features/events/uncertainty, removing a nonnegative -gamma(1-Q) penalty cannot lower a sigmoid score. Archived negative deltas do not verify this conditional comparison.',extension_limit='Retained numeric summaries can be aggregated, but the original extension generator and row-level predictions/covariances are not established by these six summary files.',kpi_reconciliation_all=all(r['availability_matches_archived_traceability_metric'] and r['quality_matches_archived_quality'] for r in kpi_checks))
    (a.out/'audit_summary.json').write_text(json.dumps(summary,indent=2),encoding='utf-8');print(json.dumps(summary),flush=True)
if __name__=='__main__':main()
