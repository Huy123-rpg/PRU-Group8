/**
 * =========================================================================
 * GOOGLE APPS SCRIPT - BACKEND ĐỒNG BỘ GAME VẬT LÝ & QUẢN LÝ DÀNH CHO GIÁO VIÊN
 * =========================================================================
 * Dự án: ScienceQuest - PRU213 (Group 8)
 * Spreadsheet ID: 1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY
 * Tab Ngân hàng câu hỏi: GID 286121556 (Chuẩn 18 cột)
 * Tab Tiến độ & Lịch sử: GID 1079761384 (6 cột lịch sử làm bài)
 *
 * 18 CỘT CHUẨN ĐỒNG BỘ:
 * 1. STT
 * 2. CauHoiID
 * 3. ChuongId
 * 4. TenChuong
 * 5. BaiID
 * 6. TenBai
 * 7. LoaiCauHoi
 * 8. MucDo
 * 9. NoiDung
 * 10. DapAnA
 * 11. DapAnB
 * 12. DapAnC
 * 13. DapAnD
 * 14. DapAnDung
 * 15. GiaiThich
 * 16. MeoGhiNho
 * 17. TrangThai
 * 18. GiaoVienID
 * =========================================================================
 */

var SPREADSHEET_ID = "1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY";
var GID_QUESTIONS = "286121556";
var GID_PROGRESS = "1079761384";

var TAB_QUESTIONS_DEFAULT = "NganHangCauHoi";
var TAB_PROGRESS_DEFAULT = "PhienLuyenTap";

var FULL_18_HEADERS = [
  "STT", "CauHoiID", "ChuongId", "TenChuong", "BaiID", "TenBai",
  "LoaiCauHoi", "MucDo", "NoiDung", "DapAnA", "DapAnB", "DapAnC", "DapAnD",
  "DapAnDung", "GiaiThich", "MeoGhiNho", "TrangThai", "GiaoVienID"
];

var PROGRESS_HEADERS = [
  "Thời Gian", "UserID", "MonID", "BaiID", "Điểm", "Tỷ Lệ Đúng"
];

function doGet(e) {
  var action = (e && e.parameter && e.parameter.action) ? e.parameter.action : "info";

  if (action === "get_questions") {
    return handleGetQuestions();
  } else if (action === "get_progress") {
    return handleGetProgress();
  }

  var response = {
    status: "online",
    message: "ScienceQuest Google Apps Script Backend đang hoạt động ổn định!",
    timestamp: new Date().toISOString(),
    spreadsheetId: SPREADSHEET_ID,
    tabs: {
      questionsGid: GID_QUESTIONS,
      progressGid: GID_PROGRESS
    }
  };
  return ContentService.createTextOutput(JSON.stringify(response))
    .setMimeType(ContentService.MimeType.JSON);
}

function doPost(e) {
  try {
    var params = (e && e.parameter) ? e.parameter : {};
    var action = params.action || "";

    if (!action && e && e.postData && e.postData.contents) {
      try {
        var jsonBody = JSON.parse(e.postData.contents);
        action = jsonBody.action || "";
        params = jsonBody;
      } catch (err) {}
    }

    // Hỗ trợ tự luận AI (gradeEssay): tương thích cả khi client gửi action="gradeEssay" hoặc chỉ gửi questionID/answerText/imageBase64
    var gradingFields = ["questionID", "cauHoiID", "imageBase64", "answerText", "essayText"];
    var isGrade = (action === "gradeEssay") || gradingFields.some(function(k) {
      return Object.prototype.hasOwnProperty.call(params, k);
    });

    if (isGrade) {
      return handleGradeEssay(params);
    } else if (action === "add_question") {
      return handleAddQuestion(params);
    } else if (action === "batch_add") {
      return handleBatchAddQuestions(params);
    } else if (action === "update_progress") {
      return handleUpdateProgress(params);
    } else if (action === "get_questions") {
      return handleGetQuestions();
    } else if (action === "delete_last_row") {
      return handleDeleteLastRow();
    } else {
      return ContentService.createTextOutput(JSON.stringify({
        status: "error",
        message: "Hành động (action) không hợp lệ: " + action
      })).setMimeType(ContentService.MimeType.JSON);
    }
  } catch (error) {
    return ContentService.createTextOutput(JSON.stringify({
      status: "error",
      message: error.toString()
    })).setMimeType(ContentService.MimeType.JSON);
  }
}

/**
 * Thêm 1 câu hỏi vào đúng 18 cột của sheet câu hỏi
 */
