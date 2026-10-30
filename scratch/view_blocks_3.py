with open('scratch/all_64_dump_utf8.txt', 'r', encoding='utf-8') as f:
    text = f.read()

blocks = text.split('-' * 60)
# print items 25 to 40 (Bai 8 to Bai 11)
for b in blocks[24:40]:
    print(b.strip())
    print('=' * 40)
