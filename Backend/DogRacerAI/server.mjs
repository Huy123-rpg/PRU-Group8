import http from 'node:http';
import { createHash } from 'node:crypto';
import { GradingError, validateSubmission, gradeSubmission } from './grading.mjs';

const port = Number(process.env.DOGRACER_AI_PORT || 8787);
const apiKey = process.env.GEMINI_API_KEY?.trim();
const model = process.env.GEMINI_MODEL || 'gemini-3.5-flash-lite';
const cache = new Map();
const requests = new Map();
const server = http.createServer(async (req, res) => {
  const reply = (status, value) => {
    if (!res.destroyed && !res.writableEnded) {
      res.writeHead(status, { 'Content-Type': 'application/json; charset=utf-8', 'Cache-Control': 'no-store' });
      res.end(JSON.stringify(value));
    }
  };
  const origin = req.headers.origin;
  if (origin && !/^https?:\/\/(localhost|127\.0\.0\.1)(:\d+)?$/.test(origin)) return reply(403, { error: 'ORIGIN_NOT_ALLOWED' });
  if (origin) {
    res.setHeader('Access-Control-Allow-Origin', origin);
    res.setHeader('Vary', 'Origin');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
  }
  if (req.method === 'OPTIONS') { res.writeHead(204); return res.end(); }
  if (req.method === 'GET' && req.url === '/health')
    return reply(200, { service: 'dogracer-ai', version: 1, configured: Boolean(apiKey), model });
  if (req.method !== 'POST' || req.url !== '/grade-essay') return reply(404, { error: 'NOT_FOUND' });
  if (!req.headers['content-type']?.startsWith('application/json')) return reply(415, { error: 'JSON_REQUIRED' });
  const controller = new AbortController();
  res.on('close', () => { if (!res.writableEnded) controller.abort(); });
  try {
    let size = 0;
    const chunks = [];
    for await (const chunk of req) {
      size += chunk.length;
      if (size > 32768) throw new GradingError('SUBMISSION_TOO_LARGE', 413);
      chunks.push(chunk);
    }
    let body;
    try { body = JSON.parse(Buffer.concat(chunks).toString('utf8')); } catch { throw new GradingError('INVALID_JSON', 400); }
    const submission = validateSubmission(body);
    const hash = createHash('sha256').update(JSON.stringify(submission)).digest('hex');
    const now = Date.now();
    for (const [key, entry] of cache) if (now - entry.time > 15 * 60 * 1000) cache.delete(key);
    const previous = cache.get(submission.requestId);
    if (previous) {
      if (previous.hash !== hash) throw new GradingError('REQUEST_ID_CONFLICT', 409);
      return reply(200, await previous.promise);
    }
    const client = req.socket.remoteAddress;
    const times = (requests.get(client) || []).filter(time => now - time < 60000);
    if (times.length >= 20) throw new GradingError('LOCAL_RATE_LIMIT', 429);
    requests.set(client, [...times, now]);
    if (cache.size >= 500) cache.delete(cache.keys().next().value);
    const promise = gradeSubmission(submission, { apiKey, model, signal: controller.signal });
    cache.set(submission.requestId, { time: now, hash, promise });
    try { reply(200, await promise); }
    catch (error) { cache.delete(submission.requestId); throw error; }
  } catch (error) {
    reply(error instanceof GradingError ? error.status : 500,
      { error: error instanceof GradingError ? error.code : 'SERVER_ERROR' });
  }
});
server.requestTimeout = 60000;
server.headersTimeout = 10000;
server.listen(port, '127.0.0.1', () => console.log(`DogRacer AI ready on 127.0.0.1:${port}; model=${model}; configured=${Boolean(apiKey)}`));
server.on('error', error => { console.error(`Backend startup failed: ${error.code || 'UNKNOWN'}`); process.exitCode = 1; });