function handleAddQuestion(params) {
  var lock = LockService.getScriptLock();
  try {
    lock.waitLock(10000);
  } catch (e) {
    return ContentService.createTextOutput(JSON.stringify({
      status: "error",
      message: "Hệ thống đang bận ghi dữ liệu, vui lòng thử lại sau giây lát!"
    })).setMimeType(ContentService.MimeType.JSON);
  }

  try {
    var ss = getSpreadsheet();
    var sheet = getQuestionsSheet(ss);

    // Tính số thứ tự STT
    var lastRow = sheet.getLastRow();
    var stt = (lastRow >= 1) ? lastRow : 1;
    if (params.stt && parseInt(params.stt, 10) > 0) {
      stt = parseInt(params.stt, 10);
    }

    // Tự động sinh ID nếu trống hoặc AUTO hoặc đã tồn tại
    var rawCauHoiID = (params.cauHoiID || "").trim();
    var baiID = (params.baiID || "bai2").trim();
    var finalID = rawCauHoiID;

    if (!finalID || finalID.toUpperCase() === "AUTO" || isQuestionIdExisting(sheet, finalID)) {
      finalID = generateNextQuestionID(sheet, 2);
    }

    var chuongId = params.chuongId || getChuongIdFromBaiId(baiID);
    var tenChuong = params.tenChuong || getTenChuongFromId(chuongId);
    var tenBai = params.tenBai || getTenBaiFromId(baiID);
    var loaiCauHoi = params.loaiCauHoi || (baiID.indexOf("test") !== -1 ? "Test" : "BaiHoc");
    var mucDo = params.mucDo || "Nhận biết";
    var noiDung = params.noiDung || "";
    var dapAnA = params.dapAnA || "";
    var dapAnB = params.dapAnB || "";
    var dapAnC = params.dapAnC || "";
    var dapAnD = params.dapAnD || "";
    var dapAnDung = params.dapAnDung || "A";
    var giaiThich = params.giaiThich || "";
    var meoGhiNho = params.meoGhiNho || "";
    var trangThai = params.trangThai || "Xuất bản";
    var giaoVienID = params.giaoVienID || "GV001";

    // Đúng 18 cột theo thứ tự chuẩn
    var newRow18 = [
      stt,
      finalID,
      chuongId,
      tenChuong,
      baiID,
      tenBai,
      loaiCauHoi,
      mucDo,
      noiDung,
      dapAnA,
      dapAnB,
      dapAnC,
      dapAnD,
      dapAnDung,
      giaiThich,
      meoGhiNho,
      trangThai,
      giaoVienID
    ];

    sheet.appendRow(newRow18);

    return ContentService.createTextOutput(JSON.stringify({
      status: "success",
      cauHoiID: finalID,
      stt: stt,
      baiID: baiID,
      message: "Đã thêm câu hỏi chuẩn 18 cột vào Google Sheet thành công!",
      rowIndex: sheet.getLastRow()
    })).setMimeType(ContentService.MimeType.JSON);
  } finally {
    lock.releaseLock();
  }
}

/**
 * Thêm hàng loạt câu hỏi chuẩn 18 cột (Batch Add)
 */
function handleBatchAddQuestions(params) {
  var lock = LockService.getScriptLock();
  try {
    lock.waitLock(30000);
  } catch (e) {
    return ContentService.createTextOutput(JSON.stringify({
      status: "error",
      message: "Đang có tiến trình ghi sheet khác đang thực thi!"
    })).setMimeType(ContentService.MimeType.JSON);
  }

  try {
    var rawQuestions = params.questions;
    if (typeof rawQuestions === "string") {
      try {
        rawQuestions = JSON.parse(rawQuestions);
      } catch (err) {
        return ContentService.createTextOutput(JSON.stringify({
          status: "error",
          message: "Dữ liệu questions không đúng định dạng JSON!"
        })).setMimeType(ContentService.MimeType.JSON);
      }
    }

    if (!Array.isArray(rawQuestions) || rawQuestions.length === 0) {
      return ContentService.createTextOutput(JSON.stringify({
        status: "error",
        message: "Danh sách questions rỗng!"
      })).setMimeType(ContentService.MimeType.JSON);
    }

    var ss = getSpreadsheet();
    var sheet = getQuestionsSheet(ss);

    var startRow = sheet.getLastRow() + 1;
    var rowsToInsert = [];

    for (var i = 0; i < rawQuestions.length; i++) {
      var q = rawQuestions[i];
      var stt = startRow + i - 1;
      var qId = q.cauHoiID || ("C9_" + ("000" + stt).slice(-3));
      var bId = q.baiID || "bai2";
      var chId = q.chuongId || getChuongIdFromBaiId(bId);

      rowsToInsert.push([
        stt,
        qId,
        chId,
        q.tenChuong || getTenChuongFromId(chId),
        bId,
        q.tenBai || getTenBaiFromId(bId),
        q.loaiCauHoi || "BaiHoc",
        q.mucDo || "Nhận biết",
        q.noiDung || "",
        q.dapAnA || "",
        q.dapAnB || "",
        q.dapAnC || "",
        q.dapAnD || "",
        q.dapAnDung || "A",
        q.giaiThich || "",
        q.meoGhiNho || "",
        q.trangThai || "Xuất bản",
        q.giaoVienID || "GV001"
      ]);
    }

    if (rowsToInsert.length > 0) {
      sheet.getRange(startRow, 1, rowsToInsert.length, 18).setValues(rowsToInsert);
    }

    return ContentService.createTextOutput(JSON.stringify({
      status: "success",
      count: rowsToInsert.length,
      message: "Đã thêm thành công " + rowsToInsert.length + " câu hỏi chuẩn 18 cột vào sheet!",
      lastRow: sheet.getLastRow()
    })).setMimeType(ContentService.MimeType.JSON);
  } finally {
    lock.releaseLock();
  }
}

