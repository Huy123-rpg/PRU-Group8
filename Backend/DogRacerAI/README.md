# DogRacer AI grading

Windows + Node.js 20 or newer. No npm dependencies.

1. Run `Configure-Key.ps1` to enter a Gemini API key privately. Windows encrypts it for the current user in `.local/gemini-key.dpapi`, which is excluded from Git.
2. Run `Start-DogRacerAI.ps1`. The server starts hidden at `http://127.0.0.1:8787`. Unity Editor also starts it automatically when playing `Scene6_Race`.
3. Choose **Tự luận** in DogRacer. A question pauses the race. Submission remains paused during grading; a valid result updates score/lives, then racing resumes for five seconds. Service errors preserve the answer and allow retry without a penalty.

After changing a key with `Configure-Key.ps1` or changing the server code, run `Restart-DogRacerAI.ps1` to reload it. It verifies the local listener and process ID before stopping only this backend.

For a standalone Windows build, start the backend before playing. For other machines, deploy the backend separately and set the scene controller's `essayGradingEndpoint` to its HTTPS `/grade-essay` URL. Never put a Gemini key into Unity, a scene, PlayerPrefs or source control.

`GEMINI_API_KEY` can be supplied by the server environment instead of the Windows key store. Optional `GEMINI_MODEL` defaults to `gemini-3.5-flash-lite`. The local starter uses port 8787; hosted deployments should configure hosting and authentication separately.

The endpoint receives `{requestId, subject:"Chemistry", question, studentAnswer, referenceAnswer, rubric, maxScore:10}`. Success returns `{requestId, score, maxScore:10, isCorrect, feedback, modelAnswer, error:""}`. Scores of at least 5 pass. Race points equal `score / 10 * maxScorePerQuestion`; a failing grade costs one heart. Network/provider errors never award points or cost hearts. Repeated identical request IDs reuse the same successful grade for 15 minutes.

Use `node --test grading.test.mjs` for provider-independent validation/error-path tests. `/health` reports configuration status without revealing credentials. Logs under `.local/` contain startup status, not submissions or keys.
