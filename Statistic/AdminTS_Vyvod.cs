using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;

using GemBox.Spreadsheet;
//using Excel = Microsoft.Office.Interop.Excel;


using StatisticCommon;
using ASUTP;

namespace Statistic
{
    public partial class PanelAdminVyvod : PanelAdmin
    {
        public class AdminTS_Vyvod : AdminTS_TG
        {
            public AdminTS_Vyvod(bool[] arMarkSavePPBRValues)
                : base(arMarkSavePPBRValues, TECComponentBase.TYPE.TEPLO)
            {
                //_tsOffsetToMoscow = HDateTime.TS_NSK_OFFSET_OF_MOSCOWTIMEZONE;
                m_SumRDGValues_PBR_0 = 0F;
            }

            protected override void /*bool*/ impRDGExcelValuesRequest()
            {
                throw new NotImplementedException();
            }

            protected override int impRDGExcelValuesResponse()
            {
                throw new NotImplementedException();
            }

            protected override void /*bool*/ expRDGExcelValuesRequest()
            {
                throw new NotImplementedException();
            }

            public override void FillListKeyTECComponentDetail(int id)
            {
                TEC tec = null;

                lock (m_lockSuccessGetData) {
                    // очистить суммарные значения
                    m_SumRDGValues_PBR_0 = 0F;
                    m_arSumRDGValues = null;

                    m_listKeyTECComponentDetail.Clear();
                    // найти ТЭЦ по 'id'
                    tec = m_list_tec.Find(t => { return t.m_id == id; });

                    if (!(tec == null))
                        //ВСЕ компоненты
                        foreach (TECComponent v in tec.ListTECComponents)
                            if (v.IsVyvod == true)
                                // параметры выводов
                                foreach (Vyvod.ParamVyvod pv in v.ListLowPointDev)
                                    if (pv.m_id_param == Vyvod.ID_PARAM.T_PV) // является параметром - температура прямая (для которого есть плановые значения)
                                                                              //m_listTECComponentIndexDetail.Add(pv.m_id);
                                        m_listKeyTECComponentDetail.Add(new FormChangeMode.KeyDevice () { Id = pv.m_id, Mode = FormChangeMode.MODE_TECCOMPONENT.TG });
                                    else
                                        ;
                            else
                                ; // не ВЫВОД
                    else
                        Logging.Logg().Error(string.Format(@"Admin_TS_Vyvod::FillListIndexTECComponent (id={0}) - не найдена ТЭЦ...", id), Logging.INDEX_MESSAGE.NOT_SET);

                    ClearListRDGValues();
                }
            }

            public double m_SumRDGValues_PBR_0;
            public RDGStruct[] m_arSumRDGValues;