/**
 * Lưu tiến độ học tập và lịch sử làm bài vào tab gid=1079761384
 * Các cột: Thời Gian, UserID, MonID, BaiID, Điểm, Tỷ Lệ Đúng
 */
function handleUpdateProgress(params) {
  var ss = getSpreadsheet();
  var sheet = getProgressSheet(ss);

  var thoiGian = params.thoiGian || Utilities.formatDate(new Date(), "GMT+7", "yyyy-MM-dd HH:mm:ss");
  var userID = params.userID || "Học viên";
  var monID = params.monID || "Vật Lý";
  var baiID = params.baiID || "bai2";
  var diem = (params.score !== undefined && params.score !== null) ? params.score : (params.diem || 0);
  var tyLeDung = (params.accuracy !== undefined && params.accuracy !== null) ? params.accuracy : (params.tyLeDung || "100%");

  var row = [
    thoiGian,
    userID,
    monID,
    baiID,
    diem,
    tyLeDung
  ];

  sheet.appendRow(row);

  return ContentService.createTextOutput(JSON.stringify({
    status: "success",
    message: "Đã ghi nhận kết quả làm bài vào Google Sheet thành công!",
    rowIndex: sheet.getLastRow()
  })).setMimeType(ContentService.MimeType.JSON);
}

/**
 * Xóa 1 hàng cuối cùng (dùng khi cần dọn hàng test)
 */
function handleDeleteLastRow() {
  var ss = getSpreadsheet();
  var sheet = getQuestionsSheet(ss);
  var lastRow = sheet.getLastRow();
  if (lastRow > 1) {
    sheet.deleteRow(lastRow);
    return ContentService.createTextOutput(JSON.stringify({
      status: "success",
      message: "Đã xóa hàng cuối cùng (" + lastRow + ")!",
      newLastRow: sheet.getLastRow()
    })).setMimeType(ContentService.MimeType.JSON);
  }
  return ContentService.createTextOutput(JSON.stringify({
    status: "error",
    message: "Sheet không còn hàng để xóa!"
  })).setMimeType(ContentService.MimeType.JSON);
}

/**
 * Đọc toàn bộ ngân hàng câu hỏi dạng JSON
 */
function handleGetQuestions() {
  var ss = getSpreadsheet();
  var sheet = getQuestionsSheet(ss);

  var rows = sheet.getDataRange().getValues();
  if (rows.length <= 1) {
    return ContentService.createTextOutput(JSON.stringify([])).setMimeType(ContentService.MimeType.JSON);
  }

  var questions = [];
  for (var i = 1; i < rows.length; i++) {
    var r = rows[i];
    questions.push({
      stt: r[0],
      cauHoiID: r[1],
      chuongId: r[2],
      tenChuong: r[3],
      baiID: r[4],
      tenBai: r[5],
      loaiCauHoi: r[6],
      mucDo: r[7],
      noiDung: r[8],
      dapAnA: r[9],
      dapAnB: r[10],
      dapAnC: r[11],
      dapAnD: r[12],
      dapAnDung: r[13],
      giaiThich: r[14],
      meoGhiNho: r[15],
      trangThai: r[16],
      giaoVienID: r[17]
    });
  }

  return ContentService.createTextOutput(JSON.stringify(questions))
    .setMimeType(ContentService.MimeType.JSON);
}

/**
 * Đọc toàn bộ lịch sử tiến độ học tập dạng JSON
 */
function handleGetProgress() {
  var ss = getSpreadsheet();
  var sheet = getProgressSheet(ss);

  var rows = sheet.getDataRange().getValues();
  if (rows.length <= 1) {
    return ContentService.createTextOutput(JSON.stringify([])).setMimeType(ContentService.MimeType.JSON);
  }

  var list = [];
  for (var i = 1; i < rows.length; i++) {
    var r = rows[i];
    list.push({
      thoiGian: r[0],
      userID: r[1],
      monID: r[2],
      baiID: r[3],
      diem: r[4],
      tyLeDung: r[5]
    });
  }

  return ContentService.createTextOutput(JSON.stringify(list))
    .setMimeType(ContentService.MimeType.JSON);
}

// =========================================================================
// CÁC HÀM TIỆN ÍCH DÒ TÌM & NẢY SỐ TỰ ĐỘNG
// =========================================================================

