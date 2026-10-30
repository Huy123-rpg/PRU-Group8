with open('scratch/all_64_dump_utf8.txt', 'r', encoding='utf-8') as f:
    text = f.read()

# Print items 1 to 8 (Bai 2, 3)
blocks = text.split('-' * 60)
for b in blocks[:8]:
    print(b.strip())
    print('=' * 40)
