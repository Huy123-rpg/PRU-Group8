import urllib.request
import re
import sys
sys.stdout.reconfigure(encoding='utf-8')

sheet_id = '1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY'
url = f'https://docs.google.com/spreadsheets/d/{sheet_id}/edit?usp=sharing'

try:
    req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
    with urllib.request.urlopen(req, timeout=15) as resp:
        html = resp.read().decode('utf-8')
        print(f"HTML length: {len(html)}")
        # Look for sheet names in bootstrap data
        # Often in window.bootstrapData or "name":"..."
        matches = re.findall(r'"name":"([^"]+)"[^{}]*?"sheetId":(\d+)', html)
        print("Found sheets:")
        for name, sid in matches:
            print(f"  sheetId: {sid}, name: {name}")
        
        # Also look for any other mentions of NganHang or tabs
        tabs = re.findall(r'\[\d+,\d+,"([^"]+)",\d+\]', html)
        if tabs:
            print("Tabs from array:", set(tabs))
except Exception as e:
    print("Error:", e)