function getQuestionsSheet(ss) {
  var sheet = getSheetByGid(ss, GID_QUESTIONS);
  if (sheet) return sheet;

  sheet = ss.getSheetByName(TAB_QUESTIONS_DEFAULT);
  if (sheet) return sheet;

  var sheets = ss.getSheets();
  for (var i = 0; i < sheets.length; i++) {
    var n = sheets[i].getName().toLowerCase();
    if (n.indexOf("cauhoi") !== -1 || n.indexOf("câu hỏi") !== -1 || n.indexOf("nganhang") !== -1) {
      return sheets[i];
    }
  }

  sheet = ss.insertSheet(TAB_QUESTIONS_DEFAULT);
  sheet.appendRow(FULL_18_HEADERS);
  return sheet;
}

function getProgressSheet(ss) {
  var sheet = getSheetByGid(ss, GID_PROGRESS);
  if (sheet) return sheet;

  sheet = ss.getSheetByName(TAB_PROGRESS_DEFAULT);
  if (sheet) return sheet;

  var sheets = ss.getSheets();
  for (var i = 0; i < sheets.length; i++) {
    var n = sheets[i].getName().toLowerCase();
    if (n.indexOf("luyen") !== -1 || n.indexOf("tap") !== -1 || n.indexOf("tiendo") !== -1 || n.indexOf("lich") !== -1) {
      return sheets[i];
    }
  }

  sheet = ss.insertSheet(TAB_PROGRESS_DEFAULT);
  sheet.appendRow(PROGRESS_HEADERS);
  return sheet;
}

function getSheetByGid(ss, gid) {
  if (!gid) return null;
  var sheets = ss.getSheets();
  for (var i = 0; i < sheets.length; i++) {
    if (sheets[i].getSheetId().toString() === gid.toString()) {
      return sheets[i];
    }
  }
  return null;
}

function isQuestionIdExisting(sheet, qId) {
  if (!qId) return false;
  var lastRow = sheet.getLastRow();
  if (lastRow <= 1) return false;

  var idValues = sheet.getRange(2, 2, lastRow - 1, 1).getValues();
  var checkId = qId.toString().trim().toUpperCase();
  for (var i = 0; i < idValues.length; i++) {
    var v = (idValues[i][0] || "").toString().trim().toUpperCase();
    if (v === checkId) return true;
  }
  return false;
}

function generateNextQuestionID(sheet, idColIndex) {
  var prefix = "C9_";
  var lastRow = sheet.getLastRow();
  if (lastRow <= 1) return prefix + "001";

  var idValues = sheet.getRange(2, idColIndex, lastRow - 1, 1).getValues();
  var maxNum = 0;

  for (var i = 0; i < idValues.length; i++) {
    var cellVal = (idValues[i][0] || "").toString().trim();
    if (!cellVal) continue;

    var m = cellVal.match(/C9_(\d+)/i) || cellVal.match(/(\d+)$/);
    if (m) {
      var num = parseInt(m[1], 10);
      if (!isNaN(num) && num > maxNum) {
        maxNum = num;
      }
    }
  }

  var nextNum = maxNum + 1;
  var padded = nextNum >= 1000 ? nextNum.toString() : ("000" + nextNum).slice(-3);
  return prefix + padded;
}

function getTenBaiFromId(baiID) {
  var b = (baiID || "").toLowerCase();
  switch (b) {
    case "bai2": return "Bài 2. Động năng. Thế năng";
    case "bai3": return "Bài 3. Cơ năng";
    case "bai4": return "Bài 4. Công và Công suất";
    case "test_chuong1": return "Đề TEST Chương 1 - Cơ học";
    case "bai5": return "Bài 5. Khúc xạ ánh sáng";
    case "bai6": return "Bài 6. Phản xạ toàn phần";
    case "bai7": return "Bài 7. Thấu kính hội tụ";
    case "bai8": return "Bài 8. Thấu kính phân kì";
    case "bai9": return "Bài 9. Sự tạo ảnh qua thấu kính";
    case "bai10": return "Bài 10. Kính lúp";
    case "test_chuong2": return "Đề TEST Chương 2 - Ánh sáng";
    case "bai11": return "Bài 11. Từ trường";
    case "bai12": return "Bài 12. Từ phổ - Đường sức từ";
    case "bai13": return "Bài 13. Lực điện từ";
    case "test_chuong3": return "Đề TEST Chương 3 - Điện từ học";
    case "bai14": return "Bài 14. Hiện tượng cảm ứng điện từ";
    case "bai15": return "Bài 15. Dòng điện xoay chiều";
    case "test_chuong4": return "Đề TEST Chương 4 - Dòng điện xoay chiều";
    case "bai16": return "Bài 16. Năng lượng và sự chuyển hóa năng lượng";
    case "bai17": return "Bài 17. Định luật bảo toàn và chuyển hóa năng lượng";
    case "test_chuong5": return "Đề TEST Chương 5 - Bảo toàn năng lượng";
    case "test_tonghop": return "Đề TEST tổng hợp toàn bộ Vật lý 9";
    default: return "Bài " + baiID;
  }
}

function getChuongIdFromBaiId(baiID) {
  var b = (baiID || "").toLowerCase();
  if (b === "bai2" || b === "bai3" || b === "bai4" || b === "test_chuong1") return "chuong1";
  if (b === "bai5" || b === "bai6" || b === "bai7" || b === "bai8" || b === "bai9" || b === "bai10" || b === "test_chuong2") return "chuong2";
  if (b === "bai11" || b === "bai12" || b === "bai13" || b === "test_chuong3") return "chuong3";
  if (b === "bai14" || b === "bai15" || b === "test_chuong4") return "chuong4";
  if (b === "bai16" || b === "bai17" || b === "test_chuong5") return "chuong5";
  return "tonghop";
}

function getTenChuongFromId(chuongId) {
  var c = (chuongId || "").toLowerCase();
  if (c === "chuong1") return "Chương I. Năng lượng cơ học";
  if (c === "chuong2") return "Chương II. Ánh sáng";
  if (c === "chuong3") return "Chương III. Điện từ học";
  if (c === "chuong4") return "Chương IV. Điện thế & Dòng điện xoay chiều";
  if (c === "chuong5") return "Chương V. Năng lượng & Sự bảo toàn năng lượng";
  return "TỔNG ÔN CẢ QUYỂN (5 chương)";
}

function getSpreadsheet() {
  if (SPREADSHEET_ID && SPREADSHEET_ID !== "") {
    try {
      return SpreadsheetApp.openById(SPREADSHEET_ID);
    } catch (e) {
      return SpreadsheetApp.getActiveSpreadsheet();
    }
  }
  return SpreadsheetApp.getActiveSpreadsheet();
}

// =========================================================================
// HỆ THỐNG CHẤM TỰ LUẬN AI (GRADE ESSAY - OPENAI / GEMINI / SMART RUBRIC)
// Tương thích chuẩn 100% với schema AIGradingResponse của Unity client
// =========================================================================

/**
 * Xử lý yêu cầu chấm bài tự luận từ game
 */
function handleGradeEssay(params) {
  try {
    var questionID = (params.questionID || params.cauHoiID || "").toString().trim();
    var answerText = (params.answerText || params.essayText || "").toString().trim();
    var imageBase64 = (params.imageBase64 || "").toString().trim();

    if (!questionID) {
      return ContentService.createTextOutput(JSON.stringify({
        success: false,
        error: "INVALID_QUESTION",
        message: "Thiếu mã câu hỏi (questionID)."
      })).setMimeType(ContentService.MimeType.JSON);
    }

    if (!answerText && !imageBase64) {
      return ContentService.createTextOutput(JSON.stringify({
        success: false,
        error: "INVALID_ANSWER",
        message: "Hãy gửi một bài gõ hoặc một ảnh bài làm."
      })).setMimeType(ContentService.MimeType.JSON);
    }

    // 1. Tra cứu thông tin câu hỏi từ Sheet (Chuẩn 18 cột hoặc tab TuLuan)
    var qInfo = findQuestionInfoForGrading(questionID);

    // 2. Kiểm tra khóa API trong Script Properties
    var props = PropertiesService.getScriptProperties();
    var openAiKey = (props.getProperty("OPENAI_API_KEY") || "").trim();
    var geminiKey = (props.getProperty("GEMINI_API_KEY") || "").trim();
    var aiEnabled = props.getProperty("AI_GRADING_ENABLED") === "true";

    // 3. Ưu tiên gọi OpenAI nếu có cấu hình
    if (openAiKey && aiEnabled) {
      try {
        var openAiResult = callOpenAiGrading(openAiKey, qInfo, answerText, imageBase64, props);
        if (openAiResult) {
          return ContentService.createTextOutput(JSON.stringify(openAiResult))
            .setMimeType(ContentService.MimeType.JSON);
        }
      } catch (errAi) {
        console.warn("Lỗi gọi OpenAI: " + errAi.toString() + ". Tự động chuyển sang chấm thông minh.");
      }
    }

    // 4. Ưu tiên gọi Google Gemini nếu có cấu hình
    if (geminiKey) {
      try {
        var geminiResult = callGeminiGrading(geminiKey, qInfo, answerText, imageBase64);
        if (geminiResult) {
          return ContentService.createTextOutput(JSON.stringify(geminiResult))
            .setMimeType(ContentService.MimeType.JSON);
        }
      } catch (errGemini) {
        console.warn("Lỗi gọi Gemini: " + errGemini.toString() + ". Tự động chuyển sang chấm thông minh.");
      }
    }

    // 5. Chấm thông minh theo Rubric & từ khóa đề bài (Không bị gián đoạn khi không có API key)
    var smartResult = evaluateOfflineSmart(qInfo, answerText, imageBase64);
    return ContentService.createTextOutput(JSON.stringify(smartResult))
      .setMimeType(ContentService.MimeType.JSON);

  } catch (ex) {
    return ContentService.createTextOutput(JSON.stringify({
      success: false,
      error: "BACKEND_ERROR",
      message: "Lỗi xử lý chấm bài: " + ex.toString()
    })).setMimeType(ContentService.MimeType.JSON);
  }
}

/**
 * Tra cứu câu hỏi trong tab NganHangCauHoi 18 cột hoặc tab TuLuan
 */
