using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;

using Excel = Microsoft.Office.Interop.Excel;
using ASUTP.Core;
using ASUTP;
using ASUTP.Database;
using System.Linq;

namespace StatisticCommon
{
    public class AdminTS_NSS : AdminTS_TG
    {
        public AdminTS_NSS(bool[] arMarkPPBRValues)
            : base(arMarkPPBRValues, TECComponentBase.TYPE.ELECTRO)
        {
        }

        protected override /*override*/ int impRDGExcelValuesResponse()
        {
            //bool bRes = base.ImpRDGExcelValuesResponse();
            int iRes = IsCanUseTECComponents == true ? 0 : -1;
            int rowOffsetData = 0;

            if (iRes == 0)
            {
                int i = -1
                    , iTimeZoneOffset =
                        //allTECComponents[indxTECComponents].tec.m_timezone_offset_msc
                        HDateTime.TS_NSK_OFFSET_OF_MOSCOWTIMEZONE.Hours
                    , rowRDGExcelStart = 1 + iTimeZoneOffset,
                    hour = -1;

                if (m_tableRDGExcelValuesResponse.Rows.Count > 0) iRes = 0; else ;

                if (iRes == 0)
                {
                    for (i = rowRDGExcelStart; i < m_tableRDGExcelValuesResponse.Rows.Count - rowOffsetData; i++)
                    {
                        hour = i - iTimeZoneOffset;
                        setRDGExcelValuesItem(out m_curRDGValues[hour - 1], i);
                    }

                    /*for (i = hour; i < 24 + 1; i++)
                    {
                        hour = i;

                        m_curRDGValues.plan[hour - 1] = 0;
                        m_curRDGValues.recommendations[hour - 1] = 0;
                        m_curRDGValues.deviationPercent[hour - 1] = false;
                        m_curRDGValues.diviation[hour - 1] = 0;
                    }*/
                }
                else
                    ;
            }
            else
                throw new InvalidOperationException ("AdminTS_NSS::impRDGExcelValuesResponse () - нет компонентов ТЭЦ...");

            RDGStruct [] curRDGValues = new RDGStruct[m_curRDGValues.Length];

            m_curRDGValues.CopyTo(curRDGValues, 0);

            m_listCurRDGValues.Add(curRDGValues);

            return iRes;
        }

