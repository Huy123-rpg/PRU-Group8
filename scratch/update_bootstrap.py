import sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scripts/PhysicsGame2/Core/PhysicsGameBootstrap.cs', 'r', encoding='utf-8') as f:
    text = f.read()

old_block = """                case PhysicsSceneNames.QuizResult: // "19.1. Result_TL" (Title: TRẮC NGHIỆM)
                    if (Object.FindAnyObjectByType<QuizResultController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("QuizResultController", sceneName);
                        anchor.AddComponent<QuizResultController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn QuizResultController vào Scene 19.1. Result_TL.");
                    }
                    break;

                case PhysicsSceneNames.WrittenResult: // "19. Result_TN" (Title: TỰ LUẬN)
                    if (Object.FindAnyObjectByType<EssayResultController>() == null)
                    {
                        GameObject anchor = FindSceneCanvasOrCreateAnchor("EssayResultController", sceneName);
                        anchor.AddComponent<EssayResultController>();
                        Debug.Log("[PhysicsGameBootstrap] Đã gắn EssayResultController vào Scene 19. Result_TN.");
                    }
                    break;"""

new_block = """                case PhysicsSceneNames.QuizResult: // "19.1. Result_TL"
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
                    break;"""

# Normalize CRLF
text_norm = text.replace('\r\n', '\n')
old_norm = old_block.replace('\r\n', '\n')
new_norm = new_block.replace('\r\n', '\n')

if old_norm in text_norm:
    text_norm = text_norm.replace(old_norm, new_norm)
    with open('Assets/Scripts/PhysicsGame2/Core/PhysicsGameBootstrap.cs', 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(text_norm)
    print('Replaced successfully.')
else:
    print('old_norm not found in file!')
