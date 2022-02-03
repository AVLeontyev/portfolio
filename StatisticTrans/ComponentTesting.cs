using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatisticTrans;
using StatisticCommon;

namespace StatisticTrans
{
    /// <summary>
    /// Класс "Главная форма Trans (Передача?)"
    /// </summary>
    partial class FormMainTrans
    {
        /// <summary>
        /// Класс для проверки количества выполненных успещно/с_ошибками операций
        /// </summary>
        protected class ComponentTesting
        {
            /// <summary>
            /// Кол-во пройденных итераций
            /// </summary>
            private int successIter = 0;

            /// <summary>
            /// Текущая иттерация
            /// </summary>
            private int currentIter;

            private int commonSuccessIter
                , commonTotalIter;

            /// <summary>
            /// Кол-во компонентов
            /// </summary>
            private FormChangeMode.KeyDevice [] Iters;

            /// <summary>
            /// Конструктор класса
            /// </summary>
            /// <param name="size">размер массива</param>
            public ComponentTesting (int size)
            {
                Iters = new FormChangeMode.KeyDevice [size];
                currentIter =
                commonSuccessIter =
                commonTotalIter =
                    0;
            }

            /// <summary>
            /// Попытка итерации
            /// </summary>
            /// <param name="nameElem">имя опрашеваемого компонента</param>
            public void AttemptIter (FormChangeMode.KeyDevice key)
            {
                if (currentIter == CountIter)
                    ClearCounter ();
                else
                    ;

                Iters [currentIter] = key;
                currentIter++;
                commonTotalIter++;

                reportIter (getTextReportSuccessIter ());
            }

            /// <summary>
            /// Успешные итерации
            /// </summary>
            public void SuccessIter ()
            {
                successIter++;
                commonSuccessIter++;

                reportIter (getTextReportSuccessIter ());

                //resultIter();
            }

            /// <summary>
            /// Кол-во итераций
            /// </summary>
            /// <returns>кол-во итераций</returns>
            private int CountIter
            {
                get { return Iters.Length; }
            }

            /// <summary>
            /// Уменьшение счетчика итераций
            /// </summary>
            public void ErrorIter ()
            {
                reportIter (getTextReportErrorIter ());

                //resultIter();
            }

            //private void resultIter ()
            //{
            //    if (CountPart == currentIter)
            //        ClearStck();
            //    else
            //        ;
            //}

            private void reportIter (string text)
            {
                FormMainTrans.m_labelTime.Invoke (new Action (() => FormMainTrans.m_labelTime.Text = text));
                FormMainTrans.m_labelTime.Invoke (new Action (() => FormMainTrans.m_labelTime.Update ()));
            }

            /// <summary>
            /// отчет об успешных итерациях
            /// </summary>
            private string getTextReportSuccessIter ()
            {
                return "Время крайнего опроса: "
                    + DateTime.Now.ToString () + ";"
                    + " Успешных итераций(всего): " + successIter
                        + "(" + commonSuccessIter + @")"
                    + " из " + CountIter
                        + @"(" + commonTotalIter + @")";
            }

            /// <summary>
            /// Очистка итераций
            /// </summary>
            private void ClearCounter ()
            {
                currentIter = 0;
                successIter = 0;
            }

            private bool IsValidateIters
            {
                get
                {
                    return (
                        (!(Iters == null))
                        && (!(currentIter < 0))
                        && (currentIter < CountIter)
                        && (!(Iters [currentIter] == null))
                        );
                }
            }

            /// <summary>
            /// отчет об ошибочных итерациях
            /// </summary>
            private string getTextReportErrorIter ()
            {
                return getTextReportSuccessIter ()
                    + string.Format ("; Ошибка на компоненте: {0}, индекс={1}", (IsValidateIters ? Iters [currentIter].ToString () : @"не известно"), currentIter);
            }
        }
    }
}
