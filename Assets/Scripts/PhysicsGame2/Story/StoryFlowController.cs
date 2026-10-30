using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều phối toàn bộ luồng cốt truyện Aurelia Academy (Story Queue, Save Flags, Migration, Priority).
    /// Tách biệt hoàn toàn với tính toán Vật lý, chấm điểm Quiz/Written và mở khóa vật phẩm.
    /// </summary>
    public class StoryFlowController : MonoBehaviour
    {
        private readonly Queue<StoryContent> _storyQueue = new Queue<StoryContent>();
        private Action _onQueueFinished;
        private Transform _preferredCanvasTransform;
        private GameObject _activeFinalModal;

        public int QueuedCount => _storyQueue.Count;
        public GameObject ActiveFinalModal => _activeFinalModal;

        /// <summary>
        /// Kiểm tra nhanh xem có bất kỳ StoryModal hoặc FinalCompletionModal nào đang mở trên scene hiện tại không.
        /// Dùng để khóa tương tác Gameplay khi cốt truyện đang hiển thị.
        /// </summary>
        public static bool IsStoryModalOpen
        {
            get
            {
                var modal = FindAnyObjectByType<StoryModalController>(FindObjectsInactive.Include);
                if (modal != null && modal.IsShowing) return true;

                var flow = FindAnyObjectByType<StoryFlowController>(FindObjectsInactive.Include);
                if (flow != null && flow._activeFinalModal != null && flow._activeFinalModal.activeSelf) return true;

                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem Gameplay Scene (06..10) được vào từ luồng chơi bình thường (Explore Mode / chuyển cảnh hợp lệ)
        /// hay mở trực tiếp trong Unity Editor (Direct Play Safety).
        /// </summary>
        public static bool IsNormalGameplayEntry()
        {
            if (PhysicsGameSession.Instance == null) return false;
            return PhysicsGameSession.Instance.SelectedMode == GameMode.Explore ||
                   !string.IsNullOrEmpty(PhysicsGameSession.Instance.PreviousSceneName);
        }

        public bool IsShowingAnyModal
        {
            get
            {
                var modal = FindAnyObjectByType<StoryModalController>(FindObjectsInactive.Include);
                bool storyShowing = modal != null && modal.IsShowing;
                bool finalShowing = _activeFinalModal != null && _activeFinalModal.activeSelf;
                return storyShowing || finalShowing;
            }
        }

        /// <summary>
        /// Tìm hoặc khởi tạo StoryFlowController duy nhất trong scene hiện tại.
        /// </summary>
        public static StoryFlowController GetOrCreate(Transform canvasTransform = null)
        {
            StoryFlowController existing = FindAnyObjectByType<StoryFlowController>(FindObjectsInactive.Include);
            if (existing != null)
            {
                if (canvasTransform != null)
                {
                    existing._preferredCanvasTransform = canvasTransform;
                }
                return existing;
            }

            GameObject anchor = null;
            if (canvasTransform != null)
            {
                anchor = canvasTransform.gameObject;
            }
            else
            {
                Canvas c = FindAnyObjectByType<Canvas>();
                if (c != null) anchor = c.gameObject;
            }

            if (anchor == null)
            {
                anchor = new GameObject("StoryFlowController");
            }

            StoryFlowController controller = anchor.GetComponent<StoryFlowController>();
            if (controller == null)
            {
                controller = anchor.AddComponent<StoryFlowController>();
            }
            controller._preferredCanvasTransform = canvasTransform;
            return controller;
        }

        private void Awake()
        {
            StoryFlowController[] all = FindObjectsByType<StoryFlowController>(FindObjectsInactive.Include);
            if (all != null && all.Length > 1)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i] != null && all[i] != this)
                    {
                        DestroyImmediate(this);
                        return;
                    }
                }
            }
        }

        // =====================================================================
        // STORY QUEUE ENGINE
        // =====================================================================
        public void EnqueueStory(StoryContent content, Action onQueueFinished = null)
        {
            if (content == null)
            {
                onQueueFinished?.Invoke();
                return;
            }

            if (onQueueFinished != null)
            {
                _onQueueFinished = onQueueFinished;
            }

            _storyQueue.Enqueue(content);

            StoryModalController modal = StoryModalController.GetOrCreate(_preferredCanvasTransform);
            if (!modal.IsShowing)
            {
                ShowNextInQueue();
            }
        }

        public void EnqueueStories(IEnumerable<StoryContent> contents, Action onQueueFinished = null)
        {
            if (onQueueFinished != null)
            {
                _onQueueFinished = onQueueFinished;
            }

            bool addedAny = false;
            if (contents != null)
            {
                foreach (var c in contents)
                {
                    if (c != null)
                    {
                        _storyQueue.Enqueue(c);
                        addedAny = true;
                    }
                }
            }

            if (!addedAny)
            {
                Action cb = _onQueueFinished;
                _onQueueFinished = null;
                cb?.Invoke();
                return;
            }

            StoryModalController modal = StoryModalController.GetOrCreate(_preferredCanvasTransform);
            if (!modal.IsShowing)
            {
                ShowNextInQueue();
            }
        }

        public void ClearQueue()
        {
            _storyQueue.Clear();
            _onQueueFinished = null;
        }

        private void ShowNextInQueue()
        {
            if (_storyQueue.Count == 0)
            {
                Action finishedCallback = _onQueueFinished;
                _onQueueFinished = null;
                finishedCallback?.Invoke();
                return;
            }

            StoryContent next = _storyQueue.Dequeue();
            StoryModalController modal = StoryModalController.GetOrCreate(_preferredCanvasTransform);
            modal.ShowStory(next, onClosed: ShowNextInQueue);
        }

        // =====================================================================
        // BACKWARD COMPATIBILITY MIGRATION (REQUIREMENT 19)
        // =====================================================================
        /// <summary>
        /// Đảm bảo save cũ (đã hoàn thành trước khi có Story System) không bị spam lại toàn bộ Core Intro / Restored.
        /// Không ghi đè hasSeenFinalJourneyCompletion.
        /// </summary>
        public static bool ApplyStoryMigrationIfNeeded(PhysicsSaveData save, bool isFromLegacyFile = false)
        {
            if (save == null) return false;
            if (save.storyMigrationApplied) return false;

            bool isHistoricalCompletedSave =
                save.hasSeenFinalJourneyCompletion ||
                (isFromLegacyFile && (PhysicsFinalProgression.IsFinalJourneyComplete(save) ||
                                      save.IsItemUnlocked(PhysicsFinalProgression.FINAL_ITEM_ID)));

            if (isHistoricalCompletedSave)
            {
                save.hasSeenAureliaIntro = true;

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

                if (save.IsItemUnlocked(PhysicsFinalProgression.FINAL_ITEM_ID))
                {
                    save.hasSeenEnergyCrystalStory = true;
                }

                save.storyMigrationApplied = true;
                return true;
            }

            if (isFromLegacyFile)
            {
                for (int ch = 1; ch <= 5; ch++)
                {
                    if (IsCoreOfficiallyComplete(ch, save))
                    {
                        SetCoreIntroSeen(save, ch, true);
                        SetCoreRestoredStorySeen(save, ch, true);
                    }
                }

                if (save.IsItemUnlocked(PhysicsFinalProgression.FINAL_ITEM_ID))
                {
                    save.hasSeenEnergyCrystalStory = true;
                }

                save.storyMigrationApplied = true;
                return true;
            }

            return false;
        }

        // =====================================================================
        // OFFICIAL CORE COMPLETION & FLAG ACCESSORS
        // =====================================================================
        /// <summary>
        /// Kiểm tra điều kiện hoàn thành chính thức của từng Physics Core (tái sử dụng chuẩn PhysicsFinalProgression).
        /// </summary>
        public static bool IsCoreOfficiallyComplete(int chapterNumber, PhysicsSaveData save = null)
        {
            if (save == null)
            {
                save = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SaveData : PhysicsSaveManager.Load();
            }
            if (save == null) return false;

            switch (chapterNumber)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    return PhysicsFinalProgression.IsChapterQuizPassed(chapterNumber, save);

                default:
                    return false;
            }
        }

        public static bool HasSeenTeacherIntro(PhysicsSaveData save, string teacherId)
        {
            if (save == null) return false;
            string norm = (teacherId ?? "Minh").Trim().ToLowerInvariant();
            switch (norm)
            {
                case "lan": return save.hasSeenTeacherIntroLan;
                case "duy": return save.hasSeenTeacherIntroDuy;
                case "ha": return save.hasSeenTeacherIntroHa;
                case "minh":
                default: return save.hasSeenTeacherIntroMinh;
            }
        }

        public static void SetTeacherIntroSeen(PhysicsSaveData save, string teacherId, bool seen)
        {
            if (save == null) return;
            string norm = (teacherId ?? "Minh").Trim().ToLowerInvariant();
            switch (norm)
            {
                case "lan": save.hasSeenTeacherIntroLan = seen; break;
                case "duy": save.hasSeenTeacherIntroDuy = seen; break;
                case "ha": save.hasSeenTeacherIntroHa = seen; break;
                case "minh":
                default: save.hasSeenTeacherIntroMinh = seen; break;
            }
        }

        public static bool HasSeenCoreIntro(PhysicsSaveData save, int chapterNumber)
        {
            if (save == null) return false;
            switch (chapterNumber)
            {
                case 1: return save.hasSeenCore1Intro;
                case 2: return save.hasSeenCore2Intro;
                case 3: return save.hasSeenCore3Intro;
                case 4: return save.hasSeenCore4Intro;
                case 5: return save.hasSeenCore5Intro;
                default: return true;
            }
        }

        public static void SetCoreIntroSeen(PhysicsSaveData save, int chapterNumber, bool seen)
        {
            if (save == null) return;
            switch (chapterNumber)
            {
                case 1: save.hasSeenCore1Intro = seen; break;
                case 2: save.hasSeenCore2Intro = seen; break;
                case 3: save.hasSeenCore3Intro = seen; break;
                case 4: save.hasSeenCore4Intro = seen; break;
                case 5: save.hasSeenCore5Intro = seen; break;
            }
        }

        public static bool HasSeenCoreRestoredStory(PhysicsSaveData save, int chapterNumber)
        {
            if (save == null) return false;
            switch (chapterNumber)
            {
                case 1: return save.hasSeenCore1RestoredStory;
                case 2: return save.hasSeenCore2RestoredStory;
                case 3: return save.hasSeenCore3RestoredStory;
                case 4: return save.hasSeenCore4RestoredStory;
                case 5: return save.hasSeenCore5RestoredStory;
                default: return true;
            }
        }

        public static void SetCoreRestoredStorySeen(PhysicsSaveData save, int chapterNumber, bool seen)
        {
            if (save == null) return;
            switch (chapterNumber)
            {
                case 1: save.hasSeenCore1RestoredStory = seen; break;
                case 2: save.hasSeenCore2RestoredStory = seen; break;
                case 3: save.hasSeenCore3RestoredStory = seen; break;
                case 4: save.hasSeenCore4RestoredStory = seen; break;
                case 5: save.hasSeenCore5RestoredStory = seen; break;
            }
        }

        private static void SaveState(PhysicsSaveData save, bool isCustomSave)
        {
            if (save == null) return;
            save.storyMigrationApplied = true;
            if (!isCustomSave)
            {
                PhysicsSaveManager.Save(save);
                if (PhysicsGameSession.Instance != null)
                {
                    PhysicsGameSession.Instance.ReloadSaveData();
                }
            }
        }

        // =====================================================================
        // 1. GUIDE -> AURELIA INTRO -> CHARACTER SELECTION (REQUIREMENT 6)
        // =====================================================================
        public static bool HandleGuideCompletion(
            PhysicsSaveData customSave = null,
            Action navigateToCharacterSelection = null,
            Transform canvasTransform = null)
        {
            bool isCustom = customSave != null;
            var save = customSave ?? (PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SaveData : PhysicsSaveManager.Load());
            if (save == null) save = new PhysicsSaveData();

            ApplyStoryMigrationIfNeeded(save);

            save.hasViewedGuide = true;
            SaveState(save, isCustom);

            Action goNext = navigateToCharacterSelection ?? (() =>
            {
                if (PhysicsGameSession.Instance != null)
                    PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.CharacterSelection);
            });

            if (!save.hasSeenAureliaIntro)
            {
                var flow = GetOrCreate(canvasTransform);
                StoryContent intro = AureliaStoryCatalog.CreateAureliaCrisisIntro(() =>
                {
                    save.hasSeenAureliaIntro = true;
                    SaveState(save, isCustom);
                    goNext?.Invoke();
                });

                flow.EnqueueStory(intro);
                return true;
            }

            goNext?.Invoke();
            return false;
        }

        // =====================================================================
        // 2. TEACHER SELECTION -> TEACHER STORY -> CHAPTER SELECTION (REQ 8 & 20)
        // =====================================================================
        public static bool HandleTeacherSelection(
            string teacherId,
            PhysicsSaveData customSave = null,
            Action navigateToChapterSelection = null,
            Transform canvasTransform = null,
            Sprite portrait = null)
        {
            bool isCustom = customSave != null;
            var save = customSave ?? (PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SaveData : PhysicsSaveManager.Load());
            if (save == null) save = new PhysicsSaveData();

            ApplyStoryMigrationIfNeeded(save);

            string resolvedTeacher = !string.IsNullOrEmpty(teacherId)
                ? teacherId
                : (PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SelectedTeacherId : save.selectedTeacherId);

            if (string.IsNullOrEmpty(resolvedTeacher)) resolvedTeacher = "Minh";

            save.selectedTeacherId = resolvedTeacher;
            if (!isCustom && PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.SelectTeacher(resolvedTeacher);
            }

            Action goNext = navigateToChapterSelection ?? (() =>
            {
                if (PhysicsGameSession.Instance != null)
                    PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.ChapterSelection);
            });

            if (!HasSeenTeacherIntro(save, resolvedTeacher) || !save.hasSeenAureliaIntro)
            {
                var flow = GetOrCreate(canvasTransform);

                // Hộp thoại 1: Người dẫn chuyện về Lễ hội Ánh Sao và cơn bão
                StoryContent p1 = AureliaStoryCatalog.CreatePrologueNarrator();
                flow.EnqueueStory(p1);

                // Hộp thoại 2: Giáo viên giới thiệu nhiệm vụ khôi phục 5 khu vực
                StoryContent p2 = AureliaStoryCatalog.CreatePrologueTeacherMission(resolvedTeacher, null, portrait);
                flow.EnqueueStory(p2);

                // Hộp thoại 3: Giáo viên nhấn mạnh tự do chọn khu vực -> Bắt đầu hành trình
                StoryContent p3 = AureliaStoryCatalog.CreatePrologueTeacherChoice(resolvedTeacher, () =>
                {
                    SetTeacherIntroSeen(save, resolvedTeacher, true);
                    save.hasSeenAureliaIntro = true;
                    SaveState(save, isCustom);
                    goNext?.Invoke();
                }, portrait);
                flow.EnqueueStory(p3);

                return true;
            }

            goNext?.Invoke();
            return false;
        }

        // =====================================================================
        // 3. CHAPTER SELECTION -> LESSON SELECTION (Giới thiệu nhiệm vụ chương)
        // =====================================================================
        public static bool HandleChapterEntry(
            int chapterNumber,
            PhysicsSaveData customSave = null,
            Action navigateToLessonSelection = null,
            Transform canvasTransform = null)
        {
            bool isCustom = customSave != null;
            var save = customSave ?? (PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SaveData : PhysicsSaveManager.Load());
            if (save == null) save = new PhysicsSaveData();

            ApplyStoryMigrationIfNeeded(save);

            int clampedChapter = Mathf.Clamp(chapterNumber, 1, 5);
            save.lastChapterId = clampedChapter;
            if (!isCustom && PhysicsGameSession.Instance != null)
            {
                PhysicsGameSession.Instance.SelectChapter(clampedChapter);
            }

            Action goNext = navigateToLessonSelection ?? (() =>
            {
                bool isCompleted = RestorationService.IsChapterFullyRestored(clampedChapter);
                RouteId mapRoute = SceneCatalog.GetChapterMapRoute(clampedChapter, isCompleted);
                if (NavigationService.Instance != null)
                {
                    NavigationService.Instance.Selection.ChapterId = clampedChapter;
                    NavigationService.Instance.NavigateTo(mapRoute);
                }
                else if (PhysicsGameSession.Instance != null)
                {
                    string sceneName = SceneCatalog.GetSceneName(mapRoute);
                    PhysicsGameSession.Instance.LoadScene(sceneName);
                }
            });

            // Nếu là lần đầu vào chương: Hiện popup giới thiệu nhiệm vụ của chương đó
            if (!HasSeenCoreIntro(save, clampedChapter))
            {
                var flow = GetOrCreate(canvasTransform);
                StoryContent chapterIntro = AureliaStoryCatalog.CreateCoreIntro(clampedChapter, () =>
                {
                    SetCoreIntroSeen(save, clampedChapter, true);
                    SaveState(save, isCustom);
                    goNext?.Invoke();
                });
                flow.EnqueueStory(chapterIntro);
                return true;
            }

            goNext?.Invoke();
            return false;
        }

        // =====================================================================
        // 3B. GAMEPLAY SCENE 06..10 -> FULL PHYSICS CORE I..V INTRO STORY
        //     Chỉ hiển thị khi vào từ luồng bình thường (Normal Flow) và !hasSeenCoreXIntro.
        //     Cờ hasSeenCoreXIntro chỉ bật = true khi người chơi nhấn "BẮT ĐẦU NHIỆM VỤ".
        // =====================================================================
        public static bool TryShowGameplayCoreIntro(
            int chapterNumber,
            PhysicsSaveData customSave = null,
            Action onIntroFinishedOrSkipped = null,
            Transform canvasTransform = null,
            bool? forceNormalFlow = null)
        {
            bool isNormalFlow = forceNormalFlow ?? IsNormalGameplayEntry();
            if (!isNormalFlow)
            {
                onIntroFinishedOrSkipped?.Invoke();
                return false;
            }

            bool isCustom = customSave != null;
            var save = customSave ?? (PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SaveData : PhysicsSaveManager.Load());
            if (save == null) save = new PhysicsSaveData();

            ApplyStoryMigrationIfNeeded(save);

            int clampedChapter = Mathf.Clamp(chapterNumber, 1, 5);

            if (!HasSeenCoreIntro(save, clampedChapter))
            {
                var flow = GetOrCreate(canvasTransform);
                StoryContent coreIntro = AureliaStoryCatalog.CreateCoreIntro(clampedChapter, () =>
                {
                    SetCoreIntroSeen(save, clampedChapter, true);
                    SaveState(save, isCustom);
                    onIntroFinishedOrSkipped?.Invoke();
                });

                flow.EnqueueStory(coreIntro);
                return true;
            }

            onIntroFinishedOrSkipped?.Invoke();
            return false;
        }

        // =====================================================================
        // 4. CORE RESTORED -> ENERGY CRYSTAL -> FINAL COMPLETION (REQ 15-31)
        //    Priority:
        //    1. Core Restored story
        //    2. Energy Crystal story (Convergence -> Restored)
        //    3. Final Completion Modal ("AURELIA ACADEMY ĐÃ ĐƯỢC KHÔI PHỤC")
        //    READ-ONLY đối với dữ liệu vật phẩm & tiến độ bài học (chỉ đọc SaveData, chỉ ghi cờ Story).
        // =====================================================================
        public static bool TryShowPostGameplayOrPendingStories(
            int completedChapterHint = 0,
            PhysicsSaveData customSave = null,
            Action onAllFinished = null,
            Transform canvasTransform = null)
        {
            bool isCustom = customSave != null;
            var save = customSave ?? (PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SaveData : PhysicsSaveManager.Load());
            if (save == null)
            {
                onAllFinished?.Invoke();
                return false;
            }

            // Nếu là save cũ đã hoàn thành từ trước khi có Story System -> migrate để không spam
            ApplyStoryMigrationIfNeeded(save);

            List<StoryContent> pendingStories = new List<StoryContent>();

            // Priority 1: Core Restored Story
            if (completedChapterHint >= 1 && completedChapterHint <= 5)
            {
                if (IsCoreOfficiallyComplete(completedChapterHint, save) && !HasSeenCoreRestoredStory(save, completedChapterHint))
                {
                    int ch = completedChapterHint;
                    pendingStories.Add(AureliaStoryCatalog.CreateCoreRestored(ch, save, () =>
                    {
                        SetCoreRestoredStorySeen(save, ch, true);
                        SaveState(save, isCustom);
                    }));
                }
            }

            for (int ch = 1; ch <= 5; ch++)
            {
                if (ch == completedChapterHint) continue;
                if (IsCoreOfficiallyComplete(ch, save) && !HasSeenCoreRestoredStory(save, ch))
                {
                    int capturedCh = ch;
                    pendingStories.Add(AureliaStoryCatalog.CreateCoreRestored(capturedCh, save, () =>
                    {
                        SetCoreRestoredStorySeen(save, capturedCh, true);
                        SaveState(save, isCustom);
                    }));
                }
            }

            // Priority 2: Energy Crystal Story (khi energy_crystal đã unlock thật trong SaveData và chưa xem EnergyCrystalStory)
            bool crystalUnlocked = save.IsItemUnlocked(PhysicsFinalProgression.FINAL_ITEM_ID);
            if (crystalUnlocked && !save.hasSeenEnergyCrystalStory)
            {
                pendingStories.Add(AureliaStoryCatalog.CreateEnergyCrystalConvergence());
                pendingStories.Add(AureliaStoryCatalog.CreateEnergyCrystalRestored(save, () =>
                {
                    save.hasSeenEnergyCrystalStory = true;
                    SaveState(save, isCustom);
                }));
            }

            // Priority 3: Final Completion Modal
            bool shouldShowFinalModal =
                PhysicsFinalProgression.IsFinalJourneyComplete(save) &&
                crystalUnlocked &&
                !save.hasSeenFinalJourneyCompletion;

            if (pendingStories.Count == 0 && !shouldShowFinalModal)
            {
                onAllFinished?.Invoke();
                return false;
            }

            var flow = GetOrCreate(canvasTransform);
            Transform targetParent = canvasTransform;
            if (targetParent == null)
            {
                Canvas c = FindAnyObjectByType<Canvas>();
                if (c != null) targetParent = c.transform;
            }

            if (pendingStories.Count > 0)
            {
                flow.EnqueueStories(pendingStories, () =>
                {
                    if (shouldShowFinalModal && !save.hasSeenFinalJourneyCompletion && targetParent != null)
                    {
                        flow._activeFinalModal = PhysicsFinalProgression.ShowFinalCompletionModal(targetParent, () =>
                        {
                            save.hasSeenFinalJourneyCompletion = true;
                            SaveState(save, isCustom);
                            flow._activeFinalModal = null;
                            onAllFinished?.Invoke();
                        }, save, isCustom);
                    }
                    else
                    {
                        onAllFinished?.Invoke();
                    }
                });
                return true;
            }

            // Chỉ còn Final Completion Modal
            if (shouldShowFinalModal && targetParent != null)
            {
                flow._activeFinalModal = PhysicsFinalProgression.ShowFinalCompletionModal(targetParent, () =>
                {
                    save.hasSeenFinalJourneyCompletion = true;
                    SaveState(save, isCustom);
                    flow._activeFinalModal = null;
                    onAllFinished?.Invoke();
                }, save, isCustom);
                return true;
            }

            onAllFinished?.Invoke();
            return false;
        }
    }
}
