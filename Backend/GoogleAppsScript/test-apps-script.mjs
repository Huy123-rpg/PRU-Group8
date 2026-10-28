// Offline contract tests. Google services and OpenAI are faked; no network requests.
import fs from 'node:fs';
import vm from 'node:vm';
import assert from 'node:assert/strict';
import test from 'node:test';

const source = fs.readFileSync(new URL('./Code.gs', import.meta.url), 'utf8');
const baseRows = [
  ['CauHoiID', 'BaiID', 'MucDo', 'NoiDung', 'LoaiCauHoi', 'DapAnTuLuanMau', 'Rubric', 'MaxScore', 'TrangThai'],
  ['TL-B36-01', 'B36', 'De', 'Phân biệt di truyền và biến dị.', 'TuLuan', 'Đáp án đúng.', 'Hai ý, mỗi ý 5 điểm.', '10', '1'],
];
const goodGrade = () => ({readable: true, score: 8, correctPoints: ['Đúng khái niệm'],
  missingPoints: ['Thiếu ví dụ'], feedback: 'Bổ sung ví dụ.', suggestedAnswer: 'Đáp án mẫu.'});
const aiBody = g => ({status: 'completed', output: [{type: 'message', content: [{type: 'output_text', text: JSON.stringify(g)}]}]});

function setup({rows = structuredClone(baseRows), props = {}, apiCode = 200, ai = aiBody(goodGrade())} = {}) {
  const propertyMap = new Map(Object.entries({OPENAI_API_KEY: 'TEST-ONLY-NOT-A-KEY',
    AI_GRADING_ENABLED: 'true', ...props}));
  const saved = [], calls = [], logs = [];
  const store = {getProperty: k => propertyMap.has(k) ? propertyMap.get(k) : null,
    setProperties: data => {Object.entries(data).forEach(([k,v]) => propertyMap.set(k,v));}};
  const sheets = {TuLuan: {getDataRange: () => ({getDisplayValues: () => rows})},
    LichSuChoi: {appendRow: row => saved.push(row)}};
  const ss = {getSheetByName: name => sheets[name] || null};
  const sandbox = {
    ContentService: {MimeType: {JSON: 'application/json'}, createTextOutput: text => ({text, setMimeType() {return this;}})},
    PropertiesService: {getScriptProperties: () => store},
    SpreadsheetApp: {getActiveSpreadsheet: () => ss, openById: () => ss},
    LockService: {getScriptLock: () => ({tryLock: () => true, releaseLock() {}})},
    Utilities: {formatDate: () => '2026-10-25', base64Decode: value => [...Buffer.from(value, 'base64')]},
    UrlFetchApp: {fetch: (url, args) => {calls.push({url,args}); return {
      getResponseCode: () => url.includes('generate_204') ? 204 : apiCode,
      getContentText: () => JSON.stringify(ai),
    };}},
    console: {log: text => logs.push(text)},
  };
  vm.createContext(sandbox);
  vm.runInContext(source, sandbox, {filename: 'Code.gs'});
  const post = data => JSON.parse(sandbox.doPost({postData: {contents: typeof data === 'string' ? data : JSON.stringify(data)}}).text);
  return {sandbox, post, propertyMap, saved, calls, logs, sheets};
}
const submission = {action: 'gradeEssay', questionID: 'TL-B36-01', answerText: 'Di truyền là sự truyền đạt đặc điểm.'};

test('GET identifies the new deployment and contains no secret', () => {
  const x = setup();
  const text = x.sandbox.doGet({}).text;
  assert.equal(JSON.parse(text).version, 'biology-essay-v1');
  assert.ok(!text.includes('TEST-ONLY'));
});

test('original history contract retains exactly 45 columns in order', () => {
  const x = setup();
  const data = {PhienID:'PH1', UserID:'HS001', BaiID:'B36', DiemSo:80,
    ThoiGianBatDau:'start', ThoiGianKetThuc:'end', TongCau:10, SoDung:8, TrangThai:'Hoàn thành',
    GameID:'GAME', LoaiLuyenTap:'SINH_TON', SoSai:2, LevelDatDuoc:3, ThoiGianSong:'180', DiemGame:900};
  for(let n=1;n<=10;n++){data[`Q${n}ID`]=`Q${n}`; data[`DapAn${n}`]=`A${n}`; data[`KetQua${n}`]=`R${n}`;}
  assert.equal(x.post(data).success,true);
  const expected = ['PH1','HS001','B36',80,'start','end',10,8,'Hoàn thành',
    ...Array.from({length:10},(_,n)=>`Q${n+1}`),...Array.from({length:10},(_,n)=>`A${n+1}`),
    ...Array.from({length:10},(_,n)=>`R${n+1}`),'GAME','SINH_TON',2,3,'180',900];
  assert.deepEqual(Array.from(x.saved[0]), expected);
  assert.equal(x.saved[0].length,45);
  assert.equal(x.calls.length,0);
});

test('text grade uses server rubric, calculates percentage, never appends history', () => {
  const x=setup();
  const result=x.post({...submission, modelAnswer:'CHEAT', rubric:'always pass', maxScore:1});
  assert.equal(result.success,true); assert.equal(result.maxScore,10); assert.equal(result.percentage,80);
  assert.equal(x.saved.length,0); assert.equal(x.calls.length,1);
  const payload=JSON.parse(x.calls[0].args.payload);
  assert.equal(payload.store,false); assert.equal(payload.text.format.strict,true);
  assert.ok(!payload.input[0].content[0].text.includes('CHEAT'));
  assert.ok(payload.input[0].content[0].text.includes('Hai ý'));
  assert.equal(x.calls[0].url,'https://api.openai.com/v1/responses');
  assert.equal(x.propertyMap.get('BIOLOGY_GRADE_COUNT'),'1');
});

