using System;
using System.Collections.Generic;

[Serializable]
public class AIGradingResponse
{
    public bool readable;
    public float score;
    public float maxScore;
    public float percentage;
    public List<string> correctPoints;
    public List<string> missingPoints;
    public string feedback;
    public string suggestedAnswer;
}
