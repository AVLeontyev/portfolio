using StatisticCommon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.ObjectModel;
using ASUTP.Helper;

namespace StatisticTrans
{
    /// <summary>
    /// Реализация интерфейса взаимодействия с файлом конфигурации 'app.config'
    /// </summary>
    public class FileAppSettings : IFileAppSettings
    {
        /// <summary>
        /// Разделитель нескольких значений для некоторых параметров
        /// </summary>
        public static string DELIM = ";";

        private const string s_OverDate = "03:04:05";

        private static FileAppSettings _this;

        private KeyValueConfigurationCollection _config;

        /// <summary>
        /// Возвратить строку-идентификатор приложения 'ASUTP.Helper.ProgramBase.ID_APP'
        /// </summary>
        public ProgramBase.ID_APP GetIdApplication
        {
            get
            {
                return (ProgramBase.ID_APP)Enum.Parse(typeof(ProgramBase.ID_APP), getValue("iapp"));
            }
        }

        /// <summary>
        /// Признак принудительной записи значений обязательных параметров
        /// </summary>
        public static bool RequiredForced = true;

        /// <summary>
        /// Конструктор по умолчанию: шаблон 'Singleton'
        /// </summary>
        private FileAppSettings ()
            : this (ConfigurationManager.OpenExeConfiguration (Assembly.GetEntryAssembly ().Location).AppSettings.Settings)
        {
        }

