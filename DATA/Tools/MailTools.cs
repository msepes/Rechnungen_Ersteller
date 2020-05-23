using Angeboten;
using Rechnungen;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using ToolBox;

namespace DATA.Tools
{
    public static class MailTools
    {


        private static string GetStringFromToken(TemplateParser.Token token, Benutzer User, Rechnung Rechnung = null, Angebot angebot = null)
        {
            if (token.TryGetLiteral(out string literal))
                return literal;

            if (!token.TryGetKeyWord(out Tuple<string, string> keyword))
                throw new Exception("parse Error");

            var Objekt = keyword.Item1;
            var Property = keyword.Item2;


            switch (Objekt)
            {
                case nameof(Rechnung):
                    return GetProperty(Property, Rechnung);
                case nameof(Kunde):
                    return GetProperty(Property, Rechnung==null?angebot.Kunde:Rechnung.Kunde);

                case nameof(Benutzer):
                    return GetProperty(Property, User);

                case nameof(Angebot):
                    return GetProperty(Property, angebot);

                default:
                    return $"{{{Objekt}.{Property}}}";
            }
        }

        private static string GetProperty(string PropertyName, Rechnung rechnung)
        {
            if (rechnung == null)
                return PropertyName;

            switch (PropertyName)
            {
                case nameof(rechnung.Datum):
                    return rechnung.Datum.ToShortDateString();

                case nameof(rechnung.LeistungsDatum):
                    return rechnung.LeistungsDatum.ToShortDateString();

                case nameof(rechnung.Nr):
                    return rechnung.Nr.ToString();

                case nameof(rechnung.Umsatzsteuer):
                    return rechnung.Umsatzsteuer.ToString();

                case nameof(rechnung.Summe):
                    return rechnung.Summe().ToString();

                case nameof(rechnung.MitRabatt):
                    return rechnung.MitRabatt().ToString();

                case nameof(rechnung.MitSteuer):
                    return rechnung.MitSteuer().ToString();
                default:
                    return $"{{{nameof(Rechnung)}.{PropertyName}}}";
            }

        }

        private static string GetProperty(string PropertyName, Angebot angebot)
        {
            if (angebot == null)
                return PropertyName;

            switch (PropertyName)
            {
                case nameof(angebot.Datum):
                    return angebot.Datum.ToShortDateString();

                case nameof(angebot.Nr):
                    return angebot.Nr.ToString();

                case nameof(angebot.Umsatzsteuer):
                    return angebot.Umsatzsteuer.ToString();

                case nameof(angebot.Summe):
                    return angebot.Summe().ToString();

                case nameof(angebot.MitRabatt):
                    return angebot.MitRabatt().ToString();

                case nameof(angebot.MitSteuer):
                    return angebot.MitSteuer().ToString();
                default:
                    return  $"{{{nameof(Angebot)}.{PropertyName}}}";
            }

        }

        private static string GetProperty(string PropertyName, Kunde kunde)
        {
            switch (PropertyName)
            {
                case nameof(kunde.addresse):
                    return kunde.addresse.ToString();

                case nameof(kunde.Nr):
                    return kunde.Nr.ToString();

                case nameof(kunde.Email):
                    return kunde.Email;

                case nameof(kunde.Ansprechpartner):
                    return kunde.Ansprechpartner;

                case nameof(kunde.Telephone):
                    return kunde.Telephone;

                case nameof(kunde.FirmaName):
                    return kunde.FirmaName;

                default:
                    return $"{{{nameof(kunde)}.{PropertyName}}}";
            }
        }

        private static string GetProperty(string PropertyName, Benutzer user)
        {
            switch (PropertyName)
            {
                case nameof(user.addresse):
                    return user.addresse.ToString();

                case nameof(user.Name):
                    return user.Name;

                case nameof(user.Vorname):
                    return user.Vorname;

                case nameof(user.FirmaName):
                    return user.FirmaName;

                case nameof(user.BankName):
                    return user.BankName;

                case nameof(user.IBAN):
                    return user.IBAN;

                case nameof(user.BIC):
                    return user.BIC;

                case nameof(user.SteuerID):
                    return user.SteuerID;

                case nameof(user.Email):
                    return user.Email;

                case nameof(user.Telefone):
                    return user.Telefone;

                case nameof(user.Web):
                    return user.Web;

                default:
                    return $"{{{nameof(Benutzer)}.{PropertyName}}}";
            }
        }

