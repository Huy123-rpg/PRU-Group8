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

    if (action === "add_question") {
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
