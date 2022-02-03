using StatisticCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace TestFunc {
    class ImportCSV {
        public ImportCSV ()
        {
            DataTable m_tablePPBRValuesResponse = null;
            string m_fullPathPPBRCSVValue = @"D:" //Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])
                 + @"\" + @"ГТП(генерация) Сессия(13) от 24.12.2014.csv";

            int err = -1;
            string strPPBRCSVNameFileTemp = Path.GetFileNameWithoutExtension (m_fullPathPPBRCSVValue)
                , strMes = string.Empty;
            ;

            strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace ("(", string.Empty);
            strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace (")", string.Empty);
            strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace (".", string.Empty);
            strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace (" ", string.Empty);
            strPPBRCSVNameFileTemp = Path.GetDirectoryName (m_fullPathPPBRCSVValue) + @"\" +
                                    strPPBRCSVNameFileTemp +
                                    Path.GetExtension (m_fullPathPPBRCSVValue);

            ////при аргументе = каталог размещения наборов
            //strPPBRCSVNameFile = m_PPBRCSVDirectory + strPPBRCSVNameFile + strCSVExt;
            //strPPBRCSVNameFileTemp = m_PPBRCSVDirectory + strPPBRCSVNameFileTemp + strCSVExt;

            //File.Copy(strPPBRCSVNameFile, strPPBRCSVNameFileTemp, true);
            File.Copy (m_fullPathPPBRCSVValue, strPPBRCSVNameFileTemp, true);

            StreamReader sr = new StreamReader (strPPBRCSVNameFileTemp);
            string cont = sr.ReadToEnd ().Replace (',', '.');
            sr.Close ();
            sr.Dispose ();
            StreamWriter sw = new StreamWriter (strPPBRCSVNameFileTemp);
            sw.Write (cont);
            sw.Flush ();
            sw.Close ();
            sw.Dispose ();

            if (!(m_tablePPBRValuesResponse == null))
                m_tablePPBRValuesResponse.Clear ();
            else
                ;

            if (strPPBRCSVNameFileTemp.Length > 0)
                m_tablePPBRValuesResponse = ASUTP.Database.DbTSQLInterface.Select (@"CSV_DATASOURCE=" + Path.GetDirectoryName (strPPBRCSVNameFileTemp),
                                                                        @"SELECT * FROM ["
                                                                        //+ @"Sheet1$"
                                                                        + Path.GetFileName (strPPBRCSVNameFileTemp)
                                                                        + @"]"
                                                                        //+ @" WHERE GTP_ID='" +
                                                                        //allTECComponents[indxTECComponents].name_future +
                                                                        //@"'"
                                                                        , out err);
            else
                ;

            File.Delete (strPPBRCSVNameFileTemp);

            int hour = -1;
            double val = -1F;
            HAdmin.RDGStruct [] curRDGValues = new HAdmin.RDGStruct [24];
            string name_future = @"GNOVOS02"
                , pbr_number = string.Empty;
            //Получить значения для сохранения
            DataRow [] rowsTECComponent = m_tablePPBRValuesResponse.Select (@"GTP_ID='" + name_future + @"'");
            //Проверить наличие записей для ГТП
            if (rowsTECComponent.Length > 0) {
                foreach (DataRow r in rowsTECComponent) {
                    hour = int.Parse (r [@"SESSION_INTERVAL"].ToString ());

                    if (double.TryParse (r [@"TotalBR"].ToString (), out val) == false)
                        if (hour > 0)
                            curRDGValues [hour].pbr = curRDGValues [hour - 1].pbr;
                        else
                            ;
                    else
                        curRDGValues [hour].pbr = val;

                    curRDGValues [hour].pmin = double.Parse (r [@"PminBR"].ToString ());
                    curRDGValues [hour].pmax = double.Parse (r [@"PmaxBR"].ToString ());

                    strMes = String.Format (@"Час: {0}\tПБР={1}\tПБРмин={2}\tПБРмакс={3}",
                                        hour.ToString (@"00"), curRDGValues [hour].pbr, curRDGValues [hour].pmin, curRDGValues [hour].pmax);
                    Console.WriteLine (strMes);

                    curRDGValues [hour].pbr_number = pbr_number;
                }
            } else
                ;
        }
    }
}