            public void SummatorRDGValues()
            {
                int i = m_listKeyTECComponentDetail.IndexOf(CurrentKey)
                    , hour = 1
                    , iDiv = -1; // ' = i == 0 ? 1 : 2' общий делитель для усреднения невозможно определить, т.к. зависит от условия "> 0"

                TECComponent tc = allTECComponents.Find(comp => comp.m_id == CurrentKey.Id); // компонент - параметр вывода

                if ((i < m_listCurRDGValues.Count)
                    && (m_listCurRDGValues[i].Length > 0)) {
                    if (m_arSumRDGValues == null)
                        m_arSumRDGValues = new HAdmin.RDGStruct[m_listCurRDGValues[i].Length];
                    else
                        if (!(m_arSumRDGValues.Length == m_listCurRDGValues[i].Length))
                        throw new Exception(string.Format(@"AdminTS_Vyvod::GetSumRDGValues () - не совпадают размеры массивов (часы в сутках) с полученными данными ПБР для компонента ID={0}...", tc.m_id));
                    else
                        ;

                    if (m_curRDGValues_PBR_0 > 0) {
                        m_SumRDGValues_PBR_0 += m_curRDGValues_PBR_0;
                        m_SumRDGValues_PBR_0 /= i == 0 ? 1 : 2; // делитель для усреднения
                    } else
                        ;

                    for (hour = 0; hour < m_listCurRDGValues[i].Length; hour++) {
                        //arSumCurRDGValues[hour].pbr_number = arCurRDGValues[hour].pbr_number;
                        //if (arCurRDGValues[hour].pbr > 0) arSumCurRDGValues[hour].pbr += arCurRDGValues[hour].pbr; else ;
                        // для всех элементов д.б. одинаковые!
                        if (m_listCurRDGValues[i][hour].pmin > 0) {
                            m_arSumRDGValues[hour].pmin += m_listCurRDGValues[i][hour].pmin;
                            m_arSumRDGValues[hour].pmin /= i == 0 ? 1 : 2; // делитель для усреднения
                        } else
                            ;
                        //if (arCurRDGValues[hour].pmax > 0) arSumCurRDGValues[hour].pmax += arCurRDGValues[hour].pmax; else ;
                        // рекомендации для всех элементов д.б. одинаковые!
                        if (!(m_listCurRDGValues[i][hour].recomendation == 0F)) {
                            m_arSumRDGValues[hour].recomendation += m_listCurRDGValues[i][hour].recomendation;
                            m_arSumRDGValues[hour].recomendation /= i == 0 ? 1 : 2; // делитель для усреднения
                        } else
                            ;
                        // типы отклонений  для всех элементов д.б. одинаковые!
                        if (!(m_listCurRDGValues[i][hour].deviationPercent == m_arSumRDGValues[hour].deviationPercent))
                            m_arSumRDGValues[hour].deviationPercent = m_listCurRDGValues[i][hour].deviationPercent;
                        else
                            ;
                        // величины отклонения для всех элементов д.б. одинаковые!
                        if (m_listCurRDGValues[i][hour].deviation > 0) {
                            m_arSumRDGValues[hour].deviation += m_listCurRDGValues[i][hour].deviation;
                            m_arSumRDGValues[hour].deviation /= i == 0 ? 1 : 2; // делитель для усреднения
                        } else
                            ;
                    }
                } else
                    Logging.Logg().Error(string.Format(@"PanelAdminVyvod.AdminTS_Vyvod::SummatorRDGValues (комп.ID={0}, комп.индекс={1}) - суммирование (кол-во часов={2}) не выполнено..."
                        , tc.m_id, i, m_listCurRDGValues[i].Length)
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }

            protected override double getRDGValue_PBR_0(DataRow r, int indxTables, int cntFields)
            {
                return (double)r[indxTables * cntFields + (0 + 2)];
            }
            /// <summary>
            /// Возвратить признак выполненых пользователем изменений
            /// </summary>
            /// <returns>Признак изменений</returns>
            public override bool WasChanged()
            {
                bool bRes = false;

                //RDGStruct[] arRDGValues = null;
                int hour = -1
                    , indx = -1, cnt = m_listPrevRDGValues.Count; //m_listCurRDGValues.Count

                for (indx = 0; (indx < cnt) && (bRes == false); indx++) {
                    //arRDGValues = null;
                    //arRDGValues = new RDGStruct[m_listPrevRDGValues[indx].Length];
                    //m_listPrevRDGValues[indx].CopyTo (arRDGValues, 0);

                    for (hour = 0; hour < m_listPrevRDGValues[indx].Length; hour++)
                        m_prevRDGValues[hour].From(m_listPrevRDGValues[indx][hour]);

                    //arRDGValues = null;
                    //arRDGValues = new RDGStruct[m_listCurRDGValues[indx].Length];
                    //m_listCurRDGValues[indx].CopyTo(arRDGValues, 0);

                    for (hour = 0; hour < m_listCurRDGValues[indx].Length; hour++)
                        m_curRDGValues[hour].From(m_listCurRDGValues[indx][hour]);

                    bRes = base.WasChanged();
                }

                return bRes;
            }
            /// <summary>
            /// Тиражировать(копировать одно значение в несколько переменных) значений для всех параметров выводов
            /// </summary>
            public void ReplicateCurRDGValues()
            {
                int h = -1, indx = -1;
                TECComponent tc = null;

                indx = 0;
                foreach (HAdmin.RDGStruct[] arRDGValues in m_listCurRDGValues) {
                    tc = allTECComponents.Find(comp => comp.m_id == m_listKeyTECComponentDetail[indx].Id);

                    if ((tc.IsParamVyvod == true)
                        && ((tc.ListLowPointDev[0] as Vyvod.ParamVyvod).m_id_param == Vyvod.ID_PARAM.T_PV)
                        //&& (tc.m_bKomUchet == true)
                        )
                        for (h = 0; h < m_curRDGValues.Length; h++)
                            arRDGValues[h].From(m_curRDGValues[h], true);
                    else
                        ;
                    indx++;
                }
            }
            /// <summary>
            /// Сохранение текущих значений (ПБР + рекомендации = РДГ) для последующего изменения
            /// </summary>
            public override void CopyCurToPrevRDGValues()
            {
                base.CopyCurToPrevRDGValues();

                RDGStruct[] prevRDGValues = new RDGStruct[m_prevRDGValues.Length];
                for (int h = 0; h < m_prevRDGValues.Length; h++)
                    prevRDGValues[h].From(m_prevRDGValues[h]);
                m_listPrevRDGValues.Add(prevRDGValues);
            }
        }
    }
}
