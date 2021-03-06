using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
using System.Data.Common;
using ASUTP.Database;
using ASUTP.Helper;


namespace StatisticCommon
{
    public struct HStatisticUser
    {
        public HStatisticUsers.ID_ROLES Role;

        public int Id;
    }

    public class HStatisticUsers : ASUTP.Helper.HUsers
    {
        /// <summary>
        /// ???????????? - ?????????????? ?????(?????) ?????????????
        /// </summary>
        public enum ID_ROLES { UNKNOWN, KOM_DISP = 1, ADMIN, USER, NSS = 101, MAJOR_MASHINIST, MASHINIST, LK_DISP = 201, SOURCE_DATA = 501
            , COUNT_ID_ROLES = 7
        };

        /// <summary>
        /// ???????????? - ?????????????? ????????????? ?????????? ??????? ????????????
        /// </summary>
        public enum ID_ALLOWED {
            UNKNOWN = -1
            , SOURCEDATA_CHANGED = 1 //???\???? ?????? ??????? ????
            , TAB_PBR_KOMDISP
            , AUTO_TAB_PBR_KOMDISP
            , ALARM_KOMDISP
            , AUTO_ALARM_KOMDISP
            , TAB_PBR_NSS
            , MENUITEM_SETTING_ADMIN_DB_CONGIG
            , MENUITEM_SETTING_ADMIN_DB_SOURCEDATA
            , MENUITEM_SETTING_ADMIN_STATEUSERS
            , MENUITEM_SETTING_ADMIN_PSW_KOMDISP_CHANGE
            //----------10
            , MENUITEM_SETTING_ADMIN_PSW_ADMIN_CHANGE
            , MENUITEM_SETTING_ADMIN_PSW_NSS_CHANGE
            , MENUITEM_SETTING_ADMIN_TECCOMPONENT_CHANGE
            , MENUITEM_SETTING_ADMIN_USERS_CHANGE
            , MENUITEM_SETTING_ADMIN_ROLES_ALLOWED_CHANGE
            , MENUCONTEXTITEM_PANELQUICKDATA_FORECASTEE
            , MENUCONTEXTITEM_PANELQUICKDATA_TMVALUES
            , MENUCONTEXTITEM_TABLEHOURS_COLUMN_59MIN
            , MENUITEM_SETTINGS_PARAMETERS_APP
            , MENUITEM_SETTINGS_PARAMETERS_TGBIYSK
            //--------------------20
            , APP_AUTO_RESET
            , SOURCEDATA_ASKUE_PLUS_SOTIASSO //???\???? ????? ????
            , MENUITEM_SETTING_PARAMETERS_SYNC_DATETIME_DB
            , PROFILE_SETTINGS_CHANGEMODE
            , PROFILE_VIEW_ADDINGTABS
            , SOURCEDATA_SOTIASSO_3_MIN //???\???? ????? ????            
            , AUTO_LOADSAVE_USERPROFILE //?????????????? ????????/?????????? ???????
            //, AUTO_LOAD_ADMINVALUESDEFAULT //?????????????? ???????? "????????????" ?? ?????????
            , MENUITEM_VIEW_VALUES_SOTIASSO
            , MENUITEM_SETTING_PARAMETERS_DIAGNOSTIC
            , AUTO_TAB_ALARM
            //------------------------------30
            , AUTO_TAB_PBR_NSS
            , TAB_LK_ADMIN // ?????? ? ??????????? ??????? ???????????????? ??????????????? ??
            , AUTO_TAB_LK_ADMIN
            , MENUITEM_VIEW_VZLET_TDIRECT
            , TAB_TEPLOSET_ADMIN // ?????? ? ??????????? ??????? ???????????????? ??????????????? ??
            , AUTO_TAB_TEPLOSET_ADMIN
            // KhryapinAN, 2017-06
            , MENUITEM_VIEW_VALUES_AIISKUE_SOTIASSO_DAY
            // KhryapinAN, 2017-09
            , EXPORT_PBRVALUES_KOMDISP
            , AUTO_EXPORT_PBRVALUES_KOMDISP
            //---------------------------------------39
            , PROFILE_VIEW_COLOR_CHANGESHEMA_BACKGROUND = 40 // ???? ?????????? ????? - ???
            , PROFILE_VIEW_COLOR_SHEMA // ?????????? ?? ????????? ???????? ????? ? ?????, ?? ???? ??????? ????????????? ????????? ??? ???????????????? ?????
            , PROFILE_VIEW_COLOR_CHANGESHEMA_FONT // ???? ?????????? ????? - ?????
            , PBR_HOUR_TO_HOUR //???\???? ??????? ??????????
            , PBR_HOUR_TO_HOUR_DEFAULT_VALUE //??????????/???????? 
        };

        public HStatisticUsers(int iListenerId, MODE_REGISTRATION modeRegistration)
            : base(iListenerId, modeRegistration)
        {
            Initialize(@"????: " + Role);
        }

        public HStatisticUsers(int iListenerId, string modeRegistration)
            : this (iListenerId, Enum.IsDefined(typeof(MODE_REGISTRATION), modeRegistration) == true ? (MODE_REGISTRATION)Enum.Parse(typeof(MODE_REGISTRATION), modeRegistration) : MODE_REGISTRATION.MIXED)
        {
        }

        public HStatisticUsers(int iListenerId)
            : this(iListenerId, MODE_REGISTRATION.USER_DOMAINNAME)
        {
        }

        public static ID_ROLES Role
        {
            get
            {
                return (s_DataRegistration == null) ? ID_ROLES.UNKNOWN : ((!((int)INDEX_REGISTRATION.ID_TEC < s_DataRegistration.Length)) || (s_DataRegistration[(int)INDEX_REGISTRATION.ROLE] == null)) ? ID_ROLES.ADMIN : (ID_ROLES)s_DataRegistration[(int)INDEX_REGISTRATION.ROLE];
            }
        }

        public static bool RoleIsKomDisp
        {
            get
            {
                return Role == ID_ROLES.KOM_DISP;
            }
        }

        public static bool RoleIsDisp
        {
            get
            {
                return ((Role == ID_ROLES.ADMIN) || (Role == ID_ROLES.KOM_DISP) || (Role == ID_ROLES.NSS) ||  (Role == ID_ROLES.LK_DISP));
            }
        }

        public static bool RoleIsAdmin
        {
            get
            {
                return Role == ID_ROLES.ADMIN;
            }
        }

        //public static bool RoleIsNSS
        //{
        //    get
        //    {
        //        return Role == ID_ROLES.NSS;
        //    }
        //}

        //public static bool RoleIsOperationPersonal
        //{
        //    get
        //    {
        //        return (Role == ID_ROLES.NSS) || (Role == ID_ROLES.MAJOR_MASHINIST) || (Role == ID_ROLES.MASHINIST);
        //    }
        //}

        public static void SetAllowed (ConnectionSettings connSett, ID_ALLOWED id, string value)
        {
            int iListenerId = -1;

            iListenerId = DbSources.Sources ().Register (connSett, false, string.Format(@"{0}==HStatisticUsers::SetAllowed(id={1}, value={2})", connSett.name, id.ToString(), value));
            HUsers.SetAllowed (iListenerId, (int)id, value);
            DbSources.Sources ().UnRegister (iListenerId);
        }
    }
}
