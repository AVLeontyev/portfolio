using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using STrans.Service.ModesCentre;
using StatisticCommon;
using StatisticTrans;
using ASUTP;
using StatisticTransModes;
using ASUTP.Database;
using StatisticTrans.Contract.ModesCentre;
using StatisticTrans.Contract;
using ASUTP.Core;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using System.ServiceModel.Dispatcher;

namespace STrans.Service
{
    public abstract class ServiceTransModes : StatisticTrans.Contract.IServiceTransModes
    {
        protected HAdmin [] m_arAdmin;

        //private struct PropertyPerSession
        //{
            /// <summary>
            /// !!! Объект для связи с клиентом; только для 'InstanceContextMode.PerSession'
            /// </summary>
            private IServiceCallback _callback;

        System.ServiceModel.Channels.IChannel _channel;

            IContextChannel _contextChannel;

            private InstanceContext _instanceContext;

            private EndpointDispatcher _endPointDispatcher;
        //}

        private IServiceCallback Callback
        {
            get
            {
                return
                    //OperationContext.Current?.GetCallbackChannel<IServiceCallback> ()
                    _callback
                    ;
            }
        }

        protected StatisticTrans.CONN_SETT_TYPE CurrentIndex { get; set; }

        public ServiceTransModes ()
            : base()
        {
            Logging.SetMode (Logging.LOG_MODE.FILE_DESKTOP);

            create (true);

            _instanceContext = OperationContext.Current?.InstanceContext;
            _channel = _instanceContext.IncomingChannels.FirstOrDefault ();
            _contextChannel = OperationContext.Current?.Channel;
            _endPointDispatcher = OperationContext.Current?.EndpointDispatcher;

            _callback = OperationContext.Current?.GetCallbackChannel<IServiceCallback> ();
        }

        protected void create (bool bForced)
        {
            bool bCreate = bForced;

            if (bCreate == false)
                bCreate = Equals (m_arAdmin, null);
            else
                ;

            if (bCreate == true)
                m_arAdmin = new HAdmin [(int)StatisticTrans.CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];
            else
                ;
        }

        public void Connect ()
        {
            ;
        }

        public int CountHoursOfDate (DateTime datetime)
        {
            return StatisticCommon.HAdmin.CountHoursOfDate (datetime);
        }

        //public HAdmin GetAdminModesCentre ()
        //{
        //    return _adminMC;
        //}

