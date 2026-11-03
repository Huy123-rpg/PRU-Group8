#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScienceQuest.PhysicsGame2;

namespace ScienceQuest.PhysicsGame2.Editor
{
    /// <summary>
    /// Automated verification suite for the Aurelia Academy Story System (TESTS 1 - 14).
    /// Menu: Tools/Physics Game 2/Run Aurelia Story System Verification Tests
    /// </summary>
    public static class StorySystemVerificationTest
    {
        [MenuItem("Tools/Physics Game 2/Run Aurelia Story System Verification Tests")]
        public static void RunAllTests()
        {
            int passCount = 0;
            int totalTests = 0;

            Debug.Log("==================================================");
            Debug.Log("[AureliaStoryTest] STARTING AUTOMATED STORY SYSTEM TESTS (1 - 14)");
            Debug.Log("==================================================");

            GameObject canvasGo = new GameObject("TestStoryCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            try
            {
                // ==================================================
                // TEST 1: New Game -> finish 16. Guide -> Aurelia Intro story appears
                // ==================================================
                totalTests++;
                {
                    var save = CreateFreshNewGameSave();
                    bool navigatedToCharSelect = false;

                    bool shown = StoryFlowController.HandleGuideCompletion(
                        customSave: save,
                        navigateToCharacterSelection: () => navigatedToCharSelect = true,
                        canvasTransform: canvas.transform);

                    var modal = StoryModalController.GetOrCreate(canvas.transform);
                    bool modalVisibleBeforeClick = modal.IsShowing &&
                                                   modal.CurrentContent != null &&
                                                   modal.CurrentContent.title == "KHỦNG HOẢNG TẠI AURELIA ACADEMY" &&
                                                   modal.CurrentContent.buttonText == "BẮT ĐẦU HÀNH TRÌNH" &&
                                                   !navigatedToCharSelect;

                    modal.AcknowledgeCurrentStory();

                    bool test1Passed = shown &&
                                       modalVisibleBeforeClick &&
                                       save.hasViewedGuide &&
                                       save.hasSeenAureliaIntro &&
                                       navigatedToCharSelect &&
                                       !modal.IsShowing;

                    RecordResult(1, "New Game -> Guide completion shows Aurelia Intro story once -> sets hasSeenAureliaIntro = true -> navigates to 02_CharacterSelection", test1Passed, ref passCount);
                }

                // ==================================================
                // TEST 2: Repeat Guide with hasSeenAureliaIntro = true -> Intro story does NOT show again
                // ==================================================
                totalTests++;
                {
                    var save = CreateFreshNewGameSave();
                    save.hasSeenAureliaIntro = true;
                    bool navigated = false;

                    bool shown = StoryFlowController.HandleGuideCompletion(
                        customSave: save,
                        navigateToCharacterSelection: () => navigated = true,
                        canvasTransform: canvas.transform);

                    var modal = StoryModalController.GetOrCreate(canvas.transform);
                    bool test2Passed = !shown && !modal.IsShowing && navigated;

                    RecordResult(2, "Repeat Guide with hasSeenAureliaIntro = true -> Intro story does not show again", test2Passed, ref passCount);
                }

                // ==================================================
                // TEST 3: Select Teacher Minh -> Teacher Minh story appears -> sets hasSeenTeacherIntroMinh = true -> loads 04_ChapterSelection
                // ==================================================
                totalTests++;
                {
                    var save = CreateFreshNewGameSave();
                    bool navigatedToChapters = false;

                    bool shown = StoryFlowController.HandleTeacherSelection(
                        teacherId: "Minh",
                        customSave: save,
                        navigateToChapterSelection: () => navigatedToChapters = true,
                        canvasTransform: canvas.transform);

                    var modal = StoryModalController.GetOrCreate(canvas.transform);
                    bool validBeforeClick = modal.IsShowing &&
                                            modal.CurrentContent != null &&
                                            modal.CurrentContent.speaker == "THẦY MINH" &&
                                            modal.CurrentContent.buttonText == "EM SẴN SÀNG" &&
                                            !navigatedToChapters;

                    modal.AcknowledgeCurrentStory();

                    bool test3Passed = shown &&
                                       validBeforeClick &&
                                       save.hasSeenTeacherIntroMinh &&
                                       !save.hasSeenTeacherIntroLan &&
                                       !save.hasSeenTeacherIntroDuy &&
                                       !save.hasSeenTeacherIntroHa &&
                                       navigatedToChapters &&
                                       !modal.IsShowing;

                    RecordResult(3, "Select Teacher Minh -> Teacher Minh story appears -> sets hasSeenTeacherIntroMinh = true -> navigates to 04_ChapterSelection", test3Passed, ref passCount);
                }

                // ==================================================
                // TEST 4: Select Teacher Minh again -> Minh story does not repeat
                // ==================================================
                totalTests++;
                {
                    var save = CreateFreshNewGameSave();
                    save.hasSeenTeacherIntroMinh = true;
                    bool navigated = false;

                    bool shown = StoryFlowController.HandleTeacherSelection(
                        teacherId: "Minh",
                        customSave: save,
                        navigateToChapterSelection: () => navigated = true,
                        canvasTransform: canvas.transform);

                    var modal = StoryModalController.GetOrCreate(canvas.transform);
                    bool test4Passed = !shown && !modal.IsShowing && navigated;

                    RecordResult(4, "Select Teacher Minh again -> Minh story does not repeat", test4Passed, ref passCount);
                }

                // ==================================================
                // TEST 5: Select Teacher Duy (after Minh already seen) -> Teacher Duy story appears once
                // ==================================================
                totalTests++;
                {
                    var save = CreateFreshNewGameSave();
                    save.hasSeenTeacherIntroMinh = true;
                    bool navigated = false;

                    bool shown = StoryFlowController.HandleTeacherSelection(
                        teacherId: "Duy",
                        customSave: save,
                        navigateToChapterSelection: () => navigated = true,
                        canvasTransform: canvas.transform);

                    var modal = StoryModalController.GetOrCreate(canvas.transform);
                    bool validContent = modal.IsShowing &&
                                        modal.CurrentContent != null &&
                                        modal.CurrentContent.speaker == "THẦY DUY" &&
                                        modal.CurrentContent.buttonText == "EM SẴN SÀNG";

                    modal.AcknowledgeCurrentStory();

                    bool shownSecondTime = StoryFlowController.HandleTeacherSelection(
                        teacherId: "Duy",
                        customSave: save,
                        navigateToChapterSelection: () => { },
                        canvasTransform: canvas.transform);

                    bool test5Passed = shown && validContent && save.hasSeenTeacherIntroDuy && navigated && !shownSecondTime;
                    RecordResult(5, "Select Teacher Duy -> Teacher Duy story appears once", test5Passed, ref passCount);
                }

                // ==================================================
                // TEST 6: Scene 04 does NOT show/consume Core Intro; Gameplay Scene 06..10 shows Core I..V Intro once ("BẮT ĐẦU NHIỆM VỤ")
                // ==================================================
                totalTests++;
                {
                    var save = CreateFreshNewGameSave();
                    bool allFiveValid = true;

                    string[] expectedTitles =
                    {
                        "PHYSICS CORE I – CƠ HỌC",
                        "PHYSICS CORE II – ÁNH SÁNG",
                        "PHYSICS CORE III – ĐIỆN",
                        "PHYSICS CORE IV – ĐIỆN TỪ",
                        "PHYSICS CORE V – NĂNG LƯỢNG"
                    };

                    for (int ch = 1; ch <= 5; ch++)
                    {
                        // 1) Scene 04 (HandleChapterEntry) must NOT show modal and must NOT consume hasSeenCoreXIntro
                        bool navToLessons = false;
                        bool shownOnScene04 = StoryFlowController.HandleChapterEntry(
                            chapterNumber: ch,
                            customSave: save,
                            navigateToLessonSelection: () => navToLessons = true,
                            canvasTransform: canvas.transform);

                        if (shownOnScene04 || !navToLessons || StoryFlowController.HasSeenCoreIntro(save, ch))
                        {
                            allFiveValid = false;
                            break;
                        }

                        // 2) Gameplay Scene 06..10 (TryShowGameplayCoreIntro) shows full Core Intro
                        bool gameplayUnlocked = false;
                        bool shownOnGameplay = StoryFlowController.TryShowGameplayCoreIntro(
                            chapterNumber: ch,
                            customSave: save,
                            onIntroFinishedOrSkipped: () => gameplayUnlocked = true,
                            canvasTransform: canvas.transform,
                            forceNormalFlow: true);

                        var modal = StoryModalController.GetOrCreate(canvas.transform);
                        float blockerAlpha = modal.BlockerImage != null ? modal.BlockerImage.color.a : 0f;
                        Vector2 cardSize = modal.CardRect != null ? modal.CardRect.sizeDelta : Vector2.zero;

                        // Flag must NOT be set before clicking "BẮT ĐẦU NHIỆM VỤ"
                        if (!shownOnGameplay || !modal.IsShowing || !StoryFlowController.IsStoryModalOpen ||
                            StoryFlowController.HasSeenCoreIntro(save, ch) ||
                            modal.CurrentContent == null ||
                            modal.CurrentContent.title != expectedTitles[ch - 1] ||
                            modal.CurrentContent.buttonText != "BẮT ĐẦU NHIỆM VỤ" ||
                            blockerAlpha < 0.45f || blockerAlpha > 0.60f ||
                            Mathf.Abs(cardSize.x - 1120f) > 1f || Mathf.Abs(cardSize.y - 640f) > 1f ||
                            gameplayUnlocked)
                        {
                            allFiveValid = false;
                            break;
                        }

                        modal.AcknowledgeCurrentStory();

                        if (!StoryFlowController.HasSeenCoreIntro(save, ch) || !gameplayUnlocked || modal.IsShowing || StoryFlowController.IsStoryModalOpen)
                        {
                            allFiveValid = false;
                            break;
                        }
                    }

                    RecordResult(6, "Scene 04 does NOT show/consume Core Intro; Gameplay Scenes 06..10 show Core I..V Intro once with 'BẮT ĐẦU NHIỆM VỤ' and translucent background", allFiveValid, ref passCount);
                }

                // ==================================================
                // TEST 7: Re-enter same Core Gameplay Scene does not repeat; Different Core shows its Intro; Gameplay Block & Interruption Recovery
                // ==================================================
                totalTests++;
                {
                    var save = CreateFreshNewGameSave();
                    save.hasSeenCore1Intro = true;

                    // 1) Re-enter Core I Gameplay Scene -> does NOT repeat
                    bool unlocked1 = false;
                    bool repeated1 = StoryFlowController.TryShowGameplayCoreIntro(1, save, () => unlocked1 = true, canvas.transform, forceNormalFlow: true);

                    // 2) Enter Core II Gameplay Scene -> shows Core II Intro
                    bool unlocked2 = false;
                    bool shown2 = StoryFlowController.TryShowGameplayCoreIntro(2, save, () => unlocked2 = true, canvas.transform, forceNormalFlow: true);
                    var modal = StoryModalController.GetOrCreate(canvas.transform);

                    // Verify Gameplay Block while StoryModal is open
                    bool blockedWhileOpen = StoryFlowController.IsStoryModalOpen && !save.hasSeenCore2Intro && !unlocked2;

                    // Simulate interruption before clicking "BẮT ĐẦU NHIỆM VỤ" -> flag stays false, re-entering shows Core II Intro again
                    modal.HideModal();
                    bool flagStayedFalseOnInterrupt = !save.hasSeenCore2Intro;

                    bool shown2AfterInterrupt = StoryFlowController.TryShowGameplayCoreIntro(2, save, () => unlocked2 = true, canvas.transform, forceNormalFlow: true);
                    modal.AcknowledgeCurrentStory();

                    // 3) Standalone / non-normal flow entry does NOT show Core Intro
                    bool unlocked3 = false;
                    bool shownStandalone = StoryFlowController.TryShowGameplayCoreIntro(3, save, () => unlocked3 = true, canvas.transform, forceNormalFlow: false);

                    bool test7Passed = !repeated1 && unlocked1 &&
                                       shown2 && blockedWhileOpen &&
                                       flagStayedFalseOnInterrupt && shown2AfterInterrupt &&
                                       save.hasSeenCore2Intro && unlocked2 &&
                                       !shownStandalone && unlocked3 && !save.hasSeenCore3Intro;

                    RecordResult(7, "Same Core does not repeat; Different Core shows Intro; Gameplay blocked while open; Interruption before click preserves flag = false", test7Passed, ref passCount);
                }

                // ==================================================
                // TEST 8: Pass required Core I test -> Core I Restored story appears once
                // ==================================================
                totalTests++;
                {
                    var save = CreateFreshNewGameSave();
                    save.MarkGameplayChapterCompleted("chapter_1");
                    MarkRequiredTestsComplete(save, 1);
                    save.UnlockItem("gear");

                    bool shown = StoryFlowController.TryShowPostGameplayOrPendingStories(
                        completedChapterHint: 1,
                        customSave: save,
                        onAllFinished: null,
                        canvasTransform: canvas.transform);

                    var modal = StoryModalController.GetOrCreate(canvas.transform);
                    bool validModal = modal.IsShowing &&
                                      modal.CurrentContent != null &&
                                      modal.CurrentContent.title == "CORE I ĐÃ ĐƯỢC KHÔI PHỤC" &&
                                      modal.CurrentContent.buttonText == "TIẾP TỤC HÀNH TRÌNH" &&
                                      !string.IsNullOrEmpty(modal.CurrentContent.rewardTitle);

                    modal.AcknowledgeCurrentStory();

                    bool shownAgain = StoryFlowController.TryShowPostGameplayOrPendingStories(1, save, null, canvas.transform);
                    bool test8Passed = shown && validModal && save.hasSeenCore1RestoredStory && !shownAgain;

                    RecordResult(8, "Pass Core I required test -> Core I Restored story appears once -> sets hasSeenCore1RestoredStory = true", test8Passed, ref passCount);
                }

                // ==================================================
                // TEST 9: Pass required Core II..V tests -> Core II..V Restored stories appear once
                // ==================================================
                totalTests++;
                {
                    var save = CreateFreshNewGameSave();
                    for (int chapter = 2; chapter <= 5; chapter++) MarkRequiredTestsComplete(save, chapter);
                    foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH2) save.MarkExploreLessonCompleted(l);
                    foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH3) save.MarkExploreLessonCompleted(l);
                    foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH4) save.MarkExploreLessonCompleted(l);
                    foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH5) save.MarkExploreLessonCompleted(l);

