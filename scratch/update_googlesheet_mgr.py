import sys
sys.stdout.reconfigure(encoding='utf-8')

path = 'Assets/Scripts/Core/GoogleSheetDataManager.cs'
with open(path, 'r', encoding='utf-8') as f:
    text = f.read()

target1 = """    [Header("Physics Game 2 - Live Questions URL")]
    [Tooltip("Link CSV Sheet Ngân Hàng Câu Hỏi Vật Lý Game 2")]
    public string physicsQuestionsCsvUrl = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/gviz/tq?tqx=out:csv&sheet=NganHangCauHoi";"""

replacement1 = """    [Header("Physics Game 2 - Live Questions URL")]
    [Tooltip("Link CSV Sheet Ngân Hàng Câu Hỏi Trắc Nghiệm Game 2")]
    public string physicsQuestionsCsvUrl = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/gviz/tq?tqx=out:csv&sheet=NganHangCauHoi";

    [Tooltip("Link CSV Sheet Ngân Hàng Câu Hỏi Tự Luận Game 2")]
    public string physicsEssayCsvUrl = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/gviz/tq?tqx=out:csv&sheet=Cauhoituluan";"""

target2 = """    public IEnumerator FetchLivePhysicsQuestionsRoutine(Action<int> onComplete = null)
    {
        if (string.IsNullOrEmpty(physicsQuestionsCsvUrl))
        {
            onComplete?.Invoke(0);
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequest.Get(physicsQuestionsCsvUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    int count = ScienceQuest.PhysicsGame2.GameDataRepository.LoadOrUpdateMCQsFromCsv(www.downloadHandler.text);
                    Debug.Log($"<color=green>[GoogleSheet] Đã đồng bộ Live {count} câu hỏi trắc nghiệm từ Google Sheets!</color>");
                    onComplete?.Invoke(count);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[GoogleSheet] Lỗi phân tích Live CSV, dùng local cache: " + ex.Message);
                    onComplete?.Invoke(0);
                }
            }
            else
            {
                Debug.Log($"[GoogleSheet] Không tải được Live Sheet ({www.error}), tiếp tục dùng ngân hàng local.");
                onComplete?.Invoke(0);
            }
        }
    }"""

replacement2 = """    public IEnumerator FetchLivePhysicsQuestionsRoutine(Action<int> onComplete = null)
    {
        yield return FetchLivePhysicsQuestionsRoutine((success, count, err) =>
        {
            onComplete?.Invoke(count);
        });
    }

    public IEnumerator FetchLivePhysicsQuestionsRoutine(Action<bool, int, string> onComplete)
    {
        if (string.IsNullOrEmpty(physicsQuestionsCsvUrl))
        {
            onComplete?.Invoke(false, 0, "URL trống");
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequest.Get(physicsQuestionsCsvUrl))
        {
            www.timeout = 12;
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    int count = ScienceQuest.PhysicsGame2.GameDataRepository.LoadOrUpdateMCQsFromCsv(www.downloadHandler.text);
                    Debug.Log($"<color=green>[GoogleSheet] Đã đồng bộ Live {count} câu hỏi trắc nghiệm từ Google Sheets!</color>");
                    onComplete?.Invoke(true, count, null);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[GoogleSheet] Lỗi phân tích Live CSV trắc nghiệm: " + ex.Message);
                    onComplete?.Invoke(false, 0, ex.Message);
                }
            }
            else
            {
                Debug.LogWarning($"[GoogleSheet] Không tải được Live Sheet ({www.error}).");
                onComplete?.Invoke(false, 0, www.error);
            }
        }
    }

    public IEnumerator FetchLivePhysicsEssaysRoutine(Action<bool, int, string> onComplete)
    {
        if (string.IsNullOrEmpty(physicsEssayCsvUrl))
        {
            onComplete?.Invoke(false, 0, "URL trống");
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequest.Get(physicsEssayCsvUrl))
        {
            www.timeout = 12;
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    int count = ScienceQuest.PhysicsGame2.GameDataRepository.LoadOrUpdateEssaysFromCsv(www.downloadHandler.text);
                    Debug.Log($"<color=green>[GoogleSheet] Đã đồng bộ Live {count} câu hỏi tự luận từ Google Sheets!</color>");
                    onComplete?.Invoke(true, count, null);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[GoogleSheet] Lỗi phân tích Live CSV tự luận: " + ex.Message);
                    onComplete?.Invoke(false, 0, ex.Message);
                }
            }
            else
            {
                Debug.LogWarning($"[GoogleSheet] Không tải được Live Sheet Tự Luận ({www.error}).");
                onComplete?.Invoke(false, 0, www.error);
            }
        }
    }"""

# Normalize \r\n
target1_norm = target1.replace('\r\n', '\n')
target2_norm = target2.replace('\r\n', '\n')
text_norm = text.replace('\r\n', '\n')

assert target1_norm in text_norm, "Target 1 not found"
assert target2_norm in text_norm, "Target 2 not found"

text_norm = text_norm.replace(target1_norm, replacement1.replace('\r\n', '\n'), 1)
text_norm = text_norm.replace(target2_norm, replacement2.replace('\r\n', '\n'), 1)

with open(path, 'w', encoding='utf-8', newline='\r\n') as f:
    f.write(text_norm)

print("Successfully updated GoogleSheetDataManager.cs")