        public void GetRDGValues (StatisticTrans.CONN_SETT_TYPE indx, FormChangeMode.KeyDevice key, DateTime date)
        {
            try {
                m_arAdmin [(int)(CurrentIndex = indx)].GetRDGValues (key, date);
            } catch (Exception e) {
                //Logging.Logg ().Exception (e, "", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        public StatisticCommon.HAdmin.RDGStruct [] GetRDGValues (StatisticTrans.CONN_SETT_TYPE indx)
        {
            return m_arAdmin [(int)indx].m_curRDGValues;
        }

        public void ClearRDGValues (StatisticTrans.CONN_SETT_TYPE indx, DateTime date)
        {
            ((AdminTS)m_arAdmin [(int)indx]).ClearRDGValues (date);
        }

        protected bool preInitialize (ConnectionSettings connSett, int iMainSourceData)
        {
            //??? как проверить
            bool bRes = true;

            ASUTP.Forms.FormMainBase.s_iMainSourceData = iMainSourceData;

            create (false);

            //??? для создания статического 'DbMCSources' = 'DbSources'
            DbMCSources.Sources ();
            new DbTSQLConfigDatabase (connSett);
            DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings (connSett);
            DbTSQLConfigDatabase.DbConfig ().Register ();

            return bRes;
        }

        /// <summary>
        /// Создать объект для записи в БД ИС Статистика
        /// </summary>
        /// <param name="modeTECComponent">Режим списка компонентов ТЭЦ</param>
        /// <param name="listID_TECNotUse">Список идентификаторов ТЭЦ, не использующихся при операциях сохранения</param>
        /// <returns></returns>
        protected bool postInitialize (FormChangeMode.MODE_TECCOMPONENT modeTECComponent
            , List<int> listID_TECNotUse)
        {
            bool bIgnoreTECInUse = false;
            ASUTP.Core.HMark markQueries = new ASUTP.Core.HMark (new int [] { (int)StatisticCommon.CONN_SETT_TYPE.ADMIN, (int)StatisticCommon.CONN_SETT_TYPE.PBR });

            try {
                m_arAdmin [(int)StatisticTrans.CONN_SETT_TYPE.DEST] = new AdminTS_Modes (new bool [] { false, true });
                m_arAdmin [(int)StatisticTrans.CONN_SETT_TYPE.DEST].InitTEC (modeTECComponent, markQueries, bIgnoreTECInUse, new int [] { 0, (int)TECComponent.ID.LK });
                removeTEC (m_arAdmin [(int)StatisticTrans.CONN_SETT_TYPE.DEST], listID_TECNotUse);
            } catch (Exception e) {
                //Logging.Logg ().Exception (e, "FormMainTransMC::FormMainTransMC ()", Logging.INDEX_MESSAGE.NOT_SET);

                ////ErrorReport("Ошибка соединения. Переход в ожидание.");
                ////setUIControlConnectionSettings(i);
            }

            //Callback?.Raise (-1);

            setDelegateWait ();
            setDelegateReport ();

            setDelegateForceDate ();

            setDelegateData ();
            setDelegateSaveComplete ();

            DbTSQLConfigDatabase.DbConfig ().UnRegister ();

            return IsValidate();
        }

        public void SetIgnoreDate (StatisticTrans.CONN_SETT_TYPE indx, bool bIgnoreDate)
        {
            m_arAdmin [(int)indx].m_ignore_date = bIgnoreDate;
        }

        public void Start ()
        {
            for (StatisticTrans.CONN_SETT_TYPE i = (StatisticTrans.CONN_SETT_TYPE)0; i < StatisticTrans.CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                m_arAdmin [(int)i].Start ();

            Started ();
        }

        public virtual void Stop ()
        {
            //Action delegateStop = async delegate () {
            //    await Task.Factory.StartNew (() => {
                    for (StatisticTrans.CONN_SETT_TYPE i = (StatisticTrans.CONN_SETT_TYPE)0; (i < StatisticTrans.CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE) && (Equals (m_arAdmin == null) == false); i++) {
                        if (!(m_arAdmin [(int)i] == null)) {
                            m_arAdmin [(int)i].Stop ();
                            m_arAdmin [(int)i] = null;
                        } else
                            ;
                    }
            //    });

                Stopped ();
            //};

            //delegateStop ();

            ////Task.Factory.StartNew(() => );
        }

        public virtual void Close ()
        {
            close ();

            //Closed ();
        }

        private void close ()
        {
            try {
                //_endPointDispatcher?.ChannelDispatcher.Close ();
                //_contextChannel?.Close ();
                //_instanceContext?.Close ();

                if (_channel.State == CommunicationState.Opened)
                    _channel.Close ();
                else
                    Logging.Logg ().Warning ($"STrans.Service.ServicetransModes::close (State={_channel.State}) - closing channel/context/instance...", Logging.INDEX_MESSAGE.NOT_SET);
            } catch (Exception e) {
                Logging.Logg ().Exception (e, $"STrans.Service.ServicetransModes::close () - closing channel/context/instance...", Logging.INDEX_MESSAGE.NOT_SET);
            } finally {
                _callback = null;
            }
        }

        public void Activate (StatisticTrans.CONN_SETT_TYPE indx, bool actived)
        {
            m_arAdmin [(int)indx].Activate (actived);
        }

        protected void setDelegateWait ()
        {
            for (StatisticTrans.CONN_SETT_TYPE i = (StatisticTrans.CONN_SETT_TYPE)0; i < StatisticTrans.CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                m_arAdmin [(int)i].SetDelegateWait (delegateStartWait, delegateStopWait, delegateStatus);
        }

        protected void setDelegateReport ()
        {
            for (StatisticTrans.CONN_SETT_TYPE i = (StatisticTrans.CONN_SETT_TYPE)0; i < StatisticTrans.CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                m_arAdmin [(int)i].SetDelegateReport (delegateErrorReport, delegateWarningReport, delegateActionReport, delegateClearReport);
        }

        protected void setDelegateForceDate ()
        {
            for (StatisticTrans.CONN_SETT_TYPE i = (StatisticTrans.CONN_SETT_TYPE)0; i < StatisticTrans.CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                m_arAdmin [(int)i].SetDelegateDatetime (delegateForceDatetime);
        }

        protected void setDelegateData ()
        {
            for (StatisticTrans.CONN_SETT_TYPE i = (StatisticTrans.CONN_SETT_TYPE)0; i < StatisticTrans.CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                m_arAdmin [(int)i].SetDelegateData (delegateSuccessResult, delegateErrorResult);
        }

        protected void setDelegateSaveComplete ()
        {
            for (StatisticTrans.CONN_SETT_TYPE i = (StatisticTrans.CONN_SETT_TYPE)0; i < StatisticTrans.CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                m_arAdmin [(int)i].SetDelegateSaveComplete (new DelegateIntFunc (delegateSaveCompleted));
        }

        public void SaveRDGValues (StatisticTrans.CONN_SETT_TYPE indx, StatisticTrans.Contract.PARAMToSaveRDGValues param)
        {
            ((AdminTS)m_arAdmin [(int)(CurrentIndex = indx)]).SaveRDGValues (((PARAMToSaveRDGValues)param).key, ((PARAMToSaveRDGValues)param).date, ((PARAMToSaveRDGValues)param).bCallback);
        }

        public FormChangeMode.KeyDevice PrepareActionRDGValues (StatisticTrans.CONN_SETT_TYPE indx)
        {
            return m_arAdmin [(int)indx].PrepareActionRDGValues ();
        }

        public void CopyRDGValues (StatisticTrans.CONN_SETT_TYPE indxSource, StatisticTrans.CONN_SETT_TYPE indxDest)
        {
            m_arAdmin [(int)indxDest].getCurRDGValues (m_arAdmin [(int)indxSource]);
        }

        //public void SetCurrentRDGValues (StatisticTrans.CONN_SETT_TYPE indxDest, HAdmin.RDGStruct [] valuesSource)
        //{
        //    ((AdminModes)m_arAdmin [(int)indxDest]).setCurrentRDGValues (valuesSource);
        //}

        public void SetCurrentRDGValue (StatisticTrans.CONN_SETT_TYPE indxDest, int iHour, HAdmin.RDGStruct valueSource)
        {
            ((HAdmin)m_arAdmin [(int)indxDest]).SetCurrentRDGValue (iHour, valueSource);
        }

        public void CopyCurToPrevRDGValues (StatisticTrans.CONN_SETT_TYPE indx)
        {
            m_arAdmin [(int)indx].CopyCurToPrevRDGValues ();
        }

        //public string GetPBRNumber (StatisticTrans.CONN_SETT_TYPE indx, int iHour)
        //{
        //    return _arAdmin [(int)(CurrentIndex = indx)].m_curRDGValues [iHour].pbr_number;
        //}

        public void TECComponentComplete (StatisticTrans.CONN_SETT_TYPE indx, int iState, bool bResult)
        {
            m_arAdmin [(int)(CurrentIndex = indx)].TECComponentComplete (iState, bResult);
        }

        public void SetCurrentDate (StatisticTrans.CONN_SETT_TYPE indx, DateTime date)
        {
            m_arAdmin [(int)indx].m_curDate = date;
        }

        public FormChangeMode.KeyDevice GetFirstTECComponentKey (StatisticTrans.CONN_SETT_TYPE indx)
        {
            return m_arAdmin [(int)indx].FirstTECComponentKey;
        }

        public FormChangeMode.KeyDevice GetCurrentKey (StatisticTrans.CONN_SETT_TYPE indx)
        {
            return m_arAdmin [(int)indx].CurrentKey;
        }

        public IDevice GetCurrentDevice (StatisticTrans.CONN_SETT_TYPE indx)
        {
            return m_arAdmin [(int)indx].CurrentDevice;
        }

        public bool GetAllowRequested (StatisticTrans.CONN_SETT_TYPE indx)
        {
            return GetCurrentDevice(indx).ListMCentreId.Count > 0;
        }

        protected bool IsValidate ()
        {
            return (!(m_arAdmin == null))
                && (m_arAdmin.ToList().TrueForAll(admin => Equals(admin) == false));
        }

        public bool IsValidate (StatisticTrans.CONN_SETT_TYPE indx)
        {
            return (!(m_arAdmin == null))
                && (!(m_arAdmin [(int)indx] == null));
        }

        public string GetFormatDatetime (StatisticTrans.CONN_SETT_TYPE indx, int iHour)
        {
            return m_arAdmin [(int)indx].GetFmtDatetime (iHour);
        }

        public int GetSeasonHourOffset (StatisticTrans.CONN_SETT_TYPE indx, int iHour)
        {
            return m_arAdmin [(int)indx].GetSeasonHourOffset (iHour);
        }

        public int AdminCount
        {
            get
            {
                return m_arAdmin.Length;
            }
        }

        public List<FormChangeMode.KeyDevice> GetListKeyTECComponent (StatisticTrans.CONN_SETT_TYPE indx, FormChangeMode.MODE_TECCOMPONENT mode, bool bLimitLK)
        {
            return m_arAdmin [(int)indx].GetListKeyTECComponent (mode, bLimitLK);
        }

        public string GetNameTECComponent (StatisticTrans.CONN_SETT_TYPE indx, FormChangeMode.KeyDevice key, bool bWithNameTECOwner)
        {
            return ((AdminTS)m_arAdmin [(int)indx]).GetNameTECComponent (key, bWithNameTECOwner);
        }

        public ConnectionSettings GetConnectionSettingsByKeyDeviceAndType (StatisticTrans.CONN_SETT_TYPE indx, FormChangeMode.KeyDevice key, StatisticCommon.CONN_SETT_TYPE type)
        {
            return m_arAdmin [(int)indx].FindTECComponent (key)?.tec.connSetts [(int)type];
        }

        protected void removeTEC (HAdmin admin, List<int> listID_TECNotUse)
        {
            foreach (int id in listID_TECNotUse) {
                admin.RemoveTEC (id);
            }
        }

        protected void callbackRaise (ServiceCallbackResultEventArgs ev)
        {
            try {
                Callback?.Raise (ev);
            } catch (Exception e) {
                Logging.Logg ().Exception (e, $"ServiceTransModes::delegate^ (id={ev.Id.ToString ()}) - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        private void delegateStartWait ()
        {
            IdPseudoDelegate id = IdPseudoDelegate.WaitStart;

            callbackRaise (new ServiceCallbackResultEventArgs (id));
        }

        private void delegateStopWait ()
        {
            IdPseudoDelegate id = IdPseudoDelegate.WaitStop;

            callbackRaise (new ServiceCallbackResultEventArgs (id));
        }

        private void delegateStatus ()
        {
            IdPseudoDelegate id = IdPseudoDelegate.WaitStatus;

            callbackRaise (new ServiceCallbackResultEventArgs (id));
        }

        private void delegateErrorReport (string message)
        {
            IdPseudoDelegate id = IdPseudoDelegate.ReportError;

            callbackRaise (new ServiceCallbackResultEventArgs (id, message));
        }

        private void delegateWarningReport (string message)
        {
            IdPseudoDelegate id = IdPseudoDelegate.ReportWarning;

            callbackRaise (new ServiceCallbackResultEventArgs (id, message));
        }

        private void delegateActionReport (string message)
        {
            IdPseudoDelegate id = IdPseudoDelegate.ReportAction;

            callbackRaise (new ServiceCallbackResultEventArgs (id, message));
        }

        private void delegateClearReport (bool bClear)
        {
            IdPseudoDelegate id = IdPseudoDelegate.ReportClear;

            callbackRaise (new ServiceCallbackResultEventArgs (id, bClear));
        }

        private void delegateForceDatetime (DateTime datetime)
        {
            IdPseudoDelegate id = IdPseudoDelegate.SetForceDate;

            callbackRaise (new ServiceCallbackResultEventArgs (id, datetime));
        }

        private void delegateSuccessResult (DateTime datetime, bool bSuccess)
        {
            IdPseudoDelegate id = IdPseudoDelegate.Ready;

            callbackRaise (new ServiceCallbackResultEventArgs (id
                , datetime
                , bSuccess
                , bSuccess == true ? m_arAdmin [(int)CurrentIndex].m_curRDGValues.ToList () : null
            ));
        }

        private void delegateErrorResult (int iState)
        {
            IdPseudoDelegate id = IdPseudoDelegate.Error;

            callbackRaise (new ServiceCallbackResultEventArgs (id, iState));
        }

        private void delegateSaveCompleted (int iState)
        {
            IdPseudoDelegate id = IdPseudoDelegate.SaveCompleted;

            callbackRaise (new ServiceCallbackResultEventArgs (id, iState));
        }

        public void Started ()
        {
            IdPseudoDelegate id = IdPseudoDelegate.Started;

            callbackRaise (new ServiceCallbackResultEventArgs (id));
        }

        public void Stopped ()
        {
            IdPseudoDelegate id = IdPseudoDelegate.Stopped;

            callbackRaise (new ServiceCallbackResultEventArgs (id));
        }

        //public void Closed ()
        //{
        //    IdPseudoDelegate id = IdPseudoDelegate.Closed;

        //    callbackRaise (new ServiceCallbackResultEventArgs (id));
        //}
    }
}
