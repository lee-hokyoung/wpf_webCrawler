using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NsExcel = Microsoft.Office.Interop.Excel;

namespace webCrawler.Contoller
{
    public static class ExcelDownLoad
    {
        public  static void  fnExcelDownLoad(DataGrid dgMyDB, List<ViewModel.MyDBViewModel> myDBView_list)
        {
            var excel = new NsExcel.Application();
            excel.Visible = false;
            //excel.DisplayAlerts = false;
            var excelWorkBook = excel.Workbooks.Add(Type.Missing);
            var excelSheet = (NsExcel.Worksheet)excelWorkBook.ActiveSheet;
            try
            {
                int count = myDBView_list.Count(n => n.IsSelected == true);

                int r = myDBView_list.Count(n => n.IsSelected == true) + 1;
                //int c = dgMyDB.Columns.Count;
                int c = 127;
                var data = new object[r, c];

                // 헤더 설정
                data[0, 0] = "고객사 상품코드";
                data[0, 1] = "상품명(*)";
                data[0, 2] = "상품약어";
                data[0, 3] = "샵링커카테고리";
                data[0, 4] = "모델명";
                data[0, 5] = " 모델명No ";
                data[0, 6] = " 시작가격 ";
                data[0, 7] = " 공급가격(*) ";
                data[0, 8] = " 판매가격(*) ";
                data[0, 9] = " 시중가격(*) ";
                data[0, 10] = "공급가능수량(*)";
                data[0, 11] = "과세(*)";
                data[0, 12] = "매입처아이디";
                data[0, 13] = "제조사(*)";
                data[0, 14] = "원산지(*)";
                data[0, 15] = "판매지역(*)";
                data[0, 16] = "남여구분(*)";
                data[0, 17] = "판매상태(*)";
                data[0, 18] = "옵션명1";
                data[0, 19] = "옵션항목1";
                data[0, 20] = "옵션명2";
                data[0, 21] = "옵션항목2";
                data[0, 22] = "옵션명3";
                data[0, 23] = "옵션항목3";
                data[0, 24] = "상품이미지(*)";
                data[0, 25] = "배송비부과여부(*)";
                data[0, 26] = "배송비";
                data[0, 27] = "상품요약설명";
                data[0, 28] = "상품상세설명(*)";
                data[0, 29] = "신 상세설명";
                data[0, 30] = "추가구성 상세";
                data[0, 31] = "광고홍보 상세설명";
                data[0, 32] = "브랜드명";
                data[0, 33] = "고객사      대분류코드";
                data[0, 34] = "고객사      중분류코드";
                data[0, 35] = "고객사      소분류코드";
                data[0, 36] = "고객사      세분류코드";
                data[0, 37] = " 매입처공급가 ";
                data[0, 38] = " 매입처판매가 ";
                data[0, 39] = " 매입처시중가 ";
                data[0, 40] = "옥션/지마켓     상품이미지";
                data[0, 41] = "쿠팡 외상품이미지";
                data[0, 42] = "11번가목록      상품이미지";
                data[0, 43] = "종합몰 홈쇼핑 상품이미지";
                data[0, 44] = "부가이미지6";
                data[0, 45] = "부가이미지7";
                data[0, 46] = "부가이미지8";
                data[0, 47] = "부가이미지9";
                data[0, 48] = "부가이미지10";
                data[0, 49] = "부가이미지11";
                data[0, 50] = "부가이미지12";
                data[0, 51] = "옥션&지마켓 추가이미지1";
                data[0, 52] = "옥션&지마켓 추가이미지2";
                data[0, 53] = "위메프 (460 * 460, 500 * 500)";
                data[0, 54] = "위메프 (580 * 320)";
                data[0, 55] = "발행일/제조일";
                data[0, 56] = "W컨셉 기본이미지(480 * 640)";
                data[0, 57] = "인증번호";
                data[0, 58] = "롯데홈 (81*81)";
                data[0, 59] = "롯데홈 (110*110)";
                data[0, 60] = "롯데홈 (190*190)";
                data[0, 61] = "상품유효기간";
                data[0, 62] = "성인상품여부";
                data[0, 63] = "반품지주소";
                data[0, 64] = "반품지우편번호";
                data[0, 65] = "출하지주소";
                data[0, 66] = "출하지우편번호";
                data[0, 67] = "품목고시 코드";
                data[0, 68] = "품목 값1";
                data[0, 69] = "품목 값2";
                data[0, 70] = "품목 값3";
                data[0, 71] = "품목 값4";
                data[0, 72] = "품목 값5";
                data[0, 73] = "품목 값6";
                data[0, 74] = "품목 값7";
                data[0, 75] = "품목 값8";
                data[0, 76] = "품목 값9";
                data[0, 77] = "품목 값10";
                data[0, 78] = "품목 값11";
                data[0, 79] = "품목 값12";
                data[0, 80] = "품목 값13";
                data[0, 81] = "품목 값14";
                data[0, 82] = "품목 값15";
                data[0, 83] = "인증항목1";
                data[0, 84] = "인증기관명1";
                data[0, 85] = "인증번호1";
                data[0, 86] = "인증신고번호1";
                data[0, 87] = "인증발급시작일자1";
                data[0, 88] = "인증유효시작일자1";
                data[0, 89] = "인증유효만료일자1";
                data[0, 90] = "인증이미지1";
                data[0, 91] = "인증항목2";
                data[0, 92] = "인증기관명2";
                data[0, 93] = "인증번호2";
                data[0, 94] = "인증신고번호2";
                data[0, 95] = "인증발급시작일자2";
                data[0, 96] = "인증유효시작일자2";
                data[0, 97] = "인증유효만료일자2";
                data[0, 98] = "인증이미지2";
                data[0, 99] = "인증항목3";
                data[0, 100] = "인증기관명3";
                data[0, 101] = "인증번호3";
                data[0, 102] = "인증신고번호3";
                data[0, 103] = "인증발급시작일자3";
                data[0, 104] = "인증유효시작일자3";
                data[0, 105] = "인증유효만료일자3";
                data[0, 106] = "인증이미지3";
                data[0, 107] = "인증항목4";
                data[0, 108] = "인증기관명4";
                data[0, 109] = "인증번호4";
                data[0, 110] = "인증신고번호4";
                data[0, 111] = "인증발급시작일자4";
                data[0, 112] = "인증유효시작일자4";
                data[0, 113] = "인증유효만료일자4";
                data[0, 114] = "인증이미지4";
                data[0, 115] = "인증항목5";
                data[0, 116] = "인증기관명5";
                data[0, 117] = "인증번호5";
                data[0, 118] = "인증신고번호5";
                data[0, 119] = "인증발급시작일자5";
                data[0, 120] = "인증유효시작일자5";
                data[0, 121] = "인증유효만료일자5";
                data[0, 122] = "인증이미지5";
                data[0, 123] = "하프클럽 가로배너 이미지 GS / 이지웰 모바일 이미지 B쇼핑 MC이미지";
                data[0, 124] = "품질표시 TAG";
                data[0, 125] = "무게";
                data[0, 126] = "원본url";

                int i = 0;
                string html = "";
                foreach (ViewModel.MyDBViewModel row in myDBView_list)
                {
                    if (row.IsSelected)
                    {
                        html = "";
                        ++i;
                        data[i, 0] = row.Id;
                        data[i, 1] = row.Prd_name;
                        data[i, 7] = row.Prd_price;
                        data[i, 8] = row.Prd_promo;
                        data[i, 9] = row.Prd_promo;
                        data[i, 10] = row.Prd_stock;
                        data[i, 11] = "과세";
                        data[i, 13] = row.Prd_brand;
                        data[i, 14] = "중국";
                        data[i, 15] = "전국";
                        data[i, 17] = "판매중";

                        data[i, 18] = row.Opt_1;
                        data[i, 19] = row.Opt_val_1;
                        data[i, 20] = row.Opt_2;
                        data[i, 21] = row.Opt_val_2;
                        data[i, 22] = row.Opt_3;
                        data[i, 23] = row.Opt_val_3;

                        data[i, 24] = row.Prd_img.ToString();

                        if(row.Opt_imgs != "")
                        {
                            html += "<div>";
                            foreach (var item in row.Opt_imgs.Split(','))
                            {
                                html += "<div style='max-width:31%; display: inline-block; padding: .5rem;'>";
                                html += "<img src='https:" + item.Split(new string[] { "^^" }, StringSplitOptions.None)[0] + "' style='width:100%;'>";
                                if(item.Split(new string[] { "^^" }, StringSplitOptions.None).Length > 1) html += "<p style='margin-top:.5rem; text-align:center; font-weight:bold;'>" + item.Split(new string[] { "^^" }, StringSplitOptions.None)[1] + "</p>";
                                html += "</div>";
                            }
                            html += "</div>";
                        }

                        data[i, 28] = html + row.Detail_img;       // 옵션이미지 태그 + 상세페이지 태그
                        data[i, 29] = html + row.Detail_img;

                        data[i, 44] = row.Add_img_1;
                        data[i, 45] = row.Add_img_2;
                        data[i, 46] = row.Add_img_3;
                        data[i, 47] = row.Add_img_4;
                        data[i, 126] = "https://detail.tmall.com/item.htm?id=" + row.Id;
                    }
                }
                excelSheet.Range[excelSheet.Cells[1, 1], excelSheet.Cells[r, c]].Value2 = data;
                excel.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "generateExcel");
            }
            finally
            {
                Marshal.ReleaseComObject(excelSheet);
                Marshal.ReleaseComObject(excelWorkBook);
                Marshal.ReleaseComObject(excel);
            }
        }
    }
}
