using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;
using StatisticTrans;
using StatisticTransModes;
using System.ServiceModel;
using StatisticTrans.Contract;

namespace trans_mt
{
    public partial class FormMainTransMT : FormMainTransModes
    {
        public FormMainTransMT()
            : base(ASUTP.Helper.ProgramBase.ID_APP.TRANS_MODES_TERMINALE
                , new KeyValuePair<string, string> [] {
                    new KeyValuePair<string, string> (@"ИгнорДатаВремя-ModesTerminale", false.ToString())
                    , new System.Collections.Generic.KeyValuePair<string, string> (@"FetchWaking", $"HH:mm:ss;{StatisticTrans.Contract.Default.FetchWaking}")
                    , new KeyValuePair<string, string> ("NameEndPointService", "EndPointServiceTransModesTerminale")
                })
        {
            this.notifyIconMain.Icon =
            this.Icon = trans_mt.Properties.Resources.statistic6;

            InitializeComponentTransDB ();

            m_dgwAdminTable.Size = new System.Drawing.Size(498, 471);
        }

        protected override IServiceTransModes createChannelService (InstanceContext context)
        {
            return new DuplexChannelFactory<StatisticTrans.Contract.ModesTerminale.IServiceModesTerminale> (context, FileAppSettings.This ().GetValue (@"NameEndPointService"))
                .CreateChannel ();
        }

        protected override void Start()
        {
            bool bInitialize = false;

            bInitialize = EditFormConnectionSettings ("connsett_mt.ini", false);

            if (bInitialize == true) {
                bInitialize = ((StatisticTrans.Contract.ModesTerminale.IServiceModesTerminale)_client).Initialize (s_listFormConnectionSettings [(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].getConnSett ()
                    , s_iMainSourceData
                    , handlerCmd.ModeMashine
                    , FileAppSettings.This ().FetchWaking (StatisticTrans.Contract.Default.FetchWaking)
                    , m_modeTECComponent,
                    m_listID_TECNotUse);

                if (bInitialize == true) {
                    _client.SetIgnoreDate (StatisticTrans.CONN_SETT_TYPE.SOURCE, bool.Parse (FileAppSettings.This ().GetValue (@"ИгнорДатаВремя-ModesTerminale")));
                    _client.SetIgnoreDate (StatisticTrans.CONN_SETT_TYPE.DEST, bool.Parse (FileAppSettings.This ().GetValue (@"ИгнорДатаВремя-techsite")));

                    ////??? никогда не выполняется, т.к. не заполнен ComboBox с перечнем ГТП
                    //if (IsCanSelectedIndexChanged == true)
                    //    setUIControlConnectionSettings (StatisticTrans.CONN_SETT_TYPE.DEST, _client.GetConnectionSettingsByKeyDeviceAndType (StatisticTrans.CONN_SETT_TYPE.DEST, SelectedItemKey, StatisticCommon.CONN_SETT_TYPE.PBR));
                    //else
                    //    ;

                    _client.Start ();

                    ////??? только после события 'Started'
                    //base.Start ();
                } else
                    ;
            } else
                ;
        }

        protected override void buttonSaveSourceSett_Click(object sender, EventArgs e)
        {
        }

        protected override void setUIControlSourceState(bool bEnabled)
        {
        }

        protected override void comboBoxTECComponent_SelectedIndexChanged(object sender, EventArgs ev)
        {
            ASUTP.Database.ConnectionSettings connSettSource
                , connSettDest;

            if (IsCanSelectedIndexChanged == true)
            {
                connSettDest = _client.GetConnectionSettingsByKeyDeviceAndType (StatisticTrans.CONN_SETT_TYPE.DEST, SelectedItemKey, StatisticCommon.CONN_SETT_TYPE.PBR);
                connSettSource = _client.GetConnectionSettingsByKeyDeviceAndType (StatisticTrans.CONN_SETT_TYPE.SOURCE, SelectedItemKey, StatisticCommon.CONN_SETT_TYPE.PBR);

                base.comboBoxTECComponent_SelectedIndexChanged(sender, ev);

                setUIControlConnectionSettings (StatisticTrans.CONN_SETT_TYPE.DEST, connSettDest);
                setUIControlConnectionSettings (StatisticTrans.CONN_SETT_TYPE.SOURCE, connSettSource);
            }
            else
                ;
        }

        protected override void timer_Start()
        {
            base.timer_Start();
        }
    }
}
