using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class TheoryPageData
    {
        public string heading;
        [TextArea(3, 10)]
        public string body;
        [TextArea(2, 6)]
        public string rememberText;

        public TheoryPageData() { }

        public TheoryPageData(string heading, string body, string rememberText)
        {
            this.heading = heading;
            this.body = body;
            this.rememberText = rememberText;
        }
    }

    [Serializable]
    public class TheoryLessonData
    {
        public string lessonId;
        public string chapterTitle;
        public string lessonTitle;
        public List<TheoryPageData> pages = new List<TheoryPageData>();

        public TheoryLessonData() { }

        public TheoryLessonData(string lessonId, string chapterTitle, string lessonTitle)
        {
            this.lessonId = lessonId;
            this.chapterTitle = chapterTitle;
            this.lessonTitle = lessonTitle;
            this.pages = new List<TheoryPageData>();
        }

        public void AddPage(string heading, string body, string rememberText)
        {
            pages.Add(new TheoryPageData(heading, body, rememberText));
        }
    }
}
