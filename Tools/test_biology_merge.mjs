// Exercise actual Unity UI callbacks after integrating basck. Requires Node 18+ and UnitySkills bypass.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import assert from 'node:assert/strict';
const target = JSON.parse(fs.readFileSync(path.join(os.homedir(), '.unity_skills/registry.json'), 'utf8'))['D:\\GameProject\\PRU-Group8'];
assert.equal(target.status, 'running');
const base = `http://127.0.0.1:${target.port}`;
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));
async function request(route, body) {
  const r = await fetch(base + route, { method: body ? 'POST' : 'GET', headers: { 'Content-Type': 'application/json' },
    body: body ? JSON.stringify(body) : undefined, signal: AbortSignal.timeout(55000) });
  const j = await r.json(); assert(r.ok, JSON.stringify(j)); return j;
}
async function skill(name, body = {}) {
  const j = await request(`/skill/${name}?expectInstance=${target.id}`, body);
  assert.equal(j.status, 'success', j.error || name); return j.result;
}
async function invoke(type, method, name, args = [], types = [], isStatic = false) {
  const b = { componentType: type, methodName: method, isStatic, arguments: JSON.stringify(args), parameterTypes: JSON.stringify(types) };
  if (name) b.path = name;
  const dry = await request(`/skill/component_invoke_method?expectInstance=${target.id}&mode=dryRun`, b);
  assert(dry.valid, `${type}.${method}: invalid arguments`);
  const j = await skill('component_invoke_method', b);
  for (let i = 0; i < 25; i++) {
    const job = await request(`/jobs/${j.jobId}?expectInstance=${target.id}&wait=2`);
    if (!job.terminal) continue;
    assert.equal(job.status, 'completed', job.error); return job.resultData?.returnValue;
  }
  throw Error(`${method} timed out`);
}
async function tree() {
  const data = await skill('scene_get_hierarchy', { maxDepth: 12 });
  const flat = [];
  function add(n, parent = '') { n.path = parent ? `${parent}/${n.name}` : n.name; flat.push(n); for (const child of n.children || []) add(child, n.path); }
  for (const root of data.hierarchy) add(root);
  assert(!flat.some(n => n.components.some(c => !c || /missing/i.test(c))), `Missing component in ${data.sceneName}`);
  return flat;
}
async function component(type) {
  const found = (await tree()).find(n => n.components.includes(type));
  assert(found, `No ${type} component`); return found.path;
}
async function waitScene(name) {
  for (let i = 0; i < 40; i++) {
    if ((await skill('scene_get_info')).sceneName === name) { await frames(); return; }
    await frames();
  }
  throw Error(`Did not reach ${name}`);
}
async function frames() {
  const j = await skill('editor_playmode_step', { frames: 3 });
  for (let i = 0; i < 15; i++) {
    const job = await request(`/jobs/${j.jobId}?expectInstance=${target.id}&wait=2`);
    if (job.terminal) { assert.equal(job.status, 'completed', job.error); break; }
  }
  if ((await skill('editor_get_state')).isPaused) await skill('editor_pause');
}
async function load(name) {
  await invoke('UnityEngine.Time', 'set_timeScale', null, [1], ['System.Single'], true);
  await invoke('UnityEngine.SceneManagement.SceneManager', 'LoadScene', null, [name], ['System.String'], true);
  await waitScene(path.basename(name).replace('.unity', ''));
}
async function button(name) {
  const node = (await tree()).find(n => n.name === name && n.components.includes('Button'));
  assert(node, `No button ${name}`);
  await invoke('UnityEngine.UI.Button', 'OnSubmit', node.path, [null], ['UnityEngine.EventSystems.BaseEventData']);
  await delay(200);
}
async function chooseBiology() {
  await load('Scene2_MainMenu');
  const node = (await tree()).find(n => n.name === 'Bio Card'); assert(node);
  await invoke('SelectableCard', 'SelectThisCard', node.path);
  await frames();
  const opened = await tree();
  assert(opened.some(n => n.name === 'SinhGameSelectPanel'),
    'No biology panel; menu nodes: ' + opened.filter(n => /Menu|Sinh|Bio/i.test(n.name)).map(n => n.path).join(', '));
}