        public static string GetBetriff(string head, Benutzer User, Rechnung Rechnung = null, Angebot angebot = null)
        {
            try
            {
                return TemplateParser.Parse(head)
                                     .Aggregate(string.Empty, (acc, t) => acc + GetStringFromToken(t, User, Rechnung, angebot));
            }
            catch (TemplateParser.ParseError ex)
            {
                throw new Exception(ex.Data0);
            }

        }

        public static string GetBetriff(EmailConf conf, Benutzer User, Rechnung Rechnung = null, Angebot angebot = null)
        {
            return GetBetriff(conf.EmailBetriff, User, Rechnung, angebot);
        }


        public static string GetPreview(string body, Benutzer User, Rechnung Rechnung = null, Angebot angebot = null)
        {
            try
            {
                return TemplateParser.Parse(body)
                                 .Aggregate(string.Empty, (acc, t) => acc + GetStringFromToken(t, User, Rechnung, angebot));
            }
            catch (TemplateParser.ParseError ex)
            {
                throw new Exception(ex.Data0);
            }
        }

        public static string GetPreview(EmailConf conf, Benutzer User, Rechnung Rechnung = null, Angebot angebot = null)
        {
            return GetPreview(conf.EmailInhalt, User, Rechnung, angebot);
        }

        public static void SendMail(EmailConf conf, Benutzer User, string Head, string Body, Rechnung Rechnung)
        {


            if (conf == null || !conf.IsVaild())
                throw new ArgumentNullException($"keine gültige Email Konfiguration!");

            if (Rechnung == null)
                throw new ArgumentNullException($"{nameof(Rechnung)} darf nicht leer sein!");

            if (Rechnung.Kunde == null)
                throw new ArgumentNullException($"{nameof(Rechnung.Kunde)} darf nicht leer sein!");

            if (Rechnung.Kunde.Email == null)
                throw new ArgumentNullException($"{nameof(Rechnung.Kunde.Email)} von Kunde darf nicht leer sein!");

            if (User == null)
                throw new ArgumentNullException($"{nameof(Benutzer)} darf nicht leer sein!");

            if (User.Email == null)
                throw new ArgumentNullException($"{nameof(User.Email)} der eigenen Firma darf nicht leer sein!");

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(conf.EmailServer);
            var path = string.Empty;
            path = RechnungTools.PrintBill(Rechnung, User);
            var ReceiverMail = Rechnung.Kunde.Email;

            mail.From = new MailAddress(User.Email);
            mail.To.Add(ReceiverMail);
            mail.Subject = Head;
            mail.Body = Body;
            mail.Attachments.Add(new Attachment(path));
            SmtpServer.Port = conf.Port;
            SmtpServer.Credentials = new NetworkCredential(conf.UserName, conf.password);

            SmtpServer.Send(mail);
        }

        public static void SendMail(EmailConf conf, Benutzer User, string Head, string Body, Angebot angebot)
        {
            if (conf == null || !conf.IsVaild())
                throw new ArgumentNullException($"keine gültige Email Konfiguration!");

            if (angebot == null)
                throw new ArgumentNullException($"{nameof(Rechnung)} darf nicht leer sein!");

            if (angebot.Kunde == null)
                throw new ArgumentNullException($"{nameof(Rechnung.Kunde)} darf nicht leer sein!");

            if (angebot.Kunde.Email == null)
                throw new ArgumentNullException($"{nameof(Rechnung.Kunde.Email)} von Kunde darf nicht leer sein!");

            if (User == null)
                throw new ArgumentNullException($"{nameof(Benutzer)} darf nicht leer sein!");

            if (User.Email == null)
                throw new ArgumentNullException($"{nameof(User.Email)} der eigenen Firma darf nicht leer sein!");

            var path = OfferTools.PrintOffer(angebot, User);
            var ReceiverMail = angebot.Kunde.Email;

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(conf.EmailServer);
            mail.From = new MailAddress(User.Email);
            mail.To.Add(ReceiverMail);
            mail.Subject = Head;
            mail.Body = Body;
            mail.Attachments.Add(new Attachment(path));
            SmtpServer.Port = conf.Port;
            SmtpServer.Credentials = new NetworkCredential(conf.UserName, conf.password);

            SmtpServer.Send(mail);
        }

    }
}
