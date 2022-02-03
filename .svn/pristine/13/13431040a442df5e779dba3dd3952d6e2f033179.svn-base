using System;
using System.Reflection;
using StatisticTrans;
using HClassLibrary;
using StatisticCommon;

namespace StatisticTrans
{
    /// <summary>
    /// Класс для создания спец имени для мьютекса
    /// </summary>
    static public class ProgramInfo
    {
        /// <summary>
        /// Создает уникальный идентификатор
        /// </summary>
        static public string AssemblyGuid
        {
            get
            {
                object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
                if (attributes.Length == 0)
                    return String.Empty;

                return ((System.Runtime.InteropServices.GuidAttribute)attributes[0]).Value;
            }
        }

        static public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                        return titleAttribute.Title;
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
            }
        }
    }
}