                    save.UnlockItem("prism");
                    save.UnlockItem("wire");
                    save.UnlockItem("magnet");
                    save.UnlockItem("turbine");
                    save.UnlockItem("solar_panel");

                    string[] expectedRestoredTitles =
                    {
                        "CORE II ĐÃ ĐƯỢC KHÔI PHỤC",
                        "CORE III ĐÃ ĐƯỢC KHÔI PHỤC",
                        "CORE IV ĐÃ ĐƯỢC KHÔI PHỤC",
                        "CORE V ĐÃ ĐƯỢC KHÔI PHỤC"
                    };

                    string[] expectedRestoredButtons =
                    {
                        "TIẾP TỤC HÀNH TRÌNH",
                        "TIẾP TỤC HÀNH TRÌNH",
                        "TIẾP TỤC HÀNH TRÌNH",
                        "TIẾP TỤC"
                    };

                    bool shown = StoryFlowController.TryShowPostGameplayOrPendingStories(2, save, null, canvas.transform);
                    var modal = StoryModalController.GetOrCreate(canvas.transform);
                    bool allMatched = shown;

                    for (int i = 0; i < expectedRestoredTitles.Length; i++)
                    {
                        if (!modal.IsShowing || modal.CurrentContent == null ||
                            modal.CurrentContent.title != expectedRestoredTitles[i] ||
                            modal.CurrentContent.buttonText != expectedRestoredButtons[i])
                        {
                            allMatched = false;
                            break;
                        }
                        modal.AcknowledgeCurrentStory();
                    }

                    bool flagsSet = save.hasSeenCore2RestoredStory &&
                                    save.hasSeenCore3RestoredStory &&
                                    save.hasSeenCore4RestoredStory &&
                                    save.hasSeenCore5RestoredStory;

                    RecordResult(9, "Pass required Core II..V tests -> Core II..V Restored stories appear once -> sets hasSeenCore2..5RestoredStory = true", allMatched && flagsSet && !modal.IsShowing, ref passCount);
                }

                // ==================================================
                // TEST 10: Partial Core completion -> Core Restored story does NOT show
                // ==================================================
                totalTests++;
                {
                    var save = CreateFreshNewGameSave();
                    // Only 1 lesson of Ch2, 1 lesson of Ch3, 1 lesson of Ch4, 1 lesson of Ch5
                    save.MarkExploreLessonCompleted("C2_L5");
                    save.MarkExploreLessonCompleted("C3_L11");
                    save.MarkExploreLessonCompleted("C4_L14");
                    save.MarkExploreLessonCompleted("C5_L16");
                    // Completing lessons or marking the chapter alone is insufficient; C2_TEST is the configured required test.
                    save.MarkGameplayChapterCompleted("chapter_2");

                    bool shown = StoryFlowController.TryShowPostGameplayOrPendingStories(2, save, null, canvas.transform);
                    var modal = StoryModalController.GetOrCreate(canvas.transform);
                    bool test10Passed = !shown &&
                                        !modal.IsShowing &&
                                        !StoryFlowController.IsCoreOfficiallyComplete(2, save) &&
                                        !StoryFlowController.IsCoreOfficiallyComplete(3, save) &&
                                        !StoryFlowController.IsCoreOfficiallyComplete(4, save) &&
                                        !StoryFlowController.IsCoreOfficiallyComplete(5, save);

                    RecordResult(10, "Partial Core completion -> Core Restored story does NOT show", test10Passed, ref passCount);
                }

