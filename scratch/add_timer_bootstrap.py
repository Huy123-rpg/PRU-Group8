import sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scripts/PhysicsGame2/Core/PhysicsGameBootstrap.cs', 'r', encoding='utf-8') as f:
    text = f.read()

target = """                    }
                    break;
            }
        }
    }
}"""

replacement = """                    }
                    break;
            }

            AttachSessionTimerIfApplicable(sceneName);
        }

        private static void AttachSessionTimerIfApplicable(string sceneName)
        {
            if (sceneName == PhysicsSceneNames.Theory ||
                sceneName == PhysicsSceneNames.Quiz ||
                sceneName == PhysicsSceneNames.WrittenAnswer ||
                sceneName == PhysicsSceneNames.ModeSelection ||
                sceneName == PhysicsSceneNames.DifficultySelection ||
                sceneName == PhysicsSceneNames.QuizResult ||
                sceneName == PhysicsSceneNames.WrittenResult ||
                sceneName == PhysicsSceneNames.QuizReview ||
                sceneName == PhysicsSceneNames.WrittenReview)
            {
                if (UnityEngine.Object.FindAnyObjectByType<StudySessionTimerUI>() == null)
                {
                    GameObject anchor = FindSceneCanvasOrCreateAnchor("StudySessionTimerUI", sceneName);
                    anchor.AddComponent<StudySessionTimerUI>();
                }
            }
        }
    }
}"""

text_norm = text.replace('\r\n', '\n')
target_norm = target.replace('\r\n', '\n')
rep_norm = replacement.replace('\r\n', '\n')

if target_norm in text_norm:
    text_norm = text_norm.replace(target_norm, rep_norm)
    with open('Assets/Scripts/PhysicsGame2/Core/PhysicsGameBootstrap.cs', 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(text_norm)
    print('Inserted AttachSessionTimerIfApplicable successfully.')
else:
    print('target not found in file!')