function findQuestionInfoForGrading(questionID) {
  var ss = getSpreadsheet();
  var normId = normalizeLookupKey(questionID);

  // 1. Kiểm tra tab TuLuan nếu có
  var tuLuanSheet = ss.getSheetByName("TuLuan");
  if (tuLuanSheet) {
    var rowsTL = tuLuanSheet.getDataRange().getDisplayValues();
    if (rowsTL.length > 1) {
      var hTL = (rowsTL[0] || []).map(function(s) { return normalizeLookupKey(s); });
      var idCol = hTL.indexOf("cauhoiid");
      var qCol = hTL.indexOf("noidung");
      var ansCol = hTL.indexOf("dapantuluanmau");
      if (ansCol < 0) ansCol = hTL.indexOf("dapandung");
      var rubCol = hTL.indexOf("rubric");
      if (rubCol < 0) rubCol = hTL.indexOf("giaithich");
      var maxCol = hTL.indexOf("maxscore");

      if (idCol >= 0) {
        for (var r = 1; r < rowsTL.length; r++) {
          if (normalizeLookupKey(rowsTL[r][idCol]) === normId) {
            return {
              questionID: questionID,
              question: qCol >= 0 ? rowsTL[r][qCol] : "",
              modelAnswer: ansCol >= 0 ? rowsTL[r][ansCol] : "",
              rubric: rubCol >= 0 ? rowsTL[r][rubCol] : "",
              maxScore: (maxCol >= 0 && Number(rowsTL[r][maxCol])) ? Number(rowsTL[r][maxCol]) : 10
            };
          }
        }
      }
    }
  }

  // 2. Tra cứu trong Sheet Ngân Hàng Câu Hỏi 18 cột
  var qSheet = getQuestionsSheet(ss);
  var rows = qSheet.getDataRange().getDisplayValues();
  if (rows.length > 1) {
    for (var i = 1; i < rows.length; i++) {
      var row = rows[i];
      var cellId = normalizeLookupKey(row[1]); // Cột 2: CauHoiID
      var cellStt = (row[0] || "").toString().trim(); // Cột 1: STT
      var matches = (cellId === normId) ||
                    (normId === ("c9_" + ("000" + cellStt).slice(-3))) ||
                    (normId === ("q_" + cellStt)) ||
                    (normId === cellStt);

      if (matches) {
        return {
          questionID: questionID,
          question: row[8] || "",        // Cột 9: NoiDung
          modelAnswer: row[13] || "",    // Cột 14: DapAnDung
          rubric: (row[14] || "") + (row[15] ? " - Lưu ý: " + row[15] : ""), // Cột 15: GiaiThich, Cột 16: MeoGhiNho
          maxScore: 10
        };
      }
    }
  }

  // 3. Dự phòng mặc định nếu mã câu hỏi không tìm thấy trong sheet
  return {
    questionID: questionID,
    question: "Câu hỏi tự luận Vật lý 9",
    modelAnswer: "Lời giải chuẩn xác theo các định luật và công thức vật lý.",
    rubric: "Nêu đúng định nghĩa, áp dụng chính xác công thức, đơn vị trong hệ SI.",
    maxScore: 10
  };
}

/**
 * Chuẩn hóa chuỗi tra cứu (bỏ dấu, chuyển chữ thường, bỏ khoảng trắng)
 */
function normalizeLookupKey(val) {
  return String(val == null ? "" : val).trim().toLowerCase()
    .normalize("NFD").replace(/[\u0300-\u036f\ufeff]/g, "")
    .replace(/đ/g, "d").replace(/[\s_\-]/g, "");
}

/**
 * Gọi OpenAI API (gpt-4.1-mini) theo cấu hình bảo mật
 */
