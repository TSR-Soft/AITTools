using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace AndromedaIT
{
    /// <summary>
    /// Summary description for DbTools
    /// </summary>
    public class DbTools
    {
        /* Internal Pointer */
        private static DbTools _Instance;

        /* The Instance */
        public static DbTools Instance
        {
            get
            {
                /* Sanity */
                if (_Instance == null)
                    _Instance = new DbTools();

                /* Return the internal pointer */
                return _Instance;
            }
        }

        /* List of objects */
        List<String> _MailTarget;
        Object _FileLock;
        Object _MailLock;
        String _FileTarget;
        Boolean _EventLogEnabled;
        EventLog _EventLog;

        /* Constructor */
        private DbTools()
        {
            /* Setup Variables */
            _FileLock = new Object();
            _MailLock = new Object();
            _FileTarget = "Debug.txt";
            _MailTarget = new List<String>();

            try
            {
                /* Create our Event Source */
                if (!EventLog.SourceExists("DebugAPI"))
                {
                    //An event log source should not be created and immediately used.
                    //There is a latency time to enable the source, it should be created
                    //prior to executing the application that uses the source.
                    //Execute this sample a second time to use the new source.
                    EventLog.CreateEventSource("DebugAPI", "Andromeda Debug Log Channel");

                    /* Disable use untill next time */
                    _EventLogEnabled = false;
                    _EventLog = null;
                }
                else
                {
                    _EventLog = new EventLog();
                    _EventLog.Source = "DebugAPI";
                    _EventLog.MachineName = ".";
                    _EventLogEnabled = true;
                }
            } catch (Exception) { }
        }

        /* Set file target */
        public void SetLogPath(String pPath)
        {
            /* Update */
            _FileTarget = pPath;

            /* Encapsulate such methods */
            try
            {
                /* Make sure path exists */
                if (!Directory.Exists(Path.GetDirectoryName(_FileTarget)))
                    Directory.CreateDirectory(Path.GetDirectoryName(_FileTarget));
            } catch (Exception) { }
        }

        /* Debug to file */
        public void DebugFile(String Message)
        {
            lock (_FileLock)
            {
                try
                {
                    using (StreamWriter mWriter = new StreamWriter(_FileTarget, true))
                    {
                        /* Append */
                        mWriter.WriteLine(DateTime.Now.ToString() + " >> " + Message);
                        mWriter.Flush();
                        mWriter.Close();
                    }
                } catch (Exception) { }
            }
        }

        /* Add mail to target */
        public void AddLogMail(String Mail)
        {
            /* Sanity */
            if (_MailTarget.Contains(Mail.ToLower()))
                return;

            lock (_MailLock)
            {
                _MailTarget.Add(Mail.ToLower());
            }
        }

        /* Debug to mail(s) */
        public void DebugMail(String Subject, String Message)
        {
            /* Sanity */
            if (_MailTarget.Count == 0)
                return;
            
            lock (_MailLock)
            {
                /* Logging functions must never stop the program, 
                 * now that would be stupid */
                try
                {
                    using (MailMessage Mail = new MailMessage())
                    using (SmtpClient mClient = new SmtpClient())
                    {
                        /* Setup Client */
                        mClient.UseDefaultCredentials = false;
                        mClient.Port = 587;
                        mClient.Host = "mail.timeblock.com";

                        /* Add recipients */
                        foreach (String MA in _MailTarget)
                            Mail.To.Add(new MailAddress(MA));

                        /* Set mail details */
                        Mail.From = new System.Net.Mail.MailAddress("Debug@timeblock.com", "Debug API");
                        Mail.Subject = Subject;
                        Mail.Body = Message;

                        /* Send it */
                        mClient.Send(Mail);
                    }
                } catch (Exception) { }
            }
        }

        /* Debug to Event Log */
        public void DebugEventLog(String Message)
        {
            /* Sanity */
            if (!_EventLogEnabled)
                return;

            /* Logging functions must never stop the program, 
             * now that would be stupid */
            try
            {
                /* Write */
                _EventLog.WriteEntry("New Event at " + DateTime.Now.ToString() + ":\n\n" + Message);
            } catch (Exception) { }
        }
    }
}