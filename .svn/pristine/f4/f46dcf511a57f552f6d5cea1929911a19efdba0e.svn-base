using ASUTP;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using ModesApiExternal;
using ASUTP.Core;
using Modes;
using StatisticTransModes;

namespace trans_mc
{
    class ParseMCGetData : IDisposable
    {
        private IApiExternal _mcApi;
        //private Modes.BusinessLogic.IModesTimeSlice _modesTimeSlice;
        private IList<PlanFactorItem> _mcListPFI;

        private string []_arguments;

        private DbMCInterface.Operation _operation;

        private Func<int, Modes.BusinessLogic.IGenObject> delegateFunctionFindIGO;

        private Func<int, Modes.BusinessLogic.IGenObject> delegateFunctionAddIGO;

        private Action<FillErrorEventArgs> delegateGetData_OnFillError;

        public ParseMCGetData(IApiExternal mcApi
            //, Modes.BusinessLogic.IModesTimeSlice modesTimeSlice
            , IList<PlanFactorItem> listPFI
            , object query
            , Func<int, Modes.BusinessLogic.IGenObject> fFindIGO
            , Func<int, Modes.BusinessLogic.IGenObject> fAddIGO
            , Action<FillErrorEventArgs> fGetData_OnFillError
            )
        {
            _operation = DbMCInterface.Operation.Unknown;

            _mcApi = mcApi;
            //_modesTimeSlice = modesTimeSlice;
            _mcListPFI = listPFI;
            delegateFunctionFindIGO = fFindIGO;
            delegateFunctionAddIGO = fAddIGO;
            delegateGetData_OnFillError = fGetData_OnFillError;

            _arguments = ((string)query).Split (';');
            Enum.TryParse<DbMCInterface.Operation> (_arguments [0], out _operation);
        }

        public bool Result(DataTable table)
        {
            bool bRes = false;

            switch (_operation) {
                case DbMCInterface.Operation.InitIGO:
                    bRes = fInitIGO (table);
                    break;
                case DbMCInterface.Operation.MaketEquipment:
                    bRes = fMaketEquipment (table, _arguments [1].Split (','), DateTime.FromOADate (Double.Parse (_arguments [2])));
                    break;
                case DbMCInterface.Operation.PPBR:
                    bRes = fPPBR (table, _arguments[1].Split(','), DateTime.FromOADate (Double.Parse (_arguments [2])));
                    break;
                default:
                    break;
            }

            return bRes;
        }

        private bool fInitIGO (DataTable table)
        {
            bool bRes = false;

            return bRes;
        }

        private bool fMaketEquipment (DataTable table, string[] listGuids, DateTime date)
        {
            bool bRes = false;

            Modes.BusinessLogic.IGenObject igo;
            IList<Modes.BusinessLogic.IMaketEquipment> listEquip;

            table.Columns.Add ("ID", typeof (int));
            table.Columns.Add ("ID_INNER", typeof (int));
            table.Columns.Add ("WR_DATE_TIME", typeof (DateTime));

            listEquip = _mcApi.GetMaket53500Equipment (Array.ConvertAll<string, Guid> (listGuids, Guid.Parse));

            listEquip.ToList ().ForEach (equip => {
                string mesDebug = string.Empty;

                mesDebug = $@"DbMCInterface::GetData () - equip=[{equip.Mrid}], children.Count={equip.GenTree.Count}";

                equip.GenTree.ToList ().ForEach (item => {
                    mesDebug = $"{mesDebug}{Environment.NewLine}, item.IdIner ={item.IdInner}, item.GenObjType.Id={item.GenObjType.Id}, item.Name={item.Name}, item.children.Count={item.Children.Count}...";

                    item.Children.ToList ().ForEach (child => {
                        mesDebug = $"{mesDebug}{Environment.NewLine}, children.IdIner ={child.IdInner}, child.GenObjType.Id={child.GenObjType.Id}, child.Name={child.Name}, child.children.Count={child.Children.Count}...";
                    });

                    igo = null;
                    //igo = findIGO (item.IdInner);
                    //if (Equals (igo, null) == false)
                    //    if (item.GenObjType.Id == ID_GEN_OBJECT_TYPE.RGE)
                    //        m_listIGO.Add (item);
                    //    else
                    //        ;
                    //else
                    //    ;

                    if (Equals (igo, null) == false)
                        table.Rows.Add (new object [] { igo.Id, igo.IdInner, HDateTime.ToMoscowTimeZone () });
                    else
                        ;
                }); // equip.GenTree.ToList ().ForEach

                Logging.Logg ().Debug (mesDebug, Logging.INDEX_MESSAGE.NOT_SET);
            });

            //TODO:
            bRes = true;

            return bRes;
        }

        private bool fPPBR (DataTable table, string []idsInner, DateTime date)
        {
            bool bRes = false;

            Modes.BusinessLogic.IGenObject igo;
            int i = -1
                , idInner = -1;
            bool valid = false;
            IList<PlanValueItem> listPVI = null;
            DataRow [] ppbr_rows = null;
            string pbr_number = string.Empty
                , pbr_indx;
            DateTime dateCurrent;
            INDEX_PLAN_FACTOR indx_plan_factor = INDEX_PLAN_FACTOR.Unknown;

            table.Columns.Add ("DATE_PBR", typeof (DateTime));
            table.Columns.Add ("WR_DATE_TIME", typeof (DateTime));
            table.Columns.Add (INDEX_PLAN_FACTOR.PBR.ToString(), typeof (double));
            table.Columns.Add (INDEX_PLAN_FACTOR.Pmin.ToString (), typeof (double));
            table.Columns.Add (INDEX_PLAN_FACTOR.Pmax.ToString (), typeof (double));
            table.Columns.Add ("PBR_NUMBER", typeof (string));
            //table.Columns.Add("ID_COMPONENT", typeof(Int32));

            for (i = 0; i < idsInner.Length; i++) {
                bRes = false;
                valid = Int32.TryParse (idsInner [i], out idInner);

                if (valid == false)
                    continue;
                else
                    ;

                igo = delegateFunctionFindIGO (idInner);

                if (igo == null)
                    igo = delegateFunctionAddIGO (idInner);
                else
                    ;

                if (!(igo == null)) {
                    try {
                        listPVI = _mcApi.GetPlanValuesActual (date.LocalHqToSystemEx (), date.AddDays (1).LocalHqToSystemEx (), igo);

                        bRes = true;
                    } catch (Exception e) {
                        Logging.Logg ().Exception (e, string.Format (@"DbMCInterface::GetData () - GetPlanValuesActual () - получение значений для '{0}', [IdInner={1}]..."
                                , igo.Description, igo.IdInner)
                            , Logging.INDEX_MESSAGE.NOT_SET);

                        delegateGetData_OnFillError (new FillErrorEventArgs (table, new object [] { }));
                    }

                    if (bRes == true)
                        if (listPVI.Count == 0)
                            Logging.Logg ().Warning (string.Format ("DbMCInterface::GetData () - GetPlanValuesActual () - нет параметров генерации для '{0}', [ID={1}]..."
                                    , igo.Description, igo.IdInner)
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        else
                            ;
                    else
                    //ОШИБКА получения значений!
                        break;

                    foreach (PlanValueItem pvi in listPVI.OrderBy (RRR => RRR.DT)) {
                        //Console.WriteLine ("    " + pvi.DT.SystemToLocalHqEx ().ToString () + " " +
                        //    pvi.Type.ToString () + " [" + _mcListPFI [pvi.ObjFactor].Description + "] " +
                        //    /*it.ObjName это id генерирующего объекта*/
                        //    _mcListPFI [pvi.ObjFactor].Name + " =" + pvi.Value.ToString ());

                        dateCurrent = pvi.DT.SystemToLocalHqEx ();
                        pbr_number = pvi.Type.ToString ().IndexOf (StatisticCommon.HAdmin.PBR_PREFIX) < 0
                            ? StatisticCommon.HAdmin.PBR_PREFIX + pvi.Type.ToString ()
                                : pvi.Type.ToString ();

                        //Получение записи с другими параметрами за это время
                        ////Вариант №1
                        //if (srtListPPBR.ContainsKey(dateCurrent))
                        //    ppbr = srtListPPBR.First(item => item.Key == dateCurrent).Value;
                        //else
                        //    ;

                        //Вариант №2
                        if (table.Rows.Count > 0)
                            ppbr_rows = table.Select ("DATE_PBR='" + dateCurrent.ToString () + "'");
                        else
                            ;

                        //Обработка получения записи
                        ////Вариант №1
                        //if (ppbr == null)
                        //{
                        //    ppbr = new PPBR_Record();
                        //    ppbr.date_time = dateCurrent;
                        //    ppbr.wr_date_time = DateTime.Now;
                        //    ppbr.PBR_number = pvi.Type.ToString();
                        //    //ppbr.idInner = igo.IdInner; //??? Для TEC5_TG36 2 объекта (TG34 + TG56), т.е. 2 IGO

                        //    //После добавления можно продолжать модифицировать экземпляр класса - в коллекции та же самая ссылка хранится.
                        //    srtListPPBR.Add(dateCurrent, ppbr);
                        //}
                        //else
                        //    ;

                        //Вариант №2, 3
                        if ((ppbr_rows == null)
                            || (ppbr_rows.Length == 0)) {
                            table.Rows.Add (new object [] { dateCurrent, DateTime.Now, 0F, 0F, 0F, pbr_number/*, igo.IdInner*/});
                            ppbr_rows = table.Select ($"DATE_PBR='{dateCurrent.ToString ()}'");
                        } else
                            ;

                        ppbr_rows [0] ["PBR_NUMBER"] = pbr_number;
                        indx_plan_factor = (INDEX_PLAN_FACTOR)_mcListPFI [pvi.ObjFactor].Id;

                        switch (indx_plan_factor) {
                            case INDEX_PLAN_FACTOR.PBR:
                            case INDEX_PLAN_FACTOR.Pmin:
                            case INDEX_PLAN_FACTOR.Pmax:
                                pbr_indx = ((INDEX_PLAN_FACTOR)_mcListPFI [pvi.ObjFactor].Id).ToString();
                                break;
                            default:
                                pbr_indx = string.Empty;
                                break;
                        }

                        if (string.IsNullOrWhiteSpace (pbr_indx) == false)
                            try {
                                ppbr_rows [0] [pbr_indx] = (double)ppbr_rows [0] [pbr_indx] + pvi.Value; //Вариант №3
                            } catch (Exception e) {
                                Logging.Logg ().Exception (e, $"DbMCInterface::GetData() - тип{ppbr_rows [0] [pbr_indx].GetType ().Name} значения по индексу={indx_plan_factor.ToString()}..."
                                    , Logging.INDEX_MESSAGE.NOT_SET);
                            }
                        else
                            Logging.Logg ().Error (string.Format ("DbMCInterface::GetData() - не найден индекс={0} значения ПБР...", _mcListPFI [pvi.ObjFactor].Id)
                                , Logging.INDEX_MESSAGE.NOT_SET);
                    } // foreach
                } else
                    ; //bRes = false, igo == null
            } // for, i

            return bRes;
        }

        public void Dispose ()
        {
        }
    }
}
