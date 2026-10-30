import test from 'node:test';
import assert from 'node:assert/strict';
import { validateSubmission, gradeSubmission, parseGrade } from './grading.mjs';

const submission = validateSubmission({ requestId: 'test-1', question: 'HCl tác dụng với Zn thế nào?', studentAnswer: 'Zn tan và có khí H2.' });
const response = grade => ({ candidates: [{ finishReason: 'STOP', content: { parts: [{ text: JSON.stringify(grade) }] } }] });

test('rejects blank, oversized and wrong-subject submissions before contacting AI', () => {
  for (const body of [null, { question: 'Q', studentAnswer: '  ' },
    { question: 'Q', studentAnswer: 'x'.repeat(2001) },
    { question: 'Q', studentAnswer: 'A', subject: 'Biology' }])
    assert.throws(() => validateSubmission(body));
});

test('rejects malformed, missing, non-finite and out-of-range grades', () => {
  for (const grade of [{}, { score: 11, feedback: 'F', modelAnswer: 'A' },
    { score: '10', feedback: 'F', modelAnswer: 'A' }, { score: 8, feedback: '', modelAnswer: 'A' }])
    assert.throws(() => parseGrade(response(grade), 'id', 'model'), /AI_RESPONSE_INVALID/);
  assert.throws(() => parseGrade({ candidates: [{ finishReason: 'SAFETY' }] }, 'id', 'model'));
});

test('derives pass/fail from validated score and does not trust an injected isCorrect', () => {
  const result = parseGrade(response({ score: 0, isCorrect: true, feedback: 'Sai.', modelAnswer: 'Đáp án đúng.' }), 'id', 'model');
  assert.equal(result.isCorrect, false);
  assert.equal(result.score, 0);
});

test('quota failure never becomes a grade and is not automatically retried', async () => {
  let calls = 0;
  await assert.rejects(gradeSubmission(submission, { apiKey: 'fake-test-key', fetchImpl: async () => {
    calls++; return new Response('{}', { status: 429 });
  } }), /AI_QUOTA_EXCEEDED/);
  assert.equal(calls, 1);
});

test('retries one transient provider failure and returns validated structured data', async () => {
  let calls = 0;
  const result = await gradeSubmission(submission, { apiKey: 'fake-test-key', fetchImpl: async () => {
    calls++;
    return calls === 1 ? new Response('{}', { status: 503 }) : Response.json(response({ score: 8, feedback: 'Đúng ý.', modelAnswer: 'Zn + 2HCl -> ZnCl2 + H2.' }));
  } });
  assert.equal(calls, 2);
  assert.equal(result.requestId, submission.requestId);
  assert.equal(result.score, 8);
  assert.equal(result.maxScore, 10);
});
