using StatisticCommon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.ObjectModel;

namespace StatisticCommon
{
    /// <summary>
    /// Интерфейс взаимодействия с файлом конфигурации 'app.config'
    /// </summary>
    public interface IFileAppSettings
    {
        /// <summary>
        /// Возвратить строку-идентификатор приложения 'ASUTP.Helper.ProgramBase.ID_APP'
        /// </summary>
        ASUTP.Helper.ProgramBase.ID_APP GetIdApplication { get; }

        /// <summary>
        /// Добавить необходимую пару ключ + значение в секцию конфигурации приложения
        /// </summary>
        /// <param name="range">Пара ключ + значение для добавления</param>
        void AddRequired (string key, string value);

        /// <summary>
        /// Добавить необходимую пару ключ + значение в секцию конфигурации приложения
        /// </summary>
        /// <param name="range">Пара ключ + значение для добавления</param>
        void AddRequired (string key, object value);

        /// <summary>
        /// Добавить массив необходимых пар ключ + значение в секцию конфигурации приложения
        /// </summary>
        /// <param name="range">Массив пар ключ + значение для добавления</param>
        void AddRequired (IEnumerable<KeyValuePair<string, string>> range);

        /// <summary>
        /// Добавить массив необходимых пар ключ + значение в секцию конфигурации приложения
        /// </summary>
        /// <param name="range">Массив пар ключ + значение для добавления</param>
        void AddRequired (IEnumerable<KeyValuePair<string, object>> range);

        /// <summary>
        /// Возвратить значение по ключу
        /// </summary>
        /// <param name="key">Ключ параметра</param>
        /// <param name="valueDefault">Значение по умолчанию при отсутствии ключа</param>
        /// <returns>Значение параметра или значение по умолчанию</returns>
        string GetValue (string key, string valueDefault = "");

        /// <summary>
        /// Возвратить значение по ключу в соответствии с индексом из формы с параметрами приложения
        /// </summary>
        /// <param name="indx">Индекс параметра из формы</param>
        /// <param name="value">Значение по умолчанию при отсутствии ключа</param>
        /// <returns>Значение параметра или значение по умолчанию</returns>
        string GetValueOfMainIndexParameter (FormParameters.PARAMETR_SETUP indx, string valueDefault = "");

        /// <summary>
        /// Возвратить значение по ключу в соответствии с индексом из формы с параметрами приложения
        /// </summary>
        /// <param name="key">Индекс параметра из формы</param>
        /// <param name="value">Значение по умолчанию при отсутствии ключа</param>
        /// <returns>Значение параметра или значение по умолчанию</returns>
        string GetValueOfMainIndexParameter (int indx, string valueDefault = "");

        /// <summary>
        /// Установить значения для ключа
        /// </summary>
        /// <param name="key">Ключ в файле конфигурации</param>
        /// <param name="value">Устанавливаемое значение</param>
        void SetValue (string key, string value);

        /// <summary>
        /// Установить значения для ключа
        /// </summary>
        /// <param name="key">Ключ в файле конфигурации</param>
        /// <param name="value">Устанавливаемое значение</param>
        void SetValue (string key, object value);

        /// <summary>
        /// Признак наличия ключа в файле конфигурации
        /// </summary>
        /// <param name="key">Ключ в файле конфигурации</param>
        /// <returns>Признак наличия ключа</returns>
        bool IsContainsKey (string key);

        /// <summary>
        /// Признак наличия значения по ключу и отличиия значения от 'null' и пустого
        /// </summary>
        /// <param name="key">Ключ в файле конфигурации</param>
        /// <returns>Признак соответствия предъявляемым требованиям</returns>
        bool IsNotNullOrEmpty (string key);
    }
}
