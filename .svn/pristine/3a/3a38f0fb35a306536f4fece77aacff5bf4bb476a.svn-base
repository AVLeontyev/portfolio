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

namespace STrans.Service.ModesCentre
{
    [ServiceBehavior (ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerSession, UseSynchronizationContext =false)]
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "ServiceServiceModesCentre" в коде и файле конфигурации.
    public class ServiceModesCentre : ServiceTransModes, StatisticTrans.Contract.ModesCentre.IServiceModesCentre
    {
        /// <summary>
        /// Создать объект опроса значений источника
        /// </summary>
        /// <param name="connSett">Параметры соединения с БД источника данных</param>
        /// <param name="iMainSourceData">Идентификатор главного/базового источника данных yfpyfxtybz (сервера в интересах которого выполняется экземпляр службы - клиент)</param>
        /// <param name="mcServiceHost">Доменное наименование</param>
        /// <param name="modeMashine">Режим работы клиента</param>
        /// <param name="tsFetchWaking"></param>
        /// <param name="jEventListener">Список событий на котрые требуется подписка (только для режима 'ON_EVENT')</param>
        /// <param name="modeTECComponent">Режим списка компонентов ТЭЦ</param>
        /// <param name="listID_TECNotUse">Список идентификаторов ТЭЦ, не использующихся при операциях сохранения</param>
        /// <returns>Результат(успех) создания объекта</returns>
        public bool Initialize (ASUTP.Database.ConnectionSettings connSett
            , int iMainSourceData
            , string mcServiceHost
            , MODE_MASHINE modeMashine
            , TimeSpan tsFetchWaking
            , string jEventListener
            , FormChangeMode.MODE_TECCOMPONENT modeTECComponent
            , List<int> listID_TECNotUse)
        {
            bool bRes = false;

            StatisticTrans.CONN_SETT_TYPE indx = StatisticTrans.CONN_SETT_TYPE.SOURCE;
            int i = -1;
            bool bIgnoreTECInUse = false;

            bRes = preInitialize (connSett, iMainSourceData);

            ASUTP.Core.HMark markQueries = new ASUTP.Core.HMark (new int [] { (int)StatisticCommon.CONN_SETT_TYPE.ADMIN, (int)StatisticCommon.CONN_SETT_TYPE.PBR });

            try {
                m_arAdmin [(Int16)indx] = new AdminMC (mcServiceHost, tsFetchWaking, jEventListener);
                if (modeMashine == MODE_MASHINE.SERVICE_ON_EVENT) {
                    (m_arAdmin [(Int16)indx] as AdminMC).AddEventHandler (ID_EVENT.HANDLER_CONNECT, serviceModesCentre_EventHandlerConnect);

                    (m_arAdmin [(Int16)indx] as AdminMC).AddEventHandler (ID_EVENT.RELOAD_PLAN_VALUES, serviceModesCentre_EventMaketChanged);
                    //!!! дубликат для отладки
                    (m_arAdmin [(Int16)indx] as AdminMC).AddEventHandler (ID_EVENT.PHANTOM_RELOAD_PLAN_VALUES, serviceModesCentre_EventMaketChanged);
                    (m_arAdmin [(Int16)indx] as AdminMC).AddEventHandler (ID_EVENT.NEW_PLAN_VALUES, serviceModesCentre_EventPlanDataChanged);
                    //!!! дубликат для выполнения внеочередного запроса (например, при запуске)
                    (m_arAdmin [(Int16)indx] as AdminMC).AddEventHandler (ID_EVENT.REQUEST_PLAN_VALUES, serviceModesCentre_EventPlanDataChanged);
                } else
                    ;
                m_arAdmin [(Int16)indx].InitTEC (modeTECComponent, markQueries, bIgnoreTECInUse, new int [] { 0, (int)TECComponent.ID.LK });
                removeTEC (m_arAdmin [(Int16)indx], listID_TECNotUse);
            } catch (Exception e) {
                Logging.Logg ().Exception (e, "FormMainTransMC::FormMainTransMC ()", Logging.INDEX_MESSAGE.NOT_SET);

                ////ErrorReport("Ошибка соединения. Переход в ожидание.");
                ////setUIControlConnectionSettings(i);
            }

            return postInitialize(modeTECComponent, listID_TECNotUse);
        }

        public void GetMaketEquipment (FormChangeMode.KeyDevice key, EventArgs<Guid> arg, DateTime date)
        {
            ((AdminMC)m_arAdmin [(int)StatisticTrans.CONN_SETT_TYPE.SOURCE]).GetMaketEquipment (key, arg, date);
        }

        public void DebugEventReloadPlanValues ()
        {
            (m_arAdmin [(int)StatisticTrans.CONN_SETT_TYPE.SOURCE] as AdminMC).DebugEventReloadPlanValues ();
        }

        public void ToDateRequest (DateTime date)
        {
            (m_arAdmin [(int)StatisticTrans.CONN_SETT_TYPE.SOURCE] as AdminMC).ToDateRequest (date);
        }

        public void FetchEvent (bool bRemove)
        {
            ((AdminMC)m_arAdmin [(int)StatisticTrans.CONN_SETT_TYPE.SOURCE]).FetchEvent (bRemove);
        }

        public bool IsServiceOnEvent
        {
            get
            {
                return ((AdminMC)m_arAdmin [(int)StatisticTrans.CONN_SETT_TYPE.SOURCE]).IsServiceOnEvent;
            }
        }

        private void serviceModesCentre_EventHandlerConnect (object obj, EventArgs ev)
        {
        }

        private void serviceModesCentre_EventMaketChanged (object obj, EventArgs ev)
        {
        }

        private void serviceModesCentre_EventPlanDataChanged (object obj, EventArgs ev)
        {
            IdPseudoDelegate id = IdPseudoDelegate.ModesCentre_PlanDataChanged;

            callbackRaise (new ServiceCallbackResultEventArgs (id));
        }
    }
}
