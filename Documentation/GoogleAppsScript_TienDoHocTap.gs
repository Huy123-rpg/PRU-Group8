/**
 * GOOGLE APPS SCRIPT - ĐỒNG BỘ TIẾN ĐỘ HỌC TẬP CHUẨN 10 CỘT CHO PRU-GROUP8
 * Google Sheet: General_Game_Hub_Shared_Sheet_V1_1_Group1
 * Tab: TienDoHocTap (gid=11169486)
 * 
 * Hướng dẫn cài đặt trên Google Sheet:
 * 1. Trên Google Sheets, vào menu "Tiện ích mở rộng" (Extensions) > "Apps Script".
 * 2. Tìm file xử lý doPost(e) hoặc tạo file mới tên Code.gs và dán mã bên dưới vào.
 * 3. Bấm "Triển khai" (Deploy) > "Quản lý bản triển khai" (Manage deployments) > Chỉnh sửa (bút chì) > Phiên bản mới (New version) > Triển khai.
 */

function doPost(e) {
  try {
    var raw = e.postData.contents;
    var data = JSON.parse(raw);
    var ss = SpreadsheetApp.getActiveSpreadsheet();
    
    // =========================================================================
    // 1. XỬ LÝ TIẾN ĐỘ HỌC TẬP (Tab: TienDoHocTap - 10 cột A -> J)
    // =========================================================================
    if (data.action === "push_progress" || data.sheetName === "TienDoHocTap") {
      var sheet = ss.getSheetByName("TienDoHocTap");
      if (!sheet) {
        sheet = ss.insertSheet("TienDoHocTap");
        sheet.appendRow([
          "TienDoID", "UserID", "MonID", "BaiID", "SoLanLam", 
          "DiemCaoNhat", "DiemGanNhat", "TyLeDung", "TrangThai", "LanHocCuoi"
        ]);
      }
      
      var lastRow = sheet.getLastRow();
      var nextIndex = lastRow; // Bỏ qua tiêu đề
      var tienDoID = data.tienDoID && data.tienDoID !== "AUTO" 
        ? data.tienDoID 
        : ("TD" + ("0000" + nextIndex).slice(-4));
      
      var userID = data.userID || data.studentName || "Học viên";
      var monID = data.monID || data.className || "VatLy9";
      var baiID = data.baiID || data.lessonName || "bai1";
      var score = typeof data.score === "number" ? data.score : (data.diemCaoNhat || 0);
      var accuracy = data.tyLeDung || (data.accuracyRate ? (data.accuracyRate + "%") : "100%");
      var nowStr = Utilities.formatDate(new Date(), "Asia/Ho_Chi_Minh", "yyyy-MM-dd HH:mm:ss");
      var status = (score >= 50) ? "Đã hoàn thành" : "Cần rèn luyện";
      
      // Kiểm tra xem User và Bài này đã có trong sheet chưa để cập nhật hoặc thêm mới
      var range = sheet.getDataRange();
      var values = range.getValues();
      var foundRow = -1;
      
      for (var r = 1; r < values.length; r++) {
        // Cột B là UserID, Cột C là MonID, Cột D là BaiID
        if (values[r][1] == userID && values[r][2] == monID && values[r][3] == baiID) {
          foundRow = r + 1;
          break;
        }
      }
      
      if (foundRow > 0) {
        // Cập nhật dòng đã tồn tại
        var currentSoLan = parseInt(values[foundRow - 1][4]) || 1;
        var currentCaoNhat = parseInt(values[foundRow - 1][5]) || 0;
        var newCaoNhat = Math.max(currentCaoNhat, score);
        
        sheet.getRange(foundRow, 5).setValue(currentSoLan + 1); // SoLanLam
        sheet.getRange(foundRow, 6).setValue(newCaoNhat);         // DiemCaoNhat
        sheet.getRange(foundRow, 7).setValue(score);              // DiemGanNhat
        sheet.getRange(foundRow, 8).setValue(accuracy);           // TyLeDung
        sheet.getRange(foundRow, 9).setValue(status);             // TrangThai
        sheet.getRange(foundRow, 10).setValue(nowStr);            // LanHocCuoi
      } else {
        // Thêm mới đúng chuẩn 10 cột A -> J
        sheet.appendRow([
          tienDoID,      // Cột A: TienDoID
          userID,        // Cột B: UserID
          monID,         // Cột C: MonID
          baiID,         // Cột D: BaiID
          1,             // Cột E: SoLanLam
          score,         // Cột F: DiemCaoNhat
          score,         // Cột G: DiemGanNhat
          accuracy,      // Cột H: TyLeDung
          status,        // Cột I: TrangThai
          nowStr         // Cột J: LanHocCuoi
        ]);
      }
      
      return ContentService.createTextOutput(JSON.stringify({
        status: "success",
        message: "Cập nhật tiến độ học tập 10 cột thành công!",
        row: foundRow > 0 ? foundRow : sheet.getLastRow()
      })).setMimeType(ContentService.MimeType.JSON);
    }
    
    // =========================================================================
    // 2. XỬ LÝ LỊCH SỬ LÀM BÀI (Tab: LichSuLamBai)
    // =========================================================================
    if (data.action === "push_history" || data.sheetName === "LichSuLamBai") {
      var sheetHist = ss.getSheetByName("LichSuLamBai");
      if (sheetHist) {
        sheetHist.appendRow([
          data.studentName || "Học sinh",
          data.className || "9A1",
          data.lessonName || "Vật lý",
          data.score || 0,
          data.accuracyRate || 100,
          data.duration || "05:00",
          data.date || Utilities.formatDate(new Date(), "Asia/Ho_Chi_Minh", "yyyy-MM-dd HH:mm:ss")
        ]);
      }
      return ContentService.createTextOutput(JSON.stringify({ status: "success" })).setMimeType(ContentService.MimeType.JSON);
    }
    
    return ContentService.createTextOutput(JSON.stringify({ status: "ignored" })).setMimeType(ContentService.MimeType.JSON);
  } catch (err) {
    return ContentService.createTextOutput(JSON.stringify({ status: "error", error: err.toString() })).setMimeType(ContentService.MimeType.JSON);
  }
}