        protected override void /*bool*/ impRDGExcelValuesRequest()
        {
            Logging.Logg().Debug(@"AdminTS_NSS::ImpRDGExcelValuesRequest () - вХод...", Logging.INDEX_MESSAGE.NOT_SET);

            //bool bRes = true;
            int err = 0
                , i = -1, j = -1
                , rowOffsetData = 2
                , iTimeZoneOffset =
                    //allTECComponents[indxTECComponents].tec.m_timezone_offset_msc
                    HDateTime.TS_NSK_OFFSET_OF_MOSCOWTIMEZONE.Hours
                    ;
            string path_rdg_excel = CurrentDevice.tec.GetAddingParameter(TEC.ADDING_PARAM_KEY.PATH_RDG_EXCEL).ToString(),
                strSelect = @"SELECT * FROM [Лист1$]";
            object[] dataRowAddIn = null;

            DataTable tableRDGExcelValuesNextDay;
            if (!(m_tableRDGExcelValuesResponse == null)) m_tableRDGExcelValuesResponse.Clear(); else ;

            Logging.Logg().Debug(@"AdminTS_NSS::ImpRDGExcelValuesRequest () - path_rdg_excel=" + path_rdg_excel + @", nameFileRDGExcel=" + nameFileRDGExcel(m_curDate.Date), Logging.INDEX_MESSAGE.NOT_SET);

            delegateStartWait();
            if ((IsCanUseTECComponents == true)
                && (path_rdg_excel.Length > 0))
            {
                try { m_tableRDGExcelValuesResponse = DbTSQLInterface.Select(path_rdg_excel + "\\" + nameFileRDGExcel(m_curDate.Date) + ".xls", strSelect, out err); }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"AdminTS_NSS::ImpRDGExcelValuesRequest () - DbTSQLInterface.Select (" + strSelect + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                if (!(m_tableRDGExcelValuesResponse == null))
                {
                    Logging.Logg().Debug(@"AdminTS_NSS::ImpRDGExcelValuesRequest () - m_tableRDGExcelValuesResponse.Rows.Count=" + m_tableRDGExcelValuesResponse.Rows.Count, Logging.INDEX_MESSAGE.NOT_SET);

                    if (m_tableRDGExcelValuesResponse.Rows.Count > 0)
                    {
                        while (m_tableRDGExcelValuesResponse.Rows[m_tableRDGExcelValuesResponse.Rows.Count - 1][1] is DBNull)
                            m_tableRDGExcelValuesResponse.Rows.RemoveAt(m_tableRDGExcelValuesResponse.Rows.Count - 1);

                        //if (File.Exists(path_rdg_excel + "\\" + m_curDate.Date.AddDays(1).GetDateTimeFormats()[4] + ".xls") == true)
                        if (File.Exists(path_rdg_excel + "\\" + m_curDate.Date.AddDays(1).ToString(@"yyyyMMdd") + ".xls") == true)
                        {
                            tableRDGExcelValuesNextDay = DbTSQLInterface.Select(path_rdg_excel + "\\" + m_curDate.Date.AddDays(1).ToString(@"yyyyMMdd") + ".xls", strSelect, out err);
                            if (tableRDGExcelValuesNextDay.Rows.Count > 0)
                            {
                                while (tableRDGExcelValuesNextDay.Rows[tableRDGExcelValuesNextDay.Rows.Count - 1][1] is DBNull)
                                    tableRDGExcelValuesNextDay.Rows.RemoveAt(tableRDGExcelValuesNextDay.Rows.Count - 1);

                                for (i = 0; i < iTimeZoneOffset; i++)
                                {
                                    dataRowAddIn = new object[m_tableRDGExcelValuesResponse.Columns.Count];

                                    for (j = 0; j < m_tableRDGExcelValuesResponse.Columns.Count; j++)
                                    {
                                        dataRowAddIn.SetValue(tableRDGExcelValuesNextDay.Rows[i + rowOffsetData - 1][j], j); //"-1" т.к. заголовок для OleDb не существует
                                    }

                                    m_tableRDGExcelValuesResponse.Rows.Add(dataRowAddIn); //Т.к.
                                }
                            }
                            else
                                ;

                            tableRDGExcelValuesNextDay.Clear();
                        }
                        else
                            ;
                    }
                    else
                        ;
                }
                else
                    Logging.Logg().Debug(@"AdminTS_NSS::ImpRDGExcelValuesRequest () - m_tableRDGExcelValuesResponse=null", Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
                throw new InvalidOperationException ("AdminTS::impRDGExcelValuesRequest () - нет компонентов ТЭЦ путь импорта не указан...");

            //Logging.Logg ().LogLock ();
            //Logging.Logg().Send("Admin.cs - GetRDGExcelValuesRequest () - (path_rdg_excel = " + path_rdg_excel + ")", false, false, false);
            //Logging.Logg().LogUnlock();

            delegateStopWait ();

            Logging.Logg().Debug(@"AdminTS_NSS::ImpRDGExcelValuesRequest () - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);

            //return bRes;
        }

        protected override void /*bool*/ expRDGExcelValuesRequest()
        {
            //bool bRes = true;
            Boolean bMidnightValues = false;
            int err = 0
                , i = -1
                , rowOffsetData = 2
                , rowOffsetNextDay = 0
                //, iMidnightValues = 0
                , iTimeZoneOffset =
                    //allTECComponents[m_listTECComponentIndexDetail[0/*любой из индексов, т.к. они принадлежат одной ТЭЦ*/]].tec.m_timezone_offset_msc
                    HDateTime.TS_NSK_OFFSET_OF_MOSCOWTIMEZONE.Hours
                    ;
            string path_rdg_excel = allTECComponents.ToList().Find(c => c.m_id == m_listKeyTECComponentDetail [0/*любой из индексов, т.к. они принадлежат одной ТЭЦ*/].Id).tec.GetAddingParameter(TEC.ADDING_PARAM_KEY.PATH_RDG_EXCEL).ToString(),
                strUpdate = string.Empty;
            TECComponentBase comp;

            if ((IsCanUseTECComponents == true)
                && (path_rdg_excel.Length > 0))
            {
                Excel.Application excelApp = new Excel.Application();
                Excel.Workbook excelAppWorkbook;
                Excel.Worksheet excelAppWorksheet;
                Excel.Range excelAppCellA1, excelAppWorkcell;

                excelApp.Visible = false;

                Boolean bPrevExcelAppDisplayAlerts = excelApp.DisplayAlerts;
                excelApp.DisplayAlerts = false;

                string nameExcelBook = nameFileRDGExcel(m_curDate.Date) + ".xls",
                        pathExcelBook = path_rdg_excel + "\\" + nameExcelBook;

                i = 0;
                while (File.Exists(pathExcelBook) == false)
                {
                    i--;

                    nameExcelBook = nameFileRDGExcel(m_curDate.Date.AddDays(i)) + ".xls";
                    pathExcelBook = path_rdg_excel + "\\" + nameExcelBook;
                }

                excelApp.Workbooks.Open(pathExcelBook,
                                        Type.Missing,
                                        false,
                                        Type.Missing,
                                        Type.Missing,
                                        "nss",
                                        true);

                if (i < 0)
                {//Файл искали - необходимо созлать нпа сегодняшний день
                    nameExcelBook = nameFileRDGExcel(m_curDate.Date) + ".xls";
                    pathExcelBook = path_rdg_excel + "\\" + nameExcelBook;

                    excelApp.Workbooks[1].SaveAs(pathExcelBook, Type.Missing, Type.Missing, "nss", false, false);
                }
                else
                    ;

                excelAppWorkbook = excelApp.Workbooks[1];
                excelAppWorksheet = (Excel.Worksheet)excelAppWorkbook.Worksheets[1];
                excelAppCellA1 = excelAppWorksheet.get_Range("A1", Type.Missing);

                for (i = 0; i < 24; i++)
                {
                    strUpdate = string.Empty;

                    if ((i + iTimeZoneOffset) < 24)
                    {
                    }
                    else
                    {
                        if (rowOffsetNextDay == 0)
                        {
                            //Проверить существование книги
                            nameExcelBook = nameFileRDGExcel(m_curDate.Date.AddDays(1)) + ".xls";
                            pathExcelBook = path_rdg_excel + "\\" + nameExcelBook;

                            if (File.Exists(pathExcelBook) == false)
                                excelApp.Workbooks[1].SaveAs(pathExcelBook, Type.Missing, Type.Missing, "nss", false, false);
                            else
                            {
                                excelApp.Windows[1].Close(true, excelApp.Workbooks[1].FullName, false);

                                excelApp.Workbooks.Open(pathExcelBook,
                                                        Type.Missing,
                                                        false,
                                                        Type.Missing,
                                                        Type.Missing,
                                                        "nss",
                                                        true);

                                excelAppWorkbook = excelApp.Workbooks[1];
                                excelAppWorksheet = (Excel.Worksheet)excelAppWorkbook.Worksheets[1];
                                excelAppCellA1 = excelAppWorksheet.get_Range("A1", Type.Missing);
                            }

                            rowOffsetNextDay = 24;
                        }
                        else
                            ;
                    }

                    foreach (RDGStruct[] curRDGValues in m_listCurRDGValues)
                    {
                        CurrentKey = m_listKeyTECComponentDetail[m_listCurRDGValues.IndexOf(curRDGValues)];
                        comp = null;

                        //strUpdate += @"'A";
                        //strUpdate += @"A";
                        if (m_listKeyTECComponentDetail[m_listCurRDGValues.IndexOf(curRDGValues)].Mode == FormChangeMode.MODE_TECCOMPONENT.GTP)
                        {
                            try {
                                comp = CurrentDevice as TECComponentBase;
                            } catch (Exception e) {
                                Logging.Logg ().Exception (e, "преобразование IDevice -> TECComponentBase...", Logging.INDEX_MESSAGE.NOT_SET);
                            }
                        }
                        else
                            if (m_listKeyTECComponentDetail[m_listCurRDGValues.IndexOf(curRDGValues)].Mode == FormChangeMode.MODE_TECCOMPONENT.TG)
                            {
                                //strUpdate += allTECComponents[indxTECComponents].TG [0].m_indx_col_rdg_excel;
                                comp = CurrentDevice.ListLowPointDev[0];
                            }
                            else ;

                        //strUpdate += @"'='" + (curRDGValues[i].pbr - curRDGValues[i].recomendation) + @"', ";
                        //strUpdate += @"='" + (curRDGValues[i].pbr - curRDGValues[i].recomendation) + @"', ";

                        if (!(comp == null))
                        {
                            excelAppWorkcell = excelAppCellA1.get_Offset(rowOffsetData + i + iTimeZoneOffset - rowOffsetNextDay, comp.m_indx_col_rdg_excel - 1);

                            try { excelAppWorkcell.Value = curRDGValues[i].pbr - curRDGValues[i].recomendation; }
                            catch (Exception e)
                            {
                                Logging.Logg().Exception(e, "excelAppWorkcell.set_Value () - ...", Logging.INDEX_MESSAGE.NOT_SET);

                                i = 24;
                                break;
                            }

                            if (!(rowOffsetNextDay == 0) && (bMidnightValues == false))
                            {//Выполняется ОДИН раз (bMidnightValues - флаг) для всех компонентов
                                excelAppWorkcell = excelAppCellA1.get_Offset(rowOffsetData - 1, comp.m_indx_col_rdg_excel - 1);

                                try { excelAppWorkcell.Value = curRDGValues[24 - iTimeZoneOffset].pbr - curRDGValues[24 - iTimeZoneOffset].recomendation; }
                                catch (Exception e)
                                {
                                    Logging.Logg().Exception(e, "excelAppWorkcell.set_Value () - ...", Logging.INDEX_MESSAGE.NOT_SET);

                                    i = 24;
                                    break;
                                }
                            }
                            else
                                ;
                        }
                        else
                            ;
                    }

                    if (!(rowOffsetNextDay == 0) && (bMidnightValues == false))
                    {//Выполняется ОДИН раз (bMidnightValues - флаг) для всех компонентов
                        bMidnightValues = true;
                    }
                    else
                        ;

                    //strUpdate = strUpdate.Substring (0, strUpdate.Length - 2);

                    //strUpdate = @"UPDATE [Лист1$] SET " + strUpdate + @" WHERE A" + (i + 2) + "='" + ((i.ToString ().Length < 2) ? @"0" + i.ToString () : i.ToString ()) + @"'";
                    //strUpdate = @"UPDATE [Лист1$] SET " + strUpdate + @" WHERE 1='" + ((i.ToString().Length < 2) ? @"0" + i.ToString() : i.ToString()) + @"'";
                }

                excelApp.Windows[1].Close(true, pathExcelBook, false);

                excelApp.DisplayAlerts = bPrevExcelAppDisplayAlerts;
                excelApp.Quit();

                //base.ExpRDGExcelValuesRequest();
            }
            else
                throw new InvalidOperationException ("AdminTS::expRDGExcelValuesRequest () - нет компонентов ТЭЦ путь экспорта не указан...");

            //return bRes;
        }
    }
}