        public FileAppSettings (KeyValueConfigurationCollection config)
            //: base ()
        {
            _config = config;

            if (RequiredForced == true)
                new List<Tuple<string, string>> {
                    new Tuple<string, string> (FormParameters.GetNameParametersOfIndex ((int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGPBRNUMBER), false.ToString())
                    , new Tuple<string, string> (FormParameters.GetNameParametersOfIndex ((int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGQUERY), false.ToString())
                    , new Tuple<string, string> (FormParameters.GetNameParametersOfIndex ((int)FormParameters.PARAMETR_SETUP.MAIN_DATASOURCE), 671.ToString())
                    , new Tuple<string, string> (FormParameters.GetNameParametersOfIndex ((int)FormParameters.PARAMETR_SETUP.SEASON_DATETIME), @"21.10.2014 03:00")
                    , new Tuple<string, string> (FormParameters.GetNameParametersOfIndex ((int)FormParameters.PARAMETR_SETUP.SEASON_ACTION), "-1")
                    , new Tuple<string, string> ("iapp", "-1")
                    , new Tuple<string, string> ("debug", "False")
                    , new Tuple<string, string> (@"ИгнорДатаВремя-techsite", false.ToString())
                    , new Tuple<string, string> (@"ОкноНазначение", @"Конвертер (...)")
                    , new Tuple<string, string> (@"ID_TECNotUse", string.Empty)
                    , new Tuple<string, string> (@"ОпросСохранениеППБР", string.Join(",", new bool[] { true, false }))
                    , new Tuple<string, string> (@"ОпросСохранениеАдминЗнач", string.Join(",", new bool[] { true, false }))
                    , new Tuple<string, string> (@"ID_TECNotUse", string.Empty)
                    , new Tuple<string, string> ("OverDate", $"HH:mm:ss;{s_OverDate}")
                }.ToList ().ForEach (elem => addRequired (elem.Item1, elem.Item2));
            else
                ;
        }

        /// <summary>
        /// Возвратить объект класса для доступа к его методам/свойствам
        /// </summary>
        /// <returns>Объект класса</returns>
        public static FileAppSettings This ()
        {
            if (Equals (_this, null) == true)
                _this = new FileAppSettings ();
            else
                ;

            return _this;
        }

        private void addRequired (string key, string value)
        {
            if (IsContainsKey(key) == false)
                setValue(key, value);
            else {
            // это обязательные элементы, при наличии - перезаписывать не требуется
                //elem.Value = value;

                //_required.Move (_required.IndexOf(elem), 0);
            }
        }

        public enum TIMESPAN_PARSE_FUNC { NATIVE, DIFFERENCE }

        public TimeSpan OverDate(string valueDefault = s_OverDate)
        {
            return ParseTimeSpan (GetValue ("OverDate").Split (new string [] { DELIM }, StringSplitOptions.RemoveEmptyEntries), DateTime.Now, TIMESPAN_PARSE_FUNC.NATIVE, valueDefault);
        }

        public TimeSpan FetchWaking (string valueDefault)
        {
            return ParseTimeSpan (GetValue ("FetchWaking").Split (new string [] { DELIM }, StringSplitOptions.RemoveEmptyEntries), DateTime.Now, TIMESPAN_PARSE_FUNC.DIFFERENCE, valueDefault);
        }

        public static TimeSpan ParseTimeSpan (string [] values, DateTime now, TIMESPAN_PARSE_FUNC eFunc, string valueDefault)
        {
            TimeSpan tsRes = TimeSpan.MinValue;

            Func<string, DateTime, TimeSpan> func
                // обычное извлечение/разбор значения
                , native = delegate (string value, DateTime datetimeNow) {
                // 2-nd argument not used
                    return TimeSpan.Parse (value);
                }
                // функция получения интервала от текущего времени до указанного в аргументе
                , difference = delegate (string value, DateTime datetimeNow) {
                TimeSpan tsRet = TimeSpan.Zero
                    , tsNow = TimeSpan.FromSeconds (datetimeNow.Minute * 60 + datetimeNow.Second)
                    , tsValue = native (value, DateTime.MinValue); // 2-nd argument not used

                tsValue = tsValue.Add(TimeSpan.FromHours((int)tsValue.TotalHours).Negate());
                tsRet = tsValue - tsNow;

                if (!(tsRet > TimeSpan.Zero))
                // если текущее время превысило установленное в аргументе (в текущем часе)
                // , то возвратить интервал до установленного в аргументе в следующем
                    tsRet += TimeSpan.FromHours (1);
                else
                    ;

                return tsRet;
            };

            func = eFunc == TIMESPAN_PARSE_FUNC.NATIVE
                ? native
                    : eFunc == TIMESPAN_PARSE_FUNC.DIFFERENCE
                        ? difference
                            : null;
            try {
                tsRes =
                    //(TimeSpan.ParseExact (values [1], values [0], System.Globalization.CultureInfo.InvariantCulture))
                    //TimeSpan.Parse (values [1])
                    func (values [1], now)
                    ;
            } catch (Exception e) {
                ASUTP.Logging.Logg ().Exception (e, $"FileAppSettings::parseTimeSpan (Length={values.Length}, Values=<{string.Join (DELIM, values)}>, Func={eFunc}) - ...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
            } finally {
                if (tsRes.Equals (TimeSpan.MinValue) == true) {
                    tsRes = func (valueDefault, now);
                } else
                    ;
            }


            return tsRes;
        }

        /// <summary>
        /// Признак наличия ключа в файле конфигурации
        /// </summary>
        /// <param name="key">Ключ в файле конфигурации</param>
        /// <returns>Признак наличия ключа</returns>
        public bool IsContainsKey (string key)
        {
            return _config.AllKeys.Contains (key);
        }

        /// <summary>
        /// Признак наличия значения по ключу и отличиия значения от 'null' и пустого
        /// </summary>
        /// <param name="key">Ключ в файле конфигурации</param>
        /// <returns>Признак соответствия предъявляемым требованиям</returns>
        public bool IsNotNullOrEmpty (string key)
        {
            return IsContainsKey (key)
                && string.IsNullOrEmpty(getValue(key)) == false;
        }

        /// <summary>
        /// Добавить необходимую пару ключ + значение в секцию конфигурации приложения
        /// </summary>
        /// <param name="range">Пара ключ + значение для добавления</param>
        public void AddRequired (string key, string value)
        {
            addRequired (key, value);
        }

        /// <summary>
        /// Добавить необходимую пару ключ + значение в секцию конфигурации приложения
        /// </summary>
        /// <param name="range">Пара ключ + значение для добавления</param>
        public void AddRequired (string key, object value)
        {
            addRequired (key, value.ToString());
        }

        /// <summary>
        /// Добавить массив необходимых пар ключ + значение в секцию конфигурации приложения
        /// </summary>
        /// <param name="range">Массив пар ключ + значение для добавления</param>
        public void AddRequired (IEnumerable<KeyValuePair<string, string>> range)
        {
            range.ToList ().ForEach (pair => AddRequired (pair.Key, pair.Value));
        }

        /// <summary>
        /// Добавить массив необходимых пар ключ + значение в секцию конфигурации приложения
        /// </summary>
        /// <param name="range">Массив пар ключ + значение для добавления</param>
        public void AddRequired (IEnumerable<Tuple<string, string>> range)
        {
            range.ToList ().ForEach (pair => AddRequired (pair.Item1, pair.Item2));
        }

        /// <summary>
        /// Добавить массив необходимых пар ключ + значение в секцию конфигурации приложения
        /// </summary>
        /// <param name="range">Массив пар ключ + значение для добавления</param>
        public void AddRequired (IEnumerable<KeyValuePair<string, object>> range)
        {
            range.ToList ().ForEach (pair => AddRequired(pair.Key, pair.Value));
        }

        private string getValue (string key, string valueDefault = "")
        {
            return _config.AllKeys.Contains (key) == true
                ? _config [key].Value
                    : valueDefault;
        }

        /// <summary>
        /// Возвратить значение по ключу
        /// </summary>
        /// <param name="key">Ключ параметра</param>
        /// <param name="valueDefault">Значение по умолчанию при отсутствии ключа</param>
        /// <returns>Значение параметра или значение по умолчанию</returns>
        public string GetValue (string key, string valueDefault = "")
        {
            return getValue(key, valueDefault);
        }

        /// <summary>
        /// Возвратить значение по ключу в соответствии с индексом из формы с параметрами приложения
        /// </summary>
        /// <param name="indx">Индекс параметра из формы</param>
        /// <param name="value">Значение по умолчанию при отсутствии ключа</param>
        /// <returns>Значение параметра или значение по умолчанию</returns>
        public string GetValueOfMainIndexParameter (FormParameters.PARAMETR_SETUP indx, string valueDefault = "")
        {
            return getValue (FormParameters.GetNameParametersOfIndex ((int)indx), valueDefault);
        }

        /// <summary>
        /// Возвратить значение по ключу в соответствии с индексом из формы с параметрами приложения
        /// </summary>
        /// <param name="indx">Индекс параметра из формы</param>
        /// <param name="value">Значение по умолчанию при отсутствии ключа</param>
        /// <returns>Значение параметра или значение по умолчанию</returns>
        public string GetValueOfMainIndexParameter (int indx, string valueDefault = "")
        {
            return GetValueOfMainIndexParameter ((FormParameters.PARAMETR_SETUP)indx, valueDefault);
        }

        private void setValue (string key, string value)
        {
            if (IsContainsKey(key) == false)
                _config.Add (key, value);
            else
                _config [key].Value = value;
            _config.CurrentConfiguration.Save (ConfigurationSaveMode.Modified, true);

            ConfigurationManager.RefreshSection ("appSettings");
        }

        /// <summary>
        /// Установить значения для ключа
        /// </summary>
        /// <param name="key">Ключ в файле конфигурации</param>
        /// <param name="value">Устанавливаемое значение</param>
        public void SetValue (string key, string value)
        {
            setValue (key, value);
        }

        /// <summary>
        /// Установить значения для ключа
        /// </summary>
        /// <param name="key">Ключ в файле конфигурации</param>
        /// <param name="value">Устанавливаемое значение</param>
        public void SetValue (string key, object value)
        {
            setValue (key, value.ToString());
        }
    }
}
