with open('Assets/Scripts/PhysicsGame2/Core/PhysicsGameBootstrap.cs', 'r', encoding='utf-8') as f:
    content = f.read()

target = '''                case PhysicsSceneNames.QuizResult: // "19.1. Result_TL"
                case PhysicsSceneNames.WrittenResult: // "19. Result_TN"
                    bool isEssayMode = (NavigationService.Instance != null && NavigationService.Instance.Selection.Mode == "ESSAY");
                    if (isEssayMode)
                    {
                        if (Object.FindAnyObjectByType<EssayResultController>() == null)
                        {
                            GameObject anchor = FindSceneCanvasOrCreateAnchor("EssayResultController", sceneName);
                            anchor.AddComponent<EssayResultController>();
                            Debug.Log($"[PhysicsGameBootstrap] Đã gắn EssayResultController vào Scene {sceneName}.");
                        }
                    }
                    else
                    {
                        if (Object.FindAnyObjectByType<QuizResultController>() == null)
                        {
                            GameObject anchor = FindSceneCanvasOrCreateAnchor("QuizResultController", sceneName);
                            anchor.AddComponent<QuizResultController>();
                            Debug.Log($"[PhysicsGameBootstrap] Đã gắn QuizResultController vào Scene {sceneName}.");
                        }
                    }
                    break;'''

replacement = '''                case PhysicsSceneNames.QuizResult: // "19. Result_TN"
                    bool isEssayOnQuizScene = (NavigationService.Instance != null && NavigationService.Instance.Selection.Mode == "ESSAY");
                    if (isEssayOnQuizScene)
                    {
                        if (Object.FindAnyObjectByType<EssayResultController>() == null)
                        {
                            GameObject anchor = FindSceneCanvasOrCreateAnchor("EssayResultController", sceneName);
                            anchor.AddComponent<EssayResultController>();
                            Debug.Log($"[PhysicsGameBootstrap] Đã gắn EssayResultController vào Scene {sceneName}.");
                        }
                    }
                    else
                    {
                        if (Object.FindAnyObjectByType<QuizResultController>() == null)
                        {
                            GameObject anchor = FindSceneCanvasOrCreateAnchor("QuizResultController", sceneName);
                            anchor.AddComponent<QuizResultController>();
                            Debug.Log($"[PhysicsGameBootstrap] Đã gắn QuizResultController vào Scene {sceneName}.");
                        }
                    }
                    break;

                case PhysicsSceneNames.WrittenResult: // "19.1. Result_TL"
                    bool isQuizOnEssayScene = (NavigationService.Instance != null && NavigationService.Instance.Selection.Mode == "QUIZ");
                    if (isQuizOnEssayScene)
                    {
                        if (Object.FindAnyObjectByType<QuizResultController>() == null)
                        {
                            GameObject anchor = FindSceneCanvasOrCreateAnchor("QuizResultController", sceneName);
                            anchor.AddComponent<QuizResultController>();
                            Debug.Log($"[PhysicsGameBootstrap] Đã gắn QuizResultController vào Scene {sceneName}.");
                        }
                    }
                    else
                    {
                        if (Object.FindAnyObjectByType<EssayResultController>() == null)
                        {
                            GameObject anchor = FindSceneCanvasOrCreateAnchor("EssayResultController", sceneName);
                            anchor.AddComponent<EssayResultController>();
                            Debug.Log($"[PhysicsGameBootstrap] Đã gắn EssayResultController vào Scene {sceneName}.");
                        }
                    }
                    break;'''

content_norm = content.replace('\r\n', '\n')
target_norm = target.replace('\r\n', '\n')
replacement_norm = replacement.replace('\r\n', '\n')

if target_norm in content_norm:
    new_content = content_norm.replace(target_norm, replacement_norm)
    with open('Assets/Scripts/PhysicsGame2/Core/PhysicsGameBootstrap.cs', 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(new_content)
    print('SUCCESS')
else:
    print('TARGET NOT FOUND')