                // ==================================================
                // TEST 11: Complete all 5 Cores -> energy_crystal unlocked ->
                //          Core V Restored -> Energy Crystal story -> Final Completion Modal sequentially
                // ==================================================
                totalTests++;
                {
                    var save = CreateFreshNewGameSave();
                    // Cores I..IV already completed and seen
                    save.MarkGameplayChapterCompleted("chapter_1");
                    for (int chapter = 1; chapter <= 4; chapter++) MarkRequiredTestsComplete(save, chapter);
                    foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH2) save.MarkExploreLessonCompleted(l);
                    foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH3) save.MarkExploreLessonCompleted(l);
                    foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH4) save.MarkExploreLessonCompleted(l);
                    save.hasSeenCore1RestoredStory = true;
                    save.hasSeenCore2RestoredStory = true;
                    save.hasSeenCore3RestoredStory = true;
                    save.hasSeenCore4RestoredStory = true;

                    // Now complete Core V and unlock energy_crystal via progression (as gameplay controller does)
                    MarkRequiredTestsComplete(save, 5);
                    foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH5) save.MarkExploreLessonCompleted(l);
                    save.UnlockItem("turbine");
                    save.UnlockItem("solar_panel");
                    save.UnlockItem(PhysicsFinalProgression.FINAL_ITEM_ID);

                    bool sequenceCompletedCallback = false;
                    bool started = StoryFlowController.TryShowPostGameplayOrPendingStories(
                        completedChapterHint: 5,
                        customSave: save,
                        onAllFinished: () => sequenceCompletedCallback = true,
                        canvasTransform: canvas.transform);

                    var modal = StoryModalController.GetOrCreate(canvas.transform);

                    // Step 1: Core V Restored Story (FinalModal must NOT be active yet)
                    bool step1Valid = started &&
                                      save.IsItemUnlocked(PhysicsFinalProgression.FINAL_ITEM_ID) &&
                                      modal.IsShowing &&
                                      modal.CurrentContent != null &&
                                      modal.CurrentContent.title == "CORE V ĐÃ ĐƯỢC KHÔI PHỤC" &&
                                      modal.CurrentContent.buttonText == "TIẾP TỤC" &&
                                      canvas.transform.Find("FinalCompletionModal") == null;

                    modal.AcknowledgeCurrentStory();

                    // Step 2: Energy Crystal Convergence ("NĂNG LƯỢNG ĐANG HỘI TỤ...")
                    bool step2Valid = modal.IsShowing &&
                                      modal.CurrentContent != null &&
                                      modal.CurrentContent.title == "NĂNG LƯỢNG ĐANG HỘI TỤ..." &&
                                      modal.CurrentContent.buttonText == "TIẾP TỤC" &&
                                      canvas.transform.Find("FinalCompletionModal") == null;

                    modal.AcknowledgeCurrentStory();

                    // Step 3: Energy Crystal Restored ("TINH THỂ AURELIA ĐÃ ĐƯỢC TÁI TẠO")
                    bool step3Valid = modal.IsShowing &&
                                      modal.CurrentContent != null &&
                                      modal.CurrentContent.title == "TINH THỂ AURELIA ĐÃ ĐƯỢC TÁI TẠO" &&
                                      modal.CurrentContent.buttonText == "HOÀN TẤT KHÔI PHỤC" &&
                                      !string.IsNullOrEmpty(modal.CurrentContent.rewardTitle) &&
                                      canvas.transform.Find("FinalCompletionModal") == null;

                    modal.AcknowledgeCurrentStory();

                    // Step 4: StoryModal hidden, FinalCompletionModal now active!
                    Transform finalModalTr = canvas.transform.Find("FinalCompletionModal");
                    bool step4Valid = !modal.IsShowing &&
                                      save.hasSeenCore5RestoredStory &&
                                      save.hasSeenEnergyCrystalStory &&
                                      !save.hasSeenFinalJourneyCompletion &&
                                      finalModalTr != null &&
                                      finalModalTr.gameObject.activeSelf;

                    // Dismiss FinalCompletionModal via Btn_Close
                    if (finalModalTr != null)
                    {
                        Button closeBtn = finalModalTr.Find("Card/Btn_Close")?.GetComponent<Button>();
                        if (closeBtn != null) closeBtn.onClick.Invoke();
                    }

                    bool step5Valid = save.hasSeenFinalJourneyCompletion && sequenceCompletedCallback;
                    bool test11Passed = step1Valid && step2Valid && step3Valid && step4Valid && step5Valid;

                    RecordResult(11, "Complete all 5 Cores -> Core V Restored -> Energy Crystal story -> Final Completion Modal sequentially without overlap", test11Passed, ref passCount);
                }

                // ==================================================
                // TEST 12: Restart game after all stories seen -> no story repeats
                // ==================================================
                totalTests++;
                {
                    var save = CreateFullyCompletedSaveWithAllStoriesSeen();
                    bool shown = StoryFlowController.TryShowPostGameplayOrPendingStories(0, save, null, canvas.transform);
                    var modal = StoryModalController.GetOrCreate(canvas.transform);
                    bool test12Passed = !shown && !modal.IsShowing;

                    RecordResult(12, "Restart game after all stories seen -> no story repeats", test12Passed, ref passCount);
                }

                // ==================================================
                // TEST 13: Old completed save migration -> already completed Cores do NOT spam old stories
                // ==================================================
                totalTests++;
                {
                    // Simulate legacy completed save before Story System existed (storyMigrationApplied = false)
                    var legacySave = new PhysicsSaveData();
                    legacySave.storyMigrationApplied = false;
                    legacySave.hasViewedGuide = true;
                    legacySave.selectedCharacterId = "Lyra";
                    legacySave.selectedTeacherId = "Minh";
                    legacySave.MarkGameplayChapterCompleted("chapter_1");
                    for (int chapter = 1; chapter <= 5; chapter++) MarkRequiredTestsComplete(legacySave, chapter);
                    foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH2) legacySave.MarkExploreLessonCompleted(l);
                    foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH3) legacySave.MarkExploreLessonCompleted(l);
                    foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH4) legacySave.MarkExploreLessonCompleted(l);
                    foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH5) legacySave.MarkExploreLessonCompleted(l);
                    legacySave.UnlockItem("energy_crystal");
                    legacySave.hasSeenFinalJourneyCompletion = true;

                    bool migrated = StoryFlowController.ApplyStoryMigrationIfNeeded(legacySave, isFromLegacyFile: true);
                    bool anySpam = StoryFlowController.TryShowPostGameplayOrPendingStories(0, legacySave, null, canvas.transform);

                    bool test13Passed = migrated &&
                                        legacySave.storyMigrationApplied &&
                                        legacySave.hasSeenAureliaIntro &&
                                        legacySave.hasSeenCore1Intro &&
                                        legacySave.hasSeenCore2Intro &&
                                        legacySave.hasSeenCore3Intro &&
                                        legacySave.hasSeenCore4Intro &&
                                        legacySave.hasSeenCore5Intro &&
                                        legacySave.hasSeenCore1RestoredStory &&
                                        legacySave.hasSeenCore2RestoredStory &&
                                        legacySave.hasSeenCore3RestoredStory &&
                                        legacySave.hasSeenCore4RestoredStory &&
                                        legacySave.hasSeenCore5RestoredStory &&
                                        legacySave.hasSeenEnergyCrystalStory &&
                                        legacySave.hasSeenFinalJourneyCompletion &&
                                        !anySpam;

                    RecordResult(13, "Old completed save migration -> sets hasSeenCore*Intro, hasSeenCore*RestoredStory, hasSeenEnergyCrystalStory = true and does NOT spam old stories", test13Passed, ref passCount);
                }

                // ==================================================
                // TEST 14: Reward accuracy & Read-only safety -> Core Restored story only displays rewards actually unlocked in PhysicsSaveData and never unlocks items
                // ==================================================
                totalTests++;
                {
                    // Case A: Core III completed, only ammeter and voltmeter unlocked -> reward text shows only those, not wire
                    var savePartial = CreateFreshNewGameSave();
                    savePartial.UnlockItem("ammeter");
                    savePartial.UnlockItem("voltmeter");
                    StoryContent core3Partial = AureliaStoryCatalog.CreateCoreRestored(3, savePartial);

                    // Case B: Core I completed, no item in unlockedItemIds -> rewardTitle is empty (reward box hidden)
                    var saveNoItem = CreateFreshNewGameSave();
                    saveNoItem.MarkGameplayChapterCompleted("chapter_1");
                    MarkRequiredTestsComplete(saveNoItem, 1);
                    StoryContent core1NoItem = AureliaStoryCatalog.CreateCoreRestored(1, saveNoItem);
                    StoryFlowController.TryShowPostGameplayOrPendingStories(1, saveNoItem, null, canvas.transform);
                    var modal = StoryModalController.GetOrCreate(canvas.transform);
                    modal.AcknowledgeCurrentStory();

                    bool test14Passed =
                        !string.IsNullOrEmpty(core3Partial.rewardTitle) &&
                        !core3Partial.rewardTitle.ToUpperInvariant().Contains("DÂY DẪN") &&
                        !core3Partial.rewardTitle.ToUpperInvariant().Contains("WIRE") &&
                        string.IsNullOrEmpty(core1NoItem.rewardTitle) &&
                        saveNoItem.unlockedItemIds.Count == 0;

                    RecordResult(14, "Reward accuracy & Read-only safety -> Core Restored story only displays rewards actually unlocked in PhysicsSaveData and never unlocks items", test14Passed, ref passCount);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(canvasGo);
                var flow = UnityEngine.Object.FindAnyObjectByType<StoryFlowController>();
                if (flow != null) UnityEngine.Object.DestroyImmediate(flow.gameObject);
            }

            Debug.Log("==================================================");
            Debug.Log($"[AureliaStoryTest] SUMMARY: {passCount}/{totalTests} TESTS PASSED");
            Debug.Log("==================================================");
        }

        private static PhysicsSaveData CreateFreshNewGameSave()
        {
            var save = new PhysicsSaveData();
            save.storyMigrationApplied = true;
            return save;
        }

        private static PhysicsSaveData CreateFullyCompletedSaveWithAllStoriesSeen()
        {
            var save = CreateFreshNewGameSave();
            save.hasViewedGuide = true;
            save.hasSeenAureliaIntro = true;
            save.hasSeenTeacherIntroMinh = true;
            save.hasSeenTeacherIntroLan = true;
            save.hasSeenTeacherIntroDuy = true;
            save.hasSeenTeacherIntroHa = true;
            save.hasSeenCore1Intro = true;
            save.hasSeenCore2Intro = true;
            save.hasSeenCore3Intro = true;
            save.hasSeenCore4Intro = true;
            save.hasSeenCore5Intro = true;
            save.hasSeenCore1RestoredStory = true;
            save.hasSeenCore2RestoredStory = true;
            save.hasSeenCore3RestoredStory = true;
            save.hasSeenCore4RestoredStory = true;
            save.hasSeenCore5RestoredStory = true;
            save.hasSeenEnergyCrystalStory = true;
            save.hasSeenFinalJourneyCompletion = true;

            save.MarkGameplayChapterCompleted("chapter_1");
            for (int chapter = 1; chapter <= 5; chapter++) MarkRequiredTestsComplete(save, chapter);
            foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH2) save.MarkExploreLessonCompleted(l);
            foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH3) save.MarkExploreLessonCompleted(l);
            foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH4) save.MarkExploreLessonCompleted(l);
            foreach (var l in PhysicsFinalProgression.REQUIRED_EXPLORE_CH5) save.MarkExploreLessonCompleted(l);
            save.UnlockItem(PhysicsFinalProgression.FINAL_ITEM_ID);
            return save;
        }

        private static void MarkRequiredTestsComplete(PhysicsSaveData save, int chapterNumber)
        {
            foreach (var test in PhysicsCurriculumData.GetLessons(chapterNumber)
                         .FindAll(lesson => lesson != null && lesson.isTest && lesson.isRequiredForChapterCompletion))
            {
                string key = $"{chapterNumber}_{test.lessonId}";
                var entry = save.lessonScores.Find(score => score != null && score.lessonId == key);
                if (entry == null) save.lessonScores.Add(new LessonScoreEntry(key, 10, 10, 10, true));
                else entry.isCompleted = true;
                if (!save.completedLessons.Contains(key)) save.completedLessons.Add(key);
            }
        }

        private static void RecordResult(int testNumber, string description, bool passed, ref int passCount)
        {
            if (passed)
            {
                passCount++;
                Debug.Log($"✓ TEST {testNumber} PASSED: {description}");
            }
            else
            {
                Debug.LogError($"✗ TEST {testNumber} FAILED: {description}");
            }
        }
    }
}
#endif
