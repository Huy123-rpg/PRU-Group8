import urllib.request
import re
import sys
sys.stdout.reconfigure(encoding='utf-8')

# The spreadsheet HTML can reveal all sheet tabs!
sheet_id = '1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY'
url = f'https://docs.google.com/spreadsheets/d/{sheet_id}/htmlview'

try:
    req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
    with urllib.request.urlopen(req, timeout=15) as resp:
        html = resp.read().decode('utf-8')
        # find tab names
        tabs = re.findall(r'<li id="sheet-button-(\d+)"[^>]*><a[^>]*>([^<]+)</a>', html)
        print("Found tabs via htmlview:")
        for gid, name in tabs:
            print(f"  gid={gid}: '{name}'")
except Exception as e:
    print("Error querying htmlview:", e)
