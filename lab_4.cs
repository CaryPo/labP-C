using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Text.RegularExpressions;

namespace lab4
{
    public class Program
    {
        static class GlobalVars
        {
            static public CTableBuilder g_Table = new CTableBuilder();
            static public CWebScanner g_Scanner = new CWebScanner();

            static public int g_Scanlevel = 0;
        }

        static void OnBeginScan()
        {
            string sShiftRow = "";

            for (int i = 0; i < GlobalVars.g_Scanlevel; ++i)
                sShiftRow += "|--";

            GlobalVars.g_Table.WriteLine(sShiftRow + "Южно-Уральский Государственный Университет", ++GlobalVars.g_Scanlevel, "-", "-");
        }

        static void OnFindHighschoolStructures()
        {
            string sShiftRow = "";

            for (int i = 0; i < GlobalVars.g_Scanlevel; ++i)
                sShiftRow += "|--";

            GlobalVars.g_Table.WriteLine(sShiftRow + "Высшие школы и институты", ++GlobalVars.g_Scanlevel, "-", "-");
        }

        static void OnProcessContacts(Uri uPage, string sHighschoolName, string sAddress, string sPhoneNumber)
        {
            string sShiftRow = "";

            for (int i = 0; i < GlobalVars.g_Scanlevel; ++i)
                sShiftRow += "|--";

            GlobalVars.g_Table.WriteLine(sShiftRow + sHighschoolName, GlobalVars.g_Scanlevel, sAddress, sPhoneNumber);

            Console.WriteLine($"{sHighschoolName}\n{sAddress}\n{sPhoneNumber}\n");
        }

        static bool ProcessContactsHandled(bool bContactsFound)
        {
            return bContactsFound;
        }

        static void Main(string[] args)
        {
            GlobalVars.g_Table.BeginTable(false);
            GlobalVars.g_Table.WriteLine("Имя подразделения", "Уровень", "Адрес", "Номер(а) телефон(а)");

            GlobalVars.g_Scanner.OnBeginScan += OnBeginScan;
            GlobalVars.g_Scanner.OnFindHighschoolStructures += OnFindHighschoolStructures;
            GlobalVars.g_Scanner.OnProcessContacts += OnProcessContacts;

            GlobalVars.g_Scanner.AttachDelegate_ProcessContactsHandled(ProcessContactsHandled);

            GlobalVars.g_Scanner.Scan(new Uri("https://www.susu.ru"), new Uri("https://www.susu.ru/ru"), 2);

            GlobalVars.g_Table.EndTable("structures.csv");
        }

        public class CTableBuilder
        {
            public CTableBuilder()
            {
                m_stringBuilder = new StringBuilder();
                m_sSeparator = ",";
            }

            public void BeginTable(bool bDefaultSeparation)
            {
                m_stringBuilder.Clear();

                if (bDefaultSeparation)
                    m_sSeparator = ",";
                else
                    m_sSeparator = ";";
            }

            public void WriteLine(object param1)
            {
                string sLine = $"{param1}";
                m_stringBuilder.AppendLine(sLine);
            }
            
            public void WriteLine(object param1, object param2)
            {
                string sLine = $"{param1}{m_sSeparator}{param2}";
                m_stringBuilder.AppendLine(sLine);
            }
            
            public void WriteLine(object param1, object param2, object param3)
            {
                string sLine = $"{param1}{m_sSeparator}{param2}{m_sSeparator}{param3}";
                m_stringBuilder.AppendLine(sLine);
            }
            
            public void WriteLine(object param1, object param2, object param3, object param4)
            {
                string sLine = $"{param1}{m_sSeparator}{param2}{m_sSeparator}{param3}{m_sSeparator}{param4}";
                m_stringBuilder.AppendLine(sLine);
            }

            public void EndTable(string sFileName)
            {
                File.WriteAllText(sFileName, m_stringBuilder.ToString());
            }

            StringBuilder m_stringBuilder = null;
            string m_sSeparator = null;
        }

        public class CWebLink
        {
            public Uri m_uAddress { get; set; }
            public bool m_bLocal { get; set; }
            public string m_sUrl { get; set; }
        }

        public class CWebScanner : IDisposable
        {
            public CWebScanner()
            {
                m_webClient.Encoding = Encoding.UTF8;
                m_processContactsDelegate = ProcessContactsHandledFn;
            }

            private bool m_bProcessLinks = false;

            private readonly HashSet<Uri> m_procLinks = new HashSet<Uri>();
            private readonly HashSet<string> m_ignoreFiles = new HashSet<string> { ".ico", ".xml", ".css", ".png", ".svg" };
            private readonly WebClient m_webClient = new WebClient();

            private ProcessContactsHandled m_processContactsDelegate = null;

            public event Action OnBeginScan;
            public event Action OnFindHighschoolStructures;
            public event Action<Uri, string, string, string> OnProcessContacts;

            public delegate bool ProcessContactsHandled(bool bFoundContacts);

            private bool ProcessContactsHandledFn(bool bFoundContacts)
            {
                return false;
            }

