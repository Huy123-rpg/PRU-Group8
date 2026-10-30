// Real UDP/TCP integration test against the open Unity Editor. Node 18+.
// Run: node Tools/test_dogracer_lan.mjs
// Temporarily enters Play Mode, uses the live MC sheet and accelerates race speed.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import dgram from 'node:dgram';
import net from 'node:net';
import assert from 'node:assert/strict';

const registry = JSON.parse(fs.readFileSync(path.join(os.homedir(), '.unity_skills/registry.json'), 'utf8'));
const project = registry['D:\\GameProject\\PRU-Group8'];
assert(project?.status === 'running', 'UnitySkills must be running');
const base = `http://127.0.0.1:${project.port}`;
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));
async function request(route, body) {
  const response = await fetch(base + route, { method: body ? 'POST' : 'GET',
    headers: { 'Content-Type': 'application/json' }, body: body ? JSON.stringify(body) : undefined,
    signal: AbortSignal.timeout(55000) });
  const json = await response.json();
  assert(response.ok, JSON.stringify(json));
  return json;
}
async function skill(name, body) {
  const response = await request(`/skill/${name}?expectInstance=${project.id}`, body);
  assert.equal(response.status, 'success', JSON.stringify(response));
  return response.result;
}
async function invoke(methodName, values = [], parameterTypes = [], componentType = 'DogRaceLanSession', isStatic = false) {
  const body = { componentType, methodName, isStatic, arguments: JSON.stringify(values), parameterTypes: JSON.stringify(parameterTypes) };
  if (!isStatic) body.name = 'DogRace LAN';
  const { jobId } = await skill('component_invoke_method', body);
  for (let i = 0; i < 15; i++) {
    const result = await request(`/jobs/${jobId}?expectInstance=${project.id}&wait=2`);
    if (!result.terminal) continue;
    assert.equal(result.status, 'completed', JSON.stringify(result));
    return result.resultData?.returnValue;
  }
  throw Error(`Job timed out: ${methodName}`);
}

