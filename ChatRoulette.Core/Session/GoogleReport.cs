using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ChatRoulette.Repository.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace ChatRoulette.Core.Session
{
    public class GoogleReport
    {
        private const string MainTableId = "1NWWgJwAYl5FYa0fdPgXReaGFt3T17IHBQpJjDJ1yaY0";
        private const string OffTableId = "1KGj48h5y4ZD00mtc5GQZT7ZVY-rFQ22p3mc8-D6q3SI";
        private const string BannableTableId = "1EhGqbCojIvU6P6gXDGydVAxz5IGBMckqdS9Cja9B9IQ";

        private const string MainTableList = "Data";
        private const string OffTableListMod1 = "User perspective";
        private const string OffTableListMod2 = "Unmoderated perspective";
        private const string OffTableListMod3 = "Post moderation perspective";
        private const string BannableTableList = "Data";

        public static void Report(SessionPreference preference, ChatSession session, int id)
        {
            string[] scopes = { SheetsService.Scope.Spreadsheets };
            var applicationName = "Data Poster";
            UserCredential credential;

            using (var stream =
                new FileStream("client.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.CurrentDirectory;
                credPath = Path.Combine(credPath, "data.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });

            IList<IList<Object>> objNeRecords = GenerateData(preference, session, id);
            if (objNeRecords == null)
                return;

            string spreadsheetId = "";
            string list = "";
            switch (preference.Name)
            {
                case "Default":
                    spreadsheetId = MainTableId;
                    list = MainTableList;
                    break;
                case "User perspective":
                    spreadsheetId = OffTableId;
                    list = OffTableListMod1;
                    break;
                case "Unmoderated perspective":
                    spreadsheetId = OffTableId;
                    list = OffTableListMod2;
                    break;
                case "Post moderation perspective":
                    spreadsheetId = OffTableId;
                    list = OffTableListMod3;
                    break;
                case "Bannable":
                    spreadsheetId = BannableTableId;
                    list = BannableTableList;
                    break;
            }

            UpdateGoogleSheetInBatch(objNeRecords, spreadsheetId, $"{list}!A:AA", service);
        }

        private static IList<IList<Object>> GenerateData(SessionPreference preference, ChatSession session, int id)
        {
            switch (preference.Name)
            {
                case "Default":
                    return GenerateMainTableData(session, id);
                case "User perspective":
                case "Unmoderated perspective":
                case "Post moderation perspective":
                    return GenerateOffTableData(preference, session);
                case "Bannable":
                    return GenerateBannableTableData(session, id);
            }

            return null;
        }

        private static IList<IList<object>> GenerateBannableTableData(ChatSession session, int id)
        {
            List<IList<Object>> objNewRecords = new List<IList<Object>>();
            IList<Object> obj = new List<Object>();

            obj.Add(session.DateStart.ToOADate());
            if (session.DateEnd != null)
                obj.Add(session.DateEnd.Value.ToOADate());
            else
                obj.Add(session.DateStart.AddHours(1).ToOADate());

            obj.Add(session.ChatConnections.Count);
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Inappropriate));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.PartnerDisconnected));
            obj.Add("PC");
            obj.Add(id);
            obj.Add("None");
            obj.Add(session.DateStart.Hour);
            obj.Add("None");

            objNewRecords.Add(obj);
            return objNewRecords;
        }

        private static IList<IList<object>> GenerateOffTableData(SessionPreference preference, ChatSession session)
        {
            List<IList<Object>> objNewRecords = new List<IList<Object>>();
            IList<Object> obj = new List<Object>();

            obj.Add(session.DateStart.ToString("dd.MM.yyyy"));
            obj.Add(preference.Mod);
            if (session.DateEnd.HasValue)
                obj.Add($"{session.DateStart:HH}-{session.DateEnd:HH.mm}");
            else
                obj.Add($"{session.DateStart:HH}-{session.DateStart:HH.mm}");

            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Inappropriate));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.HiddenInappropriate));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Nobody));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Blanket));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Cp));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Male));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Female));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.OnePlus));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Performer));

            objNewRecords.Add(obj);
            return objNewRecords;
        }

        private static IList<IList<Object>> GenerateMainTableData(ChatSession session, int id)
        {
            List<IList<Object>> objNewRecords = new List<IList<Object>>();

            IList<Object> obj = new List<Object>();

            obj.Add(session.DateStart.ToOADate());
            if (session.DateEnd != null)
                obj.Add(session.DateEnd.Value.ToOADate());
            else
                obj.Add(session.DateStart.AddHours(1).ToOADate());
            obj.Add(session.ChatConnections.Count(y => y.Result != ChatConnectionResultEnum.PartnerDisconnected));

            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Inappropriate ||
                                                       x.Result == ChatConnectionResultEnum.HiddenInappropriate));

            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.HiddenInappropriate));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Inappropriate));

            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Error1));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Error2));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Error3));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Error4));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Error5));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Error6));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Error7));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Spam1));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Spam2));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Spam3));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Male));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Female));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.OnePlus));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Nobody));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Age13));
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Age16));
            obj.Add("PC");
            obj.Add(id);
            obj.Add("None");
            obj.Add(session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Spam1 ||
                                                       x.Result == ChatConnectionResultEnum.Spam2 ||
                                                       x.Result == ChatConnectionResultEnum.Spam3));
            obj.Add(session.DateStart.Hour);

            objNewRecords.Add(obj);

            return objNewRecords;
        }

        private static void UpdateGoogleSheetInBatch(IList<IList<Object>> values, string spreadsheetId, string newRange, SheetsService service)
        {
            SpreadsheetsResource.ValuesResource.AppendRequest request =
                service.Spreadsheets.Values.Append(new ValueRange() { Values = values }, spreadsheetId, newRange);
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            request.Execute();
        }
    }
}