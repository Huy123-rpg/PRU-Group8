import re

def get_slice_names(meta_path, file_ids):
    with open(meta_path, 'r', encoding='utf-8') as f:
        c = f.read()
    results = {}
    for fid in file_ids:
        m = re.search(r'213:\s*' + str(fid) + r'\s*\n\s*second:\s*\"?([^\n\"]+)', c)
        if m:
            results[fid] = m.group(1)
        else:
            m2 = re.search(r'internalID:\s*' + str(fid) + r'[\s\S]*?name:\s*([^\n]+)', c)
            if m2:
                results[fid] = m2.group(1)
            else:
                results[fid] = 'unknown'
    return results

ids_19 = [3269597246451970579, 214771166639925210, -6600993594115511537, -6956394949181413952]
print('19 sprite slices in Atlas chu:', get_slice_names('Assets/Art/Environment/Atlas chữ kết quả Aurelia Academy.png.meta', ids_19))

ids_19_1 = [-2561520835330777431, -6578510690912469364, 6553294552936595461]
print('19.1 sprite slices in Atlas chu:', get_slice_names('Assets/Art/Environment/Atlas chữ kết quả Aurelia Academy.png.meta', ids_19_1))