            private bool ProcessContacts(string sDomain, string sHTML, string sMatchValue)
            {
                bool bContactsFound = false;

                sMatchValue = sMatchValue.Replace("class=\"field-item even\"><h1>", "").Replace("</h1>", "").Replace("&nbsp;", " ");

                Match match = Regex.Match(sMatchValue, @"<h1>.*<\/h1>");

                if (match.Success)
                    sMatchValue = match.Value;

                if (sMatchValue == "Высшие школы и институты")
                {
                    OnFindHighschoolStructures?.Invoke();

                    foreach (Match m in Regex.Matches(sHTML, @"col-md-6""><a href=""([\/\w-])+""><img"))
                    {
                        match = Regex.Match(m.Value, @"href=""[\/\w-\.:]+""");

                        string url = match.Value.Replace("href=", "").Trim('"');
                        bool local = url.StartsWith("/");
                        string sUrl = local ? $"{sDomain}{url}" : url;

                        Uri uPage = new Uri(sUrl);
                        string sHTMLPage = m_webClient.DownloadString(uPage);

                        //match = Regex.Match(sHTMLPage, @"""active"">[\w\s-,]+<\/li>");
                        match = Regex.Match(sHTMLPage, @"""page-header"">[\w\s-,]+<\/h1>");

                        if (match.Value.Length > 0)
                        {
                            string sHighschoolName, sAddress, sPhoneNumber;

                            sHighschoolName = match.Value;
                            sHighschoolName = sHighschoolName.Replace("\"page-header\">", "").Replace("</h1>", "");

                            sAddress = Regex.Match(sHTMLPage, @"[\d]{6}, [\w\s.]+,( |[\S]{6})[\w\s.]+, [\d]{1,}[\w](, [0-9\w]*)*").Value;
                            sPhoneNumber = Regex.Match(sHTMLPage, @"(\+7 )*(\({1}[\d]+\){1})[\s\d-,]+").Value;

                            if (sAddress.Length > 0 && sPhoneNumber.Length > 0)
                            {
                                sAddress = sAddress.Replace("&nbsp;", " ");
                                sPhoneNumber = sPhoneNumber.Trim(',').Trim(' ');

                                if (!sPhoneNumber.StartsWith("+7"))
                                    sPhoneNumber = "+7 " + sPhoneNumber;

                                OnProcessContacts?.Invoke(uPage, sHighschoolName, sAddress, sPhoneNumber);
                            }
                        }
                    }

                    bContactsFound = true;
                }

                return m_processContactsDelegate.Invoke(bContactsFound);
            }

            private void Process(string sDomain, Uri uPage, string sUrl, int nLevel)
            {
                if (nLevel <= 0 || !m_bProcessLinks)
                    return;

                if (m_procLinks.Contains(uPage))
                    return;

                m_procLinks.Add(uPage);

                string sHTMLPage;
                Match match;

                try
                {
                    sHTMLPage = m_webClient.DownloadString(uPage);
                }
                catch (Exception)
                {
                    return;
                }

                if ((match = Regex.Match(sHTMLPage, @"class=""field-item even""><h1>(.)+</h1>")).Success)
                {
                    if (ProcessContacts(sDomain, sHTMLPage, match.Value))
                    {
                        m_bProcessLinks = false;
                        return;
                    }
                }

                //Console.WriteLine(uPage);

                List<CWebLink> hrefs = new List<CWebLink>();

                foreach (Match href in Regex.Matches(sHTMLPage, @"href=""[\/\w-\.:]+"""))
                {
                    try
                    {
                        string url = href.Value.Replace("href=", "").Trim('"');

                        if (!Regex.Match(url, @".*/ru/.*").Success)
                            continue;

                        bool local = url.StartsWith("/");
                        string _url = local ? $"{sDomain}{url}" : url;

                        hrefs.Add(new CWebLink
                        {
                            m_uAddress = new Uri(_url),
                            m_bLocal = local || url.StartsWith(sDomain),
                            m_sUrl = _url
                        });
                    }
                    catch (Exception)
                    {
                        
                    }
                }

                if (--nLevel > 0)
                {
                    foreach (var href in hrefs)
                    {
                        if (!m_bProcessLinks)
                            break;

                        if (!href.m_bLocal)
                            continue;

                        string fileEx = Path.GetExtension(href.m_uAddress.LocalPath).ToLower();

                        if (m_ignoreFiles.Contains(fileEx))
                            continue;

                        Process(sDomain, href.m_uAddress, href.m_sUrl, nLevel);
                    }
                }
            }

            public void Scan(Uri uHostPage, Uri uStartPage, int nLevel)
            {
                m_bProcessLinks = true;
                m_procLinks.Clear();

                OnBeginScan?.Invoke();

                string sDomain = $"{uHostPage.Scheme}://{uHostPage.Host}";
                Process(sDomain, uStartPage, $"{uStartPage.Scheme}://{uStartPage.Host}", nLevel);
            }

            public void AttachDelegate_ProcessContactsHandled(ProcessContactsHandled fn)
            {
                m_processContactsDelegate = fn;
            }

            public void Dispose()
            {
                m_webClient.Dispose();
            }
        }
    }
}
