with open('scratch/all_64_dump_utf8.txt', 'r', encoding='utf-8') as f:
    text = f.read()

blocks = text.split('-' * 60)
# print items 9 to 24 (Bai 4 to Bai 7)
for b in blocks[8:24]:
    print(b.strip())
    print('=' * 40)
