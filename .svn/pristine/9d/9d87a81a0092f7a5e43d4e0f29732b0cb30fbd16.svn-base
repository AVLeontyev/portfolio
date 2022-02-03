using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using StatisticCommon;
using ASUTP.Database;
using ASUTP;

namespace StatisticTransModes
{
    public class AdminTS_Modes : AdminTS
    {
        public AdminTS_Modes(bool[] arMarkPPBRValues)
            : base(arMarkPPBRValues, TECComponentBase.TYPE.ELECTRO)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        public override ASUTP.Helper.Errors SaveChanges ()
        {
            //Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - вХод...", Logging.INDEX_MESSAGE.NOT_SET);

            bool bResSemaDbAccess = false;
            
            try { delegateStartWait(); }
            catch (Exception e) {
                Logging.Logg().Exception(e, @"AdminTS_Modes::SaveChanges () - delegateStartWait() - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            int msecWaitSemaDbAccess = ASUTP.Core.Constants.MAX_RETRY * ASUTP.Core.Constants.MAX_WAIT_COUNT * ASUTP.Core.Constants.WAIT_TIME_MS;
            //Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - delegateStartWait() - Интервал ожидания для semaDBAccess=" + msecWaitSemaDbAccess, Logging.INDEX_MESSAGE.NOT_SET);

            bResSemaDbAccess = semaDBAccess.WaitOne(msecWaitSemaDbAccess);
            if (bResSemaDbAccess == true)
            {
                lock (m_lockState)
                {
                    ClearStates();

                    saveResult = ASUTP.Helper.Errors.NoAccess;
                    saving = true;
                    using_date = false;
                    m_curDate = m_prevDate;

                    //Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - states.Clear()", Logging.INDEX_MESSAGE.NOT_SET);

                    AddState((int)StatesMachine.CurrentTime);
                    //AddState((int)StatesMachine.AdminDates);
                    //??? Состояния позволяют НАЧать процесс разработки возможности редактирования ПЛАНа на вкладке 'Редактирование ПБР'
                    AddState((int)StatesMachine.PPBRDates);
                    //AddState((int)StatesMachine.SaveAdminValues);
                    AddState((int)StatesMachine.SavePPBRValues);
                    //AddState((int)StatesMachine.UpdateValuesPPBR);

                    Run(@"AdminTS_Modes::SaveChanges ()");
                }

                bResSemaDbAccess = semaDBAccess.WaitOne(msecWaitSemaDbAccess);
                //Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - semaDBAccess.WaitOne(" + DbInterface.MAX_WATING + @")=" + bResSemaDbAccess.ToString(), Logging.INDEX_MESSAGE.NOT_SET);

                try
                {
                    semaDBAccess.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"AdminTS_Modes::SaveChanges () - semaDBAccess.Release(1)", Logging.INDEX_MESSAGE.NOT_SET);
                }

                saving = false;

                //??? почему SavePPBRValues, а не SaveAdminValues
                saveComplete?.Invoke ((int)StatesMachine.SavePPBRValues);
            }
            else {
                Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - semaDBAccess.WaitOne(" + msecWaitSemaDbAccess + @")=false", Logging.INDEX_MESSAGE.NOT_SET);

                saveResult = ASUTP.Helper.Errors.NoAccess;
                saving = true;
            }

            try { delegateStopWait(); }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"AdminTS_Modes::SaveChanges () - delegateStopWait() - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            return saveResult;
        }
    }
}