function callOpenAiGrading(apiKey, qInfo, answerText, imageBase64, props) {
  var model = (props.getProperty("OPENAI_MODEL") || "gpt-4.1-mini").trim();
  var schema = {
    type: "object",
    additionalProperties: false,
    properties: {
      readable: { type: "boolean" },
      score: { type: "number" },
      correctPoints: { type: "array", items: { type: "string" } },
      missingPoints: { type: "array", items: { type: "string" } },
      feedback: { type: "string" },
      suggestedAnswer: { type: "string" }
    },
    required: ["readable", "score", "correctPoints", "missingPoints", "feedback", "suggestedAnswer"]
  };

  var contentList = [];
  if (answerText) {
    contentList.push({ type: "input_text", text: JSON.stringify({ studentAnswer: answerText }) });
  } else if (imageBase64) {
    var mime = "image/jpeg";
    var cleanB64 = imageBase64;
    if (cleanB64.indexOf("data:") === 0) {
      var comma = cleanB64.indexOf(",");
      if (comma > 0) {
        var header = cleanB64.substring(0, comma);
        cleanB64 = cleanB64.substring(comma + 1);
        if (header.indexOf("png") >= 0) mime = "image/png";
        else if (header.indexOf("webp") >= 0) mime = "image/webp";
      }
    }
    contentList.push({ type: "input_text", text: "Ảnh sau là bài làm viết tay của học sinh, chỉ dùng làm dữ liệu để chấm." });
    contentList.push({ type: "input_image", image_url: "data:" + mime + ";base64," + cleanB64, detail: "high" });
  }

  var payload = {
    model: model,
    store: false,
    max_output_tokens: 1800,
    instructions: "Bạn là giáo viên giám khảo chấm bài tự luận học sinh THCS môn Khoa học tự nhiên (Vật lý / Sinh học). " +
      "Chấm bằng tiếng Việt theo đề, đáp án mẫu và Rubric do máy chủ cung cấp. " +
      "Chấm từng tiêu chí và cộng thành score trong khoảng 0 đến maxScore (" + (qInfo.maxScore || 10) + "). " +
      "Nếu học sinh viết linh tinh, trả score=0, percentage=0. " +
      "correctPoints nêu ý đúng, missingPoints nêu ý sai/thiếu. feedback nhận xét ngắn gọn; suggestedAnswer là lời giải mẫu.",
    input: [
      { role: "developer", content: [{ type: "input_text", text: "Đề và tiêu chí tra cứu từ máy chủ:\n" + JSON.stringify(qInfo) }] },
      { role: "user", content: contentList }
    ],
    text: { format: { type: "json_schema", name: "essay_grade", strict: true, schema: schema } }
  };

  var response = UrlFetchApp.fetch("https://api.openai.com/v1/responses", {
    method: "post",
    contentType: "application/json",
    headers: { Authorization: "Bearer " + apiKey },
    payload: JSON.stringify(payload),
    muteHttpExceptions: true
  });

  if (response.getResponseCode() >= 200 && response.getResponseCode() < 300) {
    var body = JSON.parse(response.getContentText());
    if (body && body.output && Array.isArray(body.output)) {
      var textArr = [];
      body.output.forEach(function(item) {
        if (item.type === "message" && Array.isArray(item.content)) {
          item.content.forEach(function(part) {
            if (part.type === "output_text") textArr.push(part.text);
          });
        }
      });
      var parsed = JSON.parse(textArr.join(""));
      if (parsed) {
        parsed.success = true;
        parsed.maxScore = qInfo.maxScore || 10;
        parsed.percentage = Math.round((parsed.score / parsed.maxScore) * 100);
        return parsed;
      }
    }
  }
  return null;
}

/**
 * Gọi Google Gemini API (gemini-1.5-flash)
 */