const udp = dgram.createSocket({ type: 'udp4', reuseAddr: true });
let peer, queue = [], socketError, ended = false;
let mockServer, offerTimer, offerSender;
const clientMode = process.argv.includes('--client') || process.argv.includes('--disconnect');
function send(type, fields = {}) {
  peer.write(JSON.stringify({ protocol: 'DOG_RACE_LAN_1', type, ...fields }) + '\n');
}
async function waitFor(predicate, timeout = 30000) {
  const deadline = Date.now() + timeout;
  while (Date.now() < deadline) {
    if (socketError) throw socketError;
    while (queue.length) {
      const message = queue.shift();
      assert.notEqual(message.type, 'cancel', JSON.stringify(message));
      if (predicate(message)) return message;
    }
    assert(!ended, 'Peer disconnected');
    send('ping');
    await delay(100);
  }
  throw Error('Timed out waiting for network state');
}
let originalScene, enteredPlay = false, changedScene = false;
try {
  assert.equal((await skill('editor_get_state', {})).isPlaying, false, 'Stop Play Mode first');
  originalScene = await skill('scene_get_info', {});
  assert(!originalScene.isDirty, 'Save the open scene before integration testing');
  await skill('scene_load', { scenePath: 'Assets/Scenes/Scene5_PlayMode.unity' });
  changedScene = true;
  await new Promise(resolve => udp.bind(47778, resolve));
  let offer;
  udp.on('message', data => {
    const message = JSON.parse(data.toString());
    if (message.protocol === 'DOG_RACE_LAN_1' && message.type === 'offer') offer = message;
  });
  await skill('editor_play', {});
  enteredPlay = true;
  console.log('Entering Play Mode from mode scene');
  await delay(5000);
  await invoke('BeginSearch');
  for (let i = 0; !offer && i < 150; i++) await delay(100);
  assert(offer, 'No UDP discovery offer');
  console.log('PASS UDP discovery', offer.port);
  if (clientMode) {
    mockServer = net.createServer();
    await new Promise(resolve => mockServer.listen(0, '0.0.0.0', resolve));
    // A separate source port avoids Windows suppressing same-socket broadcast loopback.
    udp.close();
    offerSender = dgram.createSocket('udp4');
    await new Promise(resolve => offerSender.bind(0, resolve));
    offerSender.setBroadcast(true);
    const hostOffer = Buffer.from(JSON.stringify({ ...offer, room: '00000000000000000000000000000000', port: mockServer.address().port }));
    offerTimer = setInterval(() => {
      offerSender.send(hostOffer, 47778, '255.255.255.255');
      offerSender.send(hostOffer, 47778, '127.0.0.1');
    }, 250);
    peer = await new Promise((resolve, reject) => {
      const timer = setTimeout(() => reject(Error('Unity did not join discovered host')), 15000);
      mockServer.once('connection', socket => { clearTimeout(timer); resolve(socket); });
    });
    clearInterval(offerTimer);
  } else peer = net.createConnection({ host: '127.0.0.1', port: offer.port });
  let buffer = '';
  peer.setEncoding('utf8');
  peer.on('error', error => { socketError = error; });
  peer.on('end', () => { ended = true; });
  peer.on('data', data => {
    buffer += data;
    let end;
    while ((end = buffer.indexOf('\n')) >= 0) {
      queue.push(JSON.parse(buffer.slice(0, end)));
      buffer = buffer.slice(end + 1);
    }
  });
  if (!clientMode) await new Promise((resolve, reject) => { peer.once('connect', resolve); peer.once('error', reject); });
  if (clientMode) {
    await waitFor(m => m.type === 'hello');
    send('welcome');
    const state = { dogs: [1, 0], ready: [false, false], progress: [0, 0], scores: [0, 0],
      correct: [0, 0], wrong: [0, 0], boost: [0, 0], elapsed: 0, countdown: -1,
      sequence: 0, prompt: 'Câu hỏi thử nghiệm giao thức LAN', options: ['A', 'B', 'C', 'D'], text: '' };
    send('state', state);
    await delay(400);
    if (process.argv.includes('--disconnect')) {
      peer.end();
      await delay(1500);
      assert.equal((await skill('scene_get_info', {})).sceneName, 'Scene5_PlayMode');
      console.log('PASS disconnect cancels session and returns to mode scene');
    } else {
      await invoke('SelectDog', [2], ['System.Int32']);
      assert.equal((await waitFor(m => m.type === 'dog')).dog, 2);
      state.dogs[1] = 2; send('state', state); await delay(200);
      await invoke('Ready');
      await waitFor(m => m.type === 'ready');
      send('load');
      await waitFor(m => m.type === 'loaded');
      state.countdown = 0; state.ready = [true, true]; send('state', state);
      await delay(500);
      await invoke('Answer', [1], ['System.Int32']);
      const answer = await waitFor(m => m.type === 'answer');
      assert.equal(answer.option, 1); assert.equal(answer.sequence, 0);
      state.sequence = 1; state.scores[1] = 100; state.correct[1] = 1;
      state.progress = [1, .8]; state.elapsed = 10; send('state', state);
      send('finish', { winner: 0 });
      await delay(800);
      assert.equal((await skill('scene_get_info', {})).sceneName, 'Scene7_Results');
      peer.end(); await delay(500);
      assert.equal((await skill('scene_get_info', {})).sceneName, 'Scene7_Results');
      console.log('PASS client discovery, dog/ready commands, loading, answer submission and host result');
      console.log('PASS results remain visible after opponent leaves');
    }
  } else {
  send('hello', { context: offer.context, room: 'integration-peer' });
  await waitFor(m => m.type === 'welcome');
  await invoke('SelectDog', [1], ['System.Int32']);
  send('dog', { dog: 1 });
  await delay(300);
  queue = [];
  const locked = await waitFor(m => m.type === 'state' && m.dogs[0] === 1);
  assert.equal(locked.dogs[1], 0);
  send('dog', { dog: 2 });
  await waitFor(m => m.type === 'state' && m.dogs[0] === 1 && m.dogs[1] === 2);
  console.log('PASS exclusive dog ownership');
  await invoke('Ready');
  send('ready');
  await waitFor(m => m.type === 'load', 60000);
  send('loaded');
  const first = await waitFor(m => m.type === 'state' && m.countdown === 0 && m.prompt);
  const local = JSON.parse(await invoke('GetDiagnosticState'));
  assert.notEqual(local.prompt, first.prompt);
  console.log('PASS synchronized start and distinct questions');
  const later = await waitFor(m => m.type === 'state' && m.elapsed > first.elapsed + .5);
  const expectedSpeed = Number(fs.readFileSync('Assets/Scenes/Scene6_Race.unity', 'utf8').match(/\n  playerBaseSpeed: ([\d.]+)/)[1]);
  const measuredSpeed = (later.progress[0] - first.progress[0]) / (later.elapsed - first.elapsed);
  assert(Math.abs(measuredSpeed - expectedSpeed) < .0001, `PvP speed ${measuredSpeed}, solo speed ${expectedSpeed}`);
  console.log('PASS PvP base speed matches solo scene', measuredSpeed);
  await invoke('ConfigureRace', [.25, .01, 1], ['System.Single', 'System.Single', 'System.Single']);
  await delay(400);
  send('answer', { sequence: first.sequence, option: 0 });
  const accepted = await waitFor(m => m.type === 'state' && m.sequence === first.sequence + 1);
  assert.equal(accepted.correct[1] + accepted.wrong[1], 1);
  assert.equal(accepted.boost[1] > 0, accepted.correct[1] === 1);
  send('answer', { sequence: first.sequence, option: 0 });
  const after = await waitFor(m => m.type === 'state' && m.sequence === accepted.sequence);
  assert.equal(after.correct[1] + after.wrong[1], 1);
  console.log('PASS host grading and duplicate-answer rejection');
  const finished = await waitFor(m => m.type === 'finish');
  assert([-1, 0, 1].includes(finished.winner));
  let resultScene;
  for (let i = 0; i < 30; i++) {
    resultScene = (await skill('scene_get_info', {})).sceneName;
    if (resultScene === 'Scene7_Results') break;
    await delay(200);
  }
  assert.equal(resultScene, 'Scene7_Results');
  console.log('PASS authoritative finish and results scene', finished.winner);
  }
} finally {
  clearInterval(offerTimer); mockServer?.close(); offerSender?.close();
  peer?.destroy();
  try { udp.close(); } catch (error) { if (error.code !== 'ERR_SOCKET_DGRAM_NOT_RUNNING') throw error; }
  try { if (enteredPlay) await skill('editor_stop', {}); }
  finally { if (changedScene && originalScene?.scenePath) await skill('scene_load', { scenePath: originalScene.scenePath }); }
}
