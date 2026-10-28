using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ScienceQuest.Biology
{
    /// <summary>
    /// Một dòng kết quả của người học, sẽ được ghi ra bảng tính.
    /// </summary>
    [Serializable]
    public class ScoreRecord
    {
        public string timestamp = "";
        public string username = "";
        public string chapter = "";
        public string lesson = "";
        public string mode = "";
        public int totalQuestions = 0;
        public int correctCount = 0;
        public int wrongCount = 0;
        public int score = 0;
        public int highScore = 0;
        public int bossesDefeated = 0;
        public float accuracyPercent = 0f;
        public string details = "";
    }

    /// <summary>
    /// Ghi dữ liệu ra file Excel (.xlsx) thật mà không cần thư viện ngoài.
    ///
    /// .xlsx về bản chất là 1 file ZIP chứa các file XML.
    /// Lớp này dựng tay cấu trúc tối thiểu:
    ///   [Content_Types].xml
    ///   _rels/.rels
    ///   xl/workbook.xml
    ///   xl/_rels/workbook.xml.rels
    ///   xl/worksheets/sheet1.xml
    ///
    /// Chuỗi được ghi bằng dạng "inline string" nên không cần sharedStrings.
    /// File sinh ra mở được bằng Excel, Google Sheets, LibreOffice.
    /// </summary>
    public static class ScoreExcelWriter
    {
        /// <summary>
        /// Ghi danh sách kết quả ra file .xlsx (tạo mới, ghi đè nếu đã tồn tại).
        /// </summary>
        /// <param name="filePath">Đường dẫn file .xlsx đầu ra.</param>
        /// <param name="headers">Danh sách tiêu đề cột.</param>
        /// <param name="rows">Các dòng dữ liệu.</param>
        /// <returns>True nếu ghi thành công.</returns>
        public static bool Write(string filePath, List<string> headers, List<string[]> rows)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("[ScoreExcelWriter] Đường dẫn file rỗng.");
                return false;
            }

            try
            {
                string dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Xoá file cũ nếu có để tránh lỗi "file đang bị khoá" khi ghi lại
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using (FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    WriteEntry(zip, "[Content_Types].xml", BuildContentTypes());
                    WriteEntry(zip, "_rels/.rels", BuildRootRels());
                    WriteEntry(zip, "xl/workbook.xml", BuildWorkbook());
                    WriteEntry(zip, "xl/_rels/workbook.xml.rels", BuildWorkbookRels());
                    WriteEntry(zip, "xl/styles.xml", BuildStyles());
                    WriteEntry(zip, "xl/worksheets/sheet1.xml", BuildSheet(headers, rows));
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ScoreExcelWriter] Lỗi ghi file Excel: {e.Message}");
                return false;
            }
        }

        private static void WriteEntry(ZipArchive zip, string entryName, string content)
        {
            ZipArchiveEntry entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
            using (StreamWriter writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }

        private static string BuildContentTypes()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                   "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                   "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                   "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                   "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" +
                   "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                   "</Types>";
        }

        private static string BuildRootRels()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                   "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                   "</Relationships>";
        }

        private static string BuildWorkbook()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" " +
                   "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                   "<sheets><sheet name=\"Kết quả Sinh học\" sheetId=\"1\" r:id=\"rId1\"/></sheets>" +
                   "</workbook>";
        }

        private static string BuildWorkbookRels()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                   "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                   "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>" +
                   "</Relationships>";
        }

        /// <summary>
        /// Định nghĩa style tối thiểu: style 0 = mặc định, style 1 = header in đậm nền xám.
        /// </summary>
        private static string BuildStyles()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                   "<fonts count=\"2\">" +
                   "<font><sz val=\"11\"/><color theme=\"1\"/><name val=\"Calibri\"/></font>" +
                   "<font><b/><sz val=\"11\"/><color rgb=\"FFFFFFFF\"/><name val=\"Calibri\"/></font>" +
                   "</fonts>" +
                   "<fills count=\"3\">" +
                   "<fill><patternFill patternType=\"none\"/></fill>" +
                   "<fill><patternFill patternType=\"gray125\"/></fill>" +
                   "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FF2E7D4F\"/><bgColor indexed=\"64\"/></patternFill></fill>" +
                   "</fills>" +
                   "<borders count=\"1\"><border><left/><right/><top/><bottom/><diagonal/></border></borders>" +
                   "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
                   "<cellXfs count=\"2\">" +
                   "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>" +
                   "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyAlignment=\"1\">" +
                   "<alignment horizontal=\"center\" vertical=\"center\" wrapText=\"1\"/>" +
                   "</xf>" +
                   "</cellXfs>" +
                   "</styleSheet>";
        }

        private static string BuildSheet(List<string> headers, List<string[]> rows)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
            sb.Append("<sheetData>");

            // Cố định hàng tiêu đề khi cuộn (sheetViews phải nằm trước sheetData)
            sb.Append("<sheetViews><sheetView workbookViewId=\"0\">" +
                      "<pane ySplit=\"1\" topLeftCell=\"A2\" activePane=\"bottomLeft\" state=\"frozen\"/>" +
                      "</sheetView></sheetViews>");

            // Dòng tiêu đề (nằm ở hàng 1) - dùng style 1 (in đậm, nền xanh)
            sb.Append("<row r=\"1\" ht=\"24\" customHeight=\"1\">");
            if (headers != null)
            {
                for (int c = 0; c < headers.Count; c++)
                {
                    AppendCell(sb, c + 1, 1, headers[c], null, headerStyle: true);
                }
            }
            sb.Append("</row>");

            // Các dòng dữ liệu (bắt đầu từ hàng 2)
            if (rows != null)
            {
                for (int r = 0; r < rows.Count; r++)
                {
                    int rowNumber = r + 2;
                    sb.Append($"<row r=\"{rowNumber}\">");
                    string[] row = rows[r];
                    if (row != null)
                    {
                        for (int c = 0; c < row.Length; c++)
                        {
                            AppendCell(sb, c + 1, rowNumber, row[c], NumericColumns);
                        }
                    }
                    sb.Append("</row>");
                }
            }

            sb.Append("</sheetData>");

            // Bộ lọc (autoFilter phải nằm sau sheetData)
            if (headers != null && headers.Count > 0)
            {
                sb.Append($"<autoFilter ref=\"A1:{ColumnName(headers.Count)}{(rows?.Count ?? 0) + 1}\"/>");
            }

            sb.Append("</worksheet>");
            return sb.ToString();
        }

        /// <summary>
        /// Đánh dấu các cột chứa số để ghi dạng cell số (tính toán/sắp xếp được trong Excel).
        /// Thứ tự cột khớp với <see cref="BuildHeaders"/>.
        /// </summary>
        private static readonly bool[] NumericColumns =
        {
            false, // Thời gian
            false, // Tài khoản
            false, // Chủ đề
            false, // Bài học
            false, // Hình thức
            true,  // Tổng số câu
            true,  // Số câu đúng
            true,  // Số câu sai
            true,  // Điểm
            true,  // Kỷ lục
            true,  // Số Boss vượt qua
            true,  // Tỉ lệ đúng (%)
            false  // Chi tiết
        };

        /// <summary>
        /// Ghi 1 ô. Các cột số (đánh dấu bằng <paramref name="numericColumns"/>) được ghi dạng
        /// cell số để Excel / Google Sheets tính toán và sắp xếp được.
        /// </summary>
        private static void AppendCell(StringBuilder sb, int columnIndex, int rowNumber, string value, bool[] numericColumns, bool headerStyle = false)
        {
            string cellRef = ColumnName(columnIndex) + rowNumber;
            string text = value ?? "";
            string styleAttr = headerStyle ? " s=\"1\"" : "";

            if (!headerStyle &&
                numericColumns != null &&
                columnIndex - 1 < numericColumns.Length &&
                numericColumns[columnIndex - 1] &&
                double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
            {
                sb.Append($"<c r=\"{cellRef}\"{styleAttr}><v>{number.ToString("0.####", CultureInfo.InvariantCulture)}</v></c>");
                return;
            }

            sb.Append($"<c r=\"{cellRef}\"{styleAttr} t=\"inlineStr\"><is><t xml:space=\"preserve\">");
            sb.Append(EscapeXml(text));
            sb.Append("</t></is></c>");
        }

        /// <summary>
        /// Chuyển chỉ số cột (1-based) sang ký hiệu cột của Excel: 1 -> A, 27 -> AA.
        /// </summary>
        private static string ColumnName(int index)
        {
            string name = "";
            while (index > 0)
            {
                int rem = (index - 1) % 26;
                name = (char)('A' + rem) + name;
                index = (index - 1) / 26;
            }
            return name;
        }

        private static string EscapeXml(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        /// <summary>
        /// Chuyển 1 ScoreRecord thành mảng ô theo thứ tự cột.
        /// </summary>
        public static string[] RecordToRow(ScoreRecord rec)
        {
            return new[]
            {
                rec.timestamp,
                rec.username,
                rec.chapter,
                rec.lesson,
                rec.mode,
                rec.totalQuestions.ToString(CultureInfo.InvariantCulture),
                rec.correctCount.ToString(CultureInfo.InvariantCulture),
                rec.wrongCount.ToString(CultureInfo.InvariantCulture),
                rec.score.ToString(CultureInfo.InvariantCulture),
                rec.highScore.ToString(CultureInfo.InvariantCulture),
                rec.bossesDefeated.ToString(CultureInfo.InvariantCulture),
                rec.accuracyPercent.ToString("0.0", CultureInfo.InvariantCulture),
                rec.details
            };
        }

        /// <summary>
        /// Danh sách tiêu đề cột của bảng kết quả.
        /// </summary>
        public static List<string> BuildHeaders()
        {
            return new List<string>
            {
                "Thời gian",
                "Tài khoản",
                "Chủ đề",
                "Bài học",
                "Hình thức",
                "Tổng số câu",
                "Số câu đúng",
                "Số câu sai",
                "Điểm",
                "Kỷ lục",
                "Số Boss vượt qua",
                "Tỉ lệ đúng (%)",
                "Chi tiết"
            };
        }
    }
}
