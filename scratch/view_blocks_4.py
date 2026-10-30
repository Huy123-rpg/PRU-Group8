with open('scratch/all_64_dump_utf8.txt', 'r', encoding='utf-8') as f:
    text = f.read()

blocks = text.split('-' * 60)
# print items 41 to 64
for b in blocks[40:64]:
    print(b.strip())
    print('=' * 40)
