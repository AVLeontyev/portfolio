using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; //...File
using System.Windows.Forms;
using System.Threading; //...Thread
using System.Media; //...SoundPlayer

//using HClassLibrary;
using StatisticCommon;
using ASUTP.Core;

namespace StatisticAlarm
{
    /// <summary>
    /// Класс для окна с сообщением о событии сигнализации
    /// </summary>
    public class MessageBoxAlarmEvent
    {
        private Queue<AlarmNotifyEventArgs> _queueArgs;

        //public delegate void ActivateTabPageEventHandler (int indx, bool active);
        /// <summary>
        /// Событие - после квитирования - активация вкладки
        /// </summary>
        public event DelegateBoolFunc/*ActivateTabPageEventHandler*/ EventActivateTabPage;
        /// <summary>
        /// Событие - пользователь зафиксировал(квитировал) сообщение
        /// </summary>
        public event DelegateObjectFunc EventFixed;

        private int m_iAlarmEventCounter;
        private int AlarmEventCounter { get { return m_iAlarmEventCounter; } set { m_iAlarmEventCounter = value; } }
        private SoundPlayer m_sndAlarmEvent;
        private
            //System.Threading.Timer
            System.Windows.Forms.Timer
                m_timerAlarmEvent;
        private Form _owner;

        public MessageBoxAlarmEvent(Form owner)
        {
            _owner = owner;
            _queueArgs = new Queue<AlarmNotifyEventArgs>();
        }

        //private void timerAlarmEvent (object obj)
        private void timerAlarmEvent(object obj, EventArgs ev)
        {
            //System.Media.SystemSounds.Question.Play();
            if (m_sndAlarmEvent == null)
                Console.Beep();
            else
                m_sndAlarmEvent.Play();
        }

        private void messageBoxShow(object text)
        {
            ////Вариант №1
            //MessageBox.Show((string)text, @"Сигнализация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //Вариант №2
            MessageBox.Show((string)text, @"Сигнализация", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);

            _owner.BeginInvoke(new DelegateFunc(messageBoxHide));
        }

        private void messageBoxHide()
        {
            //bool bContinue = false;

            lock (this)
            {
                m_iAlarmEventCounter--;

                if (m_iAlarmEventCounter == 0)
                {
                    //m_timerAlarmEvent.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    m_timerAlarmEvent.Stop();
                    m_timerAlarmEvent.Dispose();
                    m_timerAlarmEvent = null;

                    if (!(m_sndAlarmEvent == null))
                    {
                        m_sndAlarmEvent.Stop();
                        m_sndAlarmEvent.Dispose();
                        m_sndAlarmEvent = null;
                    }
                    else
                        ;

                    //bContinue = true;

                    //Активация текущей вкладки
                    EventActivateTabPage(true);
                }
                else
                    ;

                EventFixed(_queueArgs.Dequeue());
            }
        }

        public void MessageBoxShow(object ev)
        {
            //int selIndxTabPage = -1;
            //string text = string.Empty;

            lock (this)
            {
                //selIndxTabPage = (int)(obj as object[])[0];
                //text = (string)(obj as object[])[1];

                if (m_timerAlarmEvent == null)
                {
                    //Деактивация текущей вкладки
                    EventActivateTabPage(false);

                    string strPathSnd = Environment.GetEnvironmentVariable("windir") + @"\Media\" + AdminAlarm.FNAME_ALARM_SYSTEMMEDIA_TIMERBEEP;
                    if (File.Exists(strPathSnd) == true)
                        m_sndAlarmEvent = new SoundPlayer(strPathSnd);
                    else
                        ;

                    m_timerAlarmEvent =
                        //new System.Threading.Timer(new TimerCallback(timerAlarmEvent), null, 0, AdminAlarm.MSEC_ALARM_TIMERBEEP) //Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_TIMER_BEEP]) * 1000
                        new System.Windows.Forms.Timer();
                    m_timerAlarmEvent.Tick += new EventHandler(timerAlarmEvent);
                    m_timerAlarmEvent.Interval = AdminAlarm.MSEC_ALARM_TIMERBEEP;
                    m_timerAlarmEvent.Start();

                    m_iAlarmEventCounter = 1;
                }
                else
                    m_iAlarmEventCounter++;

                _queueArgs.Enqueue(ev as AlarmNotifyEventArgs);
            }

            //Поверх остальных окон
            bool bPrevTopMost = _owner.TopMost;
            _owner.TopMost = true;
            //Диалоговое окно
            new Thread(new ParameterizedThreadStart(messageBoxShow)).Start((ev as AlarmNotifyEventArgs).m_messageGUI);
            //Востановить значение по умолчанию
            _owner.TopMost = bPrevTopMost;
        }
    }
}