let original, changed = false, playing = false;
try {
  assert.equal((await skill('editor_get_state')).isPlaying, false, 'Stop Play first');
  original = await skill('scene_get_info'); assert(!original.isDirty, 'Save open scene first');
  await skill('scene_load', { scenePath: 'Assets/Scenes/Scene2_MainMenu.unity' }); changed = true;
  await skill('editor_play'); playing = true; console.log('Entering Biology merge Play Mode'); await delay(5000);
  const before = await skill('console_get_logs', { type: 'All', limit: 1 });
  const firstSeq = before.coverage.newestSeq;
  if (!process.argv.includes('--routes-only')) {
  await chooseBiology();
  await button('ObstacleRunnerCard'); await waitScene('ChapterList 1');
  console.log('PASS subject menu -> Bach game selector -> obstacle chapter list');
  const chapterLabels = [];
  for (let i = 1; i <= 5; i++) {
    if (i > 1) await load('ChapterList 1');
    await button(`Btn_Chuong${i}`); await waitScene('LessonList 1');
    const nodes = await tree();
    const lesson = nodes.find(n => n.name === 'btn_bai2'); assert(lesson);
    const label = nodes.find(n => n.path.startsWith(lesson.path + '/') && n.components.includes('TextMeshProUGUI')); assert(label);
    chapterLabels.push(await invoke('TMPro.TextMeshProUGUI', 'get_text', label.path));
  }
  assert.equal(new Set(chapterLabels).size, 5, 'Chapters must have different lessons');
  console.log('PASS all 5 chapters show their own lesson titles');
  for (const mode of ['Image_tracnghiem', 'Image_tuluan']) {
    if ((await skill('scene_get_info')).sceneName !== 'LessonList 1') await load('LessonList 1');
    await button('btn_bai2'); await button(mode); await button('Image_de'); await waitScene('SinhScene');
    assert(await component('RunnerController'));
    const quiz = await component('QuizManager');
    await invoke('QuizManager', 'ShowQuiz', quiz, [null], ['UnityEngine.GameObject']);
    const popup = await skill('gameobject_get_info', { name: mode === 'Image_tuluan' ? 'paneltuluan' : 'paneltracno' });
    assert(popup.isActive, `Quiz popup not visible for ${mode}`);
    console.log(`PASS obstacle gameplay, runner and boss popup: ${mode}`);
  }
  for (const essay of [false, true]) {
    await chooseBiology(); await button('SurvivalCard'); await waitScene('MainMenu');
    let menu = await component('MainMenuController');
    await invoke('MainMenuController', 'OpenChapter', menu, ['C11'], ['System.String']); await waitScene('LessonSelect');
    menu = await component('MainMenuController');
    await invoke('MainMenuController', 'OpenLesson', menu, ['B39'], ['System.String']); await waitScene('ModeSelect');
    menu = await component('MainMenuController');
    await invoke('MainMenuController', essay ? 'StartEssay' : 'StartMultipleChoice', menu);
    await waitScene(essay ? 'EssayMaze' : 'SinhHoc'); await tree();
    console.log(`PASS survival menu -> chapter -> lesson -> ${essay ? 'EssayMaze' : 'SinhHoc'}`);
  }
  // Exercise the teacher route without persisting any account or submitting grades.
  await load('Scene2_MainMenu');
  const session = await component('GameSession');
  await skill('component_set_serialized_property', { path: session, componentType: 'GameSession', propertyPath: 'currentUser.username', value: 'teacher' });
  await skill('component_set_serialized_property', { path: session, componentType: 'GameSession', propertyPath: 'currentUser.role', value: 'Teacher' });
  await chooseBiology(); await button('ObstacleRunnerCard'); await waitScene('TeacherCenter');
  assert(await component('TeacherManager'));
  await invoke('ScienceQuest.Biology.TeacherNavigation', 'GoToSubjectSelect', null, [], [], true); await waitScene('Scene2_MainMenu');
  console.log('PASS teacher biology center and return to subject menu');
  }
  for (const [card, destination] of [['ChemCard', 'Scene3_LessonType'], ['PhyCard', 'GameSelection']]) {
    await load('Scene2_MainMenu');
    const node = (await tree()).find(n => n.name === card); assert(node);
    await invoke('SelectableCard', 'SelectThisCard', node.path);
    await waitScene(destination);
    console.log(`PASS existing subject route: ${card} -> ${destination}`);
  }
  const logs = await skill('console_get_logs', { type: 'All', filter: 'Exception', includeStackTrace: true, limit: 100 });
  const newGameErrors = logs.logs.filter(l => l.lastSeq > firstSeq && /Assets[\\/]Scripts/.test(l.stackTrace || '') && !/Unity\.AI\./.test(l.stackTrace || ''));
  assert.equal(newGameErrors.length, 0, newGameErrors.map(x => x.message.split('\n')[0]).join('; '));
  console.log('PASS no new gameplay exceptions');
} finally {
  if (playing) {
    try { await invoke('UnityEngine.Time', 'set_timeScale', null, [1], ['System.Single'], true); }
    finally { await skill('editor_stop'); }
  }
  if (changed && original?.scenePath) await skill('scene_load', { scenePath: original.scenePath });
}
