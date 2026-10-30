with open('Assets/Scripts/PhysicsGame2/Data/GameDataRepository.cs', 'r', encoding='utf-8') as f:
    c = f.read()

old_ds = '''                DataSource = LastSyncDataSource,'''
new_ds = '''                DataSource = (isEssay && IsDemoSupplementalEnabled) ? $"{LastSyncDataSource}+LocalDemoSupplemental" : LastSyncDataSource,'''

c_norm = c.replace('\r\n', '\n')
if old_ds in c_norm:
    res = c_norm.replace(old_ds, new_ds)
    with open('Assets/Scripts/PhysicsGame2/Data/GameDataRepository.cs', 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(res)
    print('SUCCESS')
else:
    print('TARGET NOT FOUND')
