using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace strans
{
    /// <summary>
    /// Обработки аргументов командной строки
    /// </summary>
    public class CmdArg : IDisposable
    {
        private static Creator.MODE [] _mode
        {
            get; set;
        }

        public static Creator.MODE ModeHost
        {
            get
            {
                return _mode [(int)Creator.INDEX_MODE.Host];
            }
        }

        public static Creator.MODE ModeClient
        {
            get
            {
                return _mode [(int)Creator.INDEX_MODE.Client];
            }
        }

        public static bool IsHostRunable
        {
            get
            {
                return ModeHost.Equals (Creator.MODE.Unknown) == false;
            }
        }

        public static bool IsClientRunable
        {
            get
            {
                return ModeClient.Equals (Creator.MODE.Unknown) == false;
            }
        }

        private bool validate (IEnumerable<ARGUMENT> args, out Creator.INDEX_MODE indxMode)
        {
            bool bRes = false;
            indxMode = Creator.INDEX_MODE.Unknown;

            Dictionary<LARGUMENT, IEnumerable<ARGUMENT>> verifies;

            // правила проверки
            // 0) мин. кол-во элементов "2"
            // 1) наличие элемента с уровнем LARGUMENT.Mode, LARGUMENT.SubMode
            // 2) на каждый уровень по одному значению
            // 3) совместимость 2-го к 1-му
            // 4)* совместимость 3-го ко 2-му
            // 5)* совместимость 3-го к 1-му
            // * - при наличии 3-го

            verifies = new Dictionary<LARGUMENT, IEnumerable<ARGUMENT>> () {
                    { LARGUMENT.Mode, from arg in args where arg.Level == LARGUMENT.Mode select arg }
                    , { LARGUMENT.SubMode, from arg in args where arg.Level == LARGUMENT.SubMode select arg }
                    , { LARGUMENT.Action, from arg in args where arg.Level == LARGUMENT.Action select arg }
                };

            bRes =
                // правила №№0-2
                verifies.ToList ().TrueForAll (verify => verify.Value.Count () == 1)
                    // правило №3
                    && !(verifies [LARGUMENT.Mode].ElementAt (0).Index > verifies [LARGUMENT.SubMode].ElementAt (0).Index)
                ;

            indxMode = ((verifies.ContainsKey (LARGUMENT.Mode) == true)
                && (verifies [LARGUMENT.Mode].Count () == 1))
                ? verifies [LARGUMENT.Mode].ElementAt (0).Index
                    : Creator.INDEX_MODE.Unknown;

            return bRes;
        }

        /// <summary>
        /// Перечисление - типы аргументов
        /// </summary>
        private enum LARGUMENT
        {
            Unknown = -1
            , Mode
            , SubMode
            , Action
        }

        /// <summary>
        /// Перечисление - минимальная составная часть аргумента командной строки
        /// </summary>
        private enum KARGUMENT
        {
            unknown = -1
            , host
            , client
            , console
            , gui
            , scnet
            , install
            , uninstall
            , start
            , stop
            , restart
        }

        /// <summary>
        /// Структура для описания части аргумента для любого из типов объектов (host, client)
        /// </summary>
        private struct ARGUMENT
        {
            public LARGUMENT Level;

            public KARGUMENT Key;

            public Creator.MODE Mode;

            public Creator.INDEX_MODE Index;
        }

        /* [Примеры допустимых аргументов командной строки] - [с пояснением поведения приложения]
         * [---host--console] - [развертывание служб в консоли, клиент на выполнение не запускается]
         * [---host--ScNet-install] - [проверка ранее выполненной регистрации служб в качестве "служб ОС", клиент на выполнение не запускается]
         * [---host--ScNet-start] - [службы запускаются на выполнение, клиент на выполнение не запускается]
         * [---host--ScNet-restart] - [службы последовательно останавливаются/запускаются, клиент на выполнение не запускается]
         * [---host--ScNet-stop] - [службы останавливаются, клиент на выполнение не запускается]
         * [---host--ScNet-uninstall] - [проверяется регистрация служб в ОС - при наличии устанавливается признак "к удалению", клиент на выполнение не запускается]
         * [---client--gui-start] - [развертывание служб не выполняется, клиент выполненяется в реж. с граф. интерфейсом]
         * [---client--gui-stop] - [развертывание служб не выполняется, клиенту передается аргумент на прекращение выполнения]
         * [---client--console-start] - [развертывание служб не выполняется, клиент выполненяется в реж. с граф. интерфейсом]
         * [---client--console-stop] - [развертывание служб не выполняется, клиенту передается аргумент на прекращение выполнения]
        */

        /// <summary>
        /// Конструктор - основной (без аргументов)
        /// </summary>
        public CmdArg ()
            : this (Environment.GetCommandLineArgs ().Skip (1).ToArray (), false)
        {
        }

        public CmdArg (bool bDebug)
            : this (Environment.GetCommandLineArgs ().Skip (1).ToArray (), bDebug)
        {
        }

        /// <summary>
        /// Конструктор - основной (без аргументов)
        /// </summary>
        public CmdArg (string [] args, bool bDebug)
        {
            _mode = new Creator.MODE [(int)Creator.INDEX_MODE.All];
            _mode [(int)Creator.INDEX_MODE.Host] =
            _mode [(int)Creator.INDEX_MODE.Client] =
                Creator.MODE.Unknown;

            if (bDebug == true) {
                // по умолчанию
                _mode [(int)Creator.INDEX_MODE.Host] = Creator.MODE.Host | Creator.MODE.Console | Creator.MODE.Start;
                _mode [(int)Creator.INDEX_MODE.Client] =
                    //Creator.MODE.Client | Creator.MODE.Console | Creator.MODE.Start
                    Creator.MODE.Unknown
                    ;
            } else {
                parse (args);
            }
        }

        private void parse (IEnumerable<string> sensed)
        {
            Creator.INDEX_MODE indxMode = Creator.INDEX_MODE.Unknown;
            IEnumerable<KARGUMENT> arg_parts;
            IEnumerable<ARGUMENT> args;

            // Список, определяющий соотношения тип аргумента - минимальная часть аргумента
            List<ARGUMENT> listArgumentTemplate;
            listArgumentTemplate = new List<ARGUMENT> () {
                    new ARGUMENT { Level = LARGUMENT.Unknown    , Key = KARGUMENT.unknown   , Mode = Creator.MODE.Unknown   , Index = Creator.INDEX_MODE.Unknown }
                    , new ARGUMENT { Level = LARGUMENT.Mode     , Key = KARGUMENT.host      , Mode = Creator.MODE.Host      , Index = Creator.INDEX_MODE.Host }
                    , new ARGUMENT { Level = LARGUMENT.Mode     , Key = KARGUMENT.client    , Mode = Creator.MODE.Client    , Index = Creator.INDEX_MODE.Client }
                    , new ARGUMENT { Level = LARGUMENT.SubMode  , Key = KARGUMENT.console   , Mode = Creator.MODE.Console   , Index = Creator.INDEX_MODE.All }
                    , new ARGUMENT { Level = LARGUMENT.SubMode  , Key = KARGUMENT.scnet     , Mode = Creator.MODE.ScNet     , Index = Creator.INDEX_MODE.Host }
                    , new ARGUMENT { Level = LARGUMENT.SubMode  , Key = KARGUMENT.gui       , Mode = Creator.MODE.Gui       , Index = Creator.INDEX_MODE.Client }
                    , new ARGUMENT { Level = LARGUMENT.Action   , Key = KARGUMENT.install   , Mode = Creator.MODE.Install   , Index = Creator.INDEX_MODE.Host }
                    , new ARGUMENT { Level = LARGUMENT.Action   , Key = KARGUMENT.uninstall , Mode = Creator.MODE.Uninstall , Index = Creator.INDEX_MODE.Host }
                    , new ARGUMENT { Level = LARGUMENT.Action   , Key = KARGUMENT.start     , Mode = Creator.MODE.Start     , Index = Creator.INDEX_MODE.All }
                    , new ARGUMENT { Level = LARGUMENT.Action   , Key = KARGUMENT.stop      , Mode = Creator.MODE.Stop      , Index = Creator.INDEX_MODE.All }
                    , new ARGUMENT { Level = LARGUMENT.Action   , Key = KARGUMENT.restart   , Mode = Creator.MODE.Restart   , Index = Creator.INDEX_MODE.All }
                };

            foreach (string s in sensed) {
                try {
                    // в случае неизвестного ключа - произойдет исключение - режим останется "как в конструкторе" (Creator.MODE.Unknown)
                    arg_parts = from karg in s.Split (new string [] { "-" }, StringSplitOptions.RemoveEmptyEntries)
                                select (KARGUMENT)Enum.Parse (typeof (KARGUMENT), karg);
                    args = from argTemplate in listArgumentTemplate
                           join argPart in arg_parts on argTemplate.Key equals argPart
                           select argTemplate;
                    //TODO: проверить 'Mode' на совместимость
                    if (validate (args, out indxMode) == true) {
                        // установить режим для указанного в аргументе индекса, разобрать (при наличии) дополнительные
                        args.ToList ().ForEach (arg => {
                            _mode [(int)indxMode] |= arg.Mode;
                        });
                    } else
                        // $"Аргумент не разобран: argument=[{s}]..."
                        // проверить наличие определяющей части для режима
                        if (!(indxMode == Creator.INDEX_MODE.Unknown))
                        _mode [(int)indxMode] = Creator.MODE.Error;
                    else
                        ;
                } catch (Exception e) {
                    ASUTP.Logging.Logg ().Exception (e, $"strans.CmdArg::setMode () - argument=<{s}>...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        }

        public void Dispose ()
        {

        }

        public string AppName
        {
            get
            {
                return IsHostRunable == true
                    ? ((ConfigSectionServiceSettings)ConfigurationManager.GetSection ($"{ConfigSectionServiceSettings.NameSection}")).Logging.Prefix
                        : IsClientRunable == true
                            ? ((ConfigSectionModesTrans)ConfigurationManager.GetSection ($"{ConfigSectionModesTrans.NameSection}")).Logging.Prefix
                                : "strans-error";
            }
        }

        #region в наследство
        /// <summary>
        /// Обрабатывает переданные при вызове параметры. Возвращает флаг необходимости выхода из программы.
        /// </summary>
        private static bool proc_args (string [] args)
        {
            //??? Properties.Settings sett = new Properties.Settings();

            Console.WriteLine (System.Diagnostics.FileVersionInfo.GetVersionInfo (AppDomain.CurrentDomain.SetupInformation.ApplicationName).FileDescription);
            Console.WriteLine (System.Diagnostics.FileVersionInfo.GetVersionInfo (AppDomain.CurrentDomain.SetupInformation.ApplicationName).CompanyName);
            Console.WriteLine (System.Diagnostics.FileVersionInfo.GetVersionInfo (AppDomain.CurrentDomain.SetupInformation.ApplicationName).LegalCopyright);

            Console.WriteLine (Environment.NewLine + "Known command line arguments: /? /list[=DD.MM.YYYY] /nowait /setmysqlpassword" + Environment.NewLine);

            return false;
        }
        #endregion
    }
}