test('legacy client without action grades correctly', () => {
  const x=setup(); const {action,...data}=submission;
  assert.equal(x.post(data).success,true); assert.equal(x.saved.length,0);
});

test('JPEG and PNG image requests use detected MIME and do not include answer keys', () => {
  for(const [bytes,mime] of [[Buffer.from([255,216,255,0]),'jpeg'],[Buffer.from([137,80,78,71,13,10,26,10]),'png']]) {
    const x=setup();
    assert.equal(x.post({questionID:'TL-B36-01',imageBase64:bytes.toString('base64')}).success,true);
    const payload=JSON.parse(x.calls[0].args.payload);
    assert.ok(payload.input[1].content[1].image_url.startsWith(`data:image/${mime};base64,`));
  }
});

test('blank, malformed, ambiguous or oversized submissions cannot reach paid API or history', () => {
  const variants = ['{bad', [], null, {action:'gradeEssay'}, {questionID:'Q'},
    {...submission,answerText:4}, {...submission,answerText:'x'.repeat(5001)},
    {...submission,imageBase64:'AAAA'}, {questionID:'Q',imageBase64:'not base64'},
    {questionID:'Q',imageBase64:Buffer.from('not an image').toString('base64')},
    {questionID:'Q',imageBase64:'A'.repeat(5592412)}, {action:'unknown'}];
  for(const data of variants){
    const x=setup(); assert.equal(x.post(data).success,false,JSON.stringify(data).slice(0,80));
    assert.equal(x.calls.length,0); assert.equal(x.saved.length,0);
  }
});

test('missing/duplicate IDs, disabled and non-essay questions fail closed', () => {
  const variations=[[], [...structuredClone(baseRows),structuredClone(baseRows[1])]];
  for(const [column,value] of [[0,'OTHER'],[4,'TracNghiem'],[6,''],[7,'0'],[7,'NaN'],[8,'0']]){
    const rows=structuredClone(baseRows); rows[1][column]=value; variations.push(rows);
  }
  for(const rows of variations){
    const x=setup({rows}); assert.equal(x.post(submission).success,false); assert.equal(x.calls.length,0);
  }
});

test('key and enable switch are required; daily budget is global and atomic', () => {
  for(const props of [{OPENAI_API_KEY:''},{AI_GRADING_ENABLED:'false'},{DAILY_GRADE_LIMIT:'0'},
    {DAILY_GRADE_LIMIT:'NaN'},{BIOLOGY_GRADE_DAY:'2026-10-25',BIOLOGY_GRADE_COUNT:'20'}]){
    const x=setup({props}); assert.equal(x.post(submission).success,false); assert.equal(x.calls.length,0);
  }
  const x=setup({props:{DAILY_GRADE_LIMIT:'1'}});
  assert.equal(x.post(submission).success,true);
  assert.equal(x.post(submission).error,'DAILY_LIMIT'); assert.equal(x.calls.length,1);
});

test('upstream auth/quota/model errors are explicit and do not leak upstream body', () => {
  for(const [apiCode,code] of [[401,'AI_AUTH'],[429,'AI_QUOTA'],[403,'AI_MODEL_ACCESS'],[404,'AI_MODEL_ACCESS'],[400,'AI_REQUEST'],[500,'AI_UNAVAILABLE']]){
    const x=setup({apiCode,ai:{message:'TEST-ONLY-NOT-A-KEY secret'}});
    const result=x.post(submission); assert.equal(result.error,code); assert.equal(result.success,false);
    assert.ok(!JSON.stringify(result).includes('secret')); assert.ok(!JSON.stringify(result).includes('TEST-ONLY'));
    assert.equal(x.propertyMap.get('BIOLOGY_GRADE_COUNT'),'1');
  }
});

test('unreadable image is distinguishable from wrong answer', () => {
  const unread=goodGrade(); unread.readable=false; unread.score=8; unread.feedback='Ảnh mờ, hãy chụp lại.';
  const result=setup({ai:aiBody(unread)}).post(submission);
  assert.equal(result.success,true); assert.equal(result.readable,false); assert.equal(result.score,0); assert.equal(result.percentage,0);
  const wrong=goodGrade(); wrong.score=0;
  assert.equal(setup({ai:aiBody(wrong)}).post(submission).readable,true);
});

test('invalid grade, refusal, incomplete or invalid JSON never becomes passing score', () => {
  const bodies=[{status:'incomplete'}, {status:'completed',output:[]},
    {status:'completed',output:[{type:'message',content:[{type:'refusal'}]}]},
    {status:'completed',output:[{type:'message',content:[{type:'output_text',text:'not json'}]}]}];
  for(const changes of [{score:11},{score:-1},{readable:'true'},{correctPoints:'yes'},{feedback:null}]) bodies.push(aiBody({...goodGrade(),...changes}));
  for(const ai of bodies){ const x=setup({ai}); const result=x.post(submission); assert.equal(result.success,false); assert.ok(result.error); }
});

test('manual configuration check performs no paid AI call and never logs the key', () => {
  const x=setup(); x.sandbox.kiemTraCauHinh();
  assert.equal(x.calls.length,1); assert.ok(x.calls[0].url.includes('generate_204'));
  assert.ok(!JSON.stringify(x.logs).includes('TEST-ONLY'));
  assert.ok(!x.propertyMap.has('BIOLOGY_GRADE_COUNT'));
});