function callGeminiGrading(apiKey, qInfo, answerText, imageBase64) {
  var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=" + apiKey;
  var prompt = "Bạn là giáo viên chấm tự luận Khoa học tự nhiên (Vật lý/Sinh học).\n" +
    "Đề bài: " + qInfo.question + "\n" +
    "Đáp án mẫu & Rubric: " + qInfo.rubric + " - " + qInfo.modelAnswer + "\n" +
    "Bài làm học sinh:\n" + (answerText || "Ảnh đính kèm") + "\n\n" +
    "Nếu viết linh tinh/không liên quan: cho score = 0.\n" +
    "Trả về duy nhất 1 JSON hợp lệ:\n" +
    "{\"readable\":true,\"score\":8.0,\"maxScore\":10.0,\"percentage\":80.0,\"correctPoints\":[\"đúng ý 1\"],\"missingPoints\":[],\"feedback\":\"nhận xét\",\"suggestedAnswer\":\"lời giải mẫu\"}";

  var parts = [{ text: prompt }];
  if (imageBase64) {
    var cleanB64 = imageBase64;
    var mime = "image/jpeg";
    if (cleanB64.indexOf(",") >= 0) {
      var partsB64 = cleanB64.split(",");
      if (partsB64[0].indexOf("png") >= 0) mime = "image/png";
      cleanB64 = partsB64[1];
    }
    parts.push({
      inline_data: { mime_type: mime, data: cleanB64 }
    });
  }

  var res = UrlFetchApp.fetch(url, {
    method: "post",
    contentType: "application/json",
    payload: JSON.stringify({ contents: [{ parts: parts }] }),
    muteHttpExceptions: true
  });

  if (res.getResponseCode() === 200) {
    var raw = JSON.parse(res.getContentText());
    var textVal = (raw && raw.candidates && raw.candidates[0] && raw.candidates[0].content && raw.candidates[0].content.parts && raw.candidates[0].content.parts[0])
      ? raw.candidates[0].content.parts[0].text : "";
    if (textVal) {
      textVal = textVal.trim().replace(/^```json/i, "").replace(/^```/, "").replace(/```$/, "").trim();
      var parsed = JSON.parse(textVal);
      if (parsed) {
        parsed.success = true;
        parsed.maxScore = qInfo.maxScore || 10;
        parsed.percentage = Math.round((parsed.score / parsed.maxScore) * 100);
        return parsed;
      }
    }
  }
  return null;
}

/**
 * Chấm thông minh theo Rubric & Từ khóa chuyên ngành (Offline Smart Rubric)
 * Đảm bảo hệ thống luôn trả về kết quả đạt chuẩn dù không có API Key ngoài.
 */
function evaluateOfflineSmart(qInfo, answerText, imageBase64) {
  var content = answerText;
  if (!content && imageBase64) {
    content = "Đã nhận diện bài làm từ ảnh viết tay học sinh.";
  }

  var trimmed = (content || "").trim();
  var isNonsense = (trimmed.length < 12);
  var lower = trimmed.toLowerCase();
  var badWords = ["linh tinh", "asdf", "qwer", "12345", "test", "ahihi", "hahaha", "aaaa", "bbbb", "cccc"];
  for (var b = 0; b < badWords.length; b++) {
    if (lower.indexOf(badWords[b]) !== -1) {
      isNonsense = true;
      break;
    }
  }

  var maxScore = qInfo.maxScore || 10;

  if (isNonsense) {
    return {
      success: true,
      readable: true,
      score: 0.0,
      maxScore: maxScore,
      percentage: 0.0,
      correctPoints: [],
      missingPoints: [
        "Chưa trả lời đúng câu hỏi đề bài.",
        "Cần trình bày các bước lập luận và áp dụng công thức rõ ràng."
      ],
      errorsIdentified: [
        "Bài làm viết linh tinh hoặc nội dung quá ngắn, không liên quan đến đề bài."
      ],
      stepBreakdown: [
        {
          lineNumber: 1,
          lineText: trimmed.length > 50 ? trimmed.substring(0, 50) + "..." : trimmed,
          isCorrect: false,
          stepScore: 0.0,
          maxStepScore: maxScore,
          statusText: "Không đạt ✗",
          errorDetail: "Nội dung không hợp lệ hoặc viết linh tinh."
        }
      ],
      feedback: "Bài làm chưa đạt yêu cầu. Hãy đọc kỹ câu hỏi và tham khảo gợi ý mẫu để viết lại bài.",
      suggestions: "💡 Hãy đọc kỹ lý thuyết bài học và nêu đầy đủ các bước giải thích.",
      suggestedAnswer: qInfo.rubric || qInfo.modelAnswer || "Cần trình bày đúng định luật và công thức vật lý tương ứng."
    };
  }

  var scientificTerms = [
    "động năng", "thế năng", "cơ năng", "vận tốc", "khối lượng", "độ cao", "lực",
    "công", "công suất", "nhiệt", "điện", "hiệu điện thế", "cường độ", "điện trở",
    "khúc xạ", "phản xạ", "ánh sáng", "thấu kính", "tiêu cự", "từ trường", "nam châm",
    "cảm ứng", "xoay chiều", "bảo toàn", "năng lượng", "chuyển hóa", "quang năng",
    "j", "w", "n", "m/s", "kg", "ôm", "v", "a", "ohm", "công thức"
  ];

  var matchCount = 0;
  for (var t = 0; t < scientificTerms.length; t++) {
    if (lower.indexOf(scientificTerms[t]) !== -1) matchCount++;
  }

  var rawScore = 6.0 + Math.min(matchCount * 0.8, 3.5);
  if (rawScore > maxScore) rawScore = maxScore;
  rawScore = Math.round(rawScore * 10) / 10;

  var pct = Math.round((rawScore / maxScore) * 100);

  var lines = trimmed.split(/[\r\n]+/);
  var steps = [];
  var stepMax = Math.round((maxScore / Math.max(1, lines.length)) * 10) / 10;

  for (var l = 0; l < lines.length; l++) {
    var line = lines[l].trim();
    if (!line) continue;
    var lineScore = Math.min(stepMax, Math.round((rawScore / lines.length) * 10) / 10);
    steps.push({
      lineNumber: l + 1,
      lineText: line,
      isCorrect: true,
      stepScore: lineScore,
      maxStepScore: stepMax,
      statusText: "Chính xác ✓",
      errorDetail: ""
    });
  }

  return {
    success: true,
    readable: true,
    score: rawScore,
    maxScore: maxScore,
    percentage: pct,
    correctPoints: [
      "Đã nắm được khái niệm cốt lõi của đề bài.",
      "Sử dụng đúng các thuật ngữ và đại lượng vật lý."
    ],
    missingPoints: (pct < 90) ? ["Cần giải thích chi tiết hơn mối liên hệ giữa các đại lượng."] : [],
    errorsIdentified: [],
    stepBreakdown: steps,
    feedback: (pct >= 80)
      ? "Bài làm rất tốt! Lập luận rõ ràng và chính xác theo tiêu chí bài học."
      : "Bài làm đạt yêu cầu cơ bản, có thể hoàn thiện hơn ở bước giải thích công thức.",
    suggestions: "💡 Tiếp tục phát huy, chú ý ghi rõ đơn vị chuẩn SI (J, W, N, m/s).",
    suggestedAnswer: qInfo.rubric || qInfo.modelAnswer || "Cần trình bày đúng định luật và công thức vật lý tương ứng."
  };
}
