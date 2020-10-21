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
        private const string OffTableId = "1KGj48h5y4ZD00mtc5GQZT7ZVY-rFQ22p3mc8-D6q3SI";

        private const string OffTableListMod1V2 = "User perspective V2";
        private const string OffTableListMod24v7 = "  Moderation 24/7";
        private const string SecondTableListMod3v7 = "Unfiltered";

        public static void Report(SessionPreference preference, ChatSession session, int id, TimeSpan idleTimeElapsed, TimeSpan cameraTimeElapsed, TimeSpan talkTimeElapsed, int cameraCount)
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

            IList<IList<Object>> objNeRecords = GenerateData(preference, session, id, idleTimeElapsed, cameraTimeElapsed, talkTimeElapsed, cameraCount);
            if (objNeRecords == null)
                return;

            string spreadsheetId = "";
            string list = "";
            switch (preference.Name)
            {
                case "User Perspective V2":
                    spreadsheetId = OffTableId;
                    list = OffTableListMod1V2;
                    break;
                case "Default v2":
                    spreadsheetId = OffTableId;
                    list = OffTableListMod24v7;
                    break;

                case "Unfiltered v2":
                    spreadsheetId = OffTableId;
                    list = SecondTableListMod3v7;
                    break;
            }

            UpdateGoogleSheetInBatch(objNeRecords, spreadsheetId, $"{list}!A:AA", service);
        }

        private static IList<IList<Object>> GenerateData(SessionPreference preference, ChatSession session, int id, TimeSpan idleTimeElapsed, TimeSpan cameraTimeElapsed, TimeSpan talkTimeElapsed, int cameraCount)
        {
            switch (preference.Name)
            {
                case "Default v2":
                case "User Perspective V2":
                case "Unfiltered v2":
                    return GenerateOffTableData(preference, session, idleTimeElapsed, cameraTimeElapsed,
                        talkTimeElapsed, cameraCount);
            }

            return null;
        }


        private static IList<IList<object>> GenerateOffTableData(SessionPreference preference, ChatSession session, TimeSpan idleTimeElapsed, TimeSpan cameraTimeElapsed, TimeSpan talkTimeElapsed, int cameraCount)
        {
            List<IList<Object>> objNewRecords = new List<IList<Object>>();
            IList<Object> obj = new List<Object>();

            obj.Add(session.DateStart.ToString("dd.MM.yyyy"));
            obj.Add(preference.Mod);
            if (session.DateEnd.HasValue)
                obj.Add($"{session.DateStart:HH}-{session.DateEnd:HH}");
            else
                obj.Add($"{session.DateStart:HH}-{session.DateStart:HH}");

            var inappropriate = session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Inappropriate);
            var hiddenInappropriate = session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.HiddenInappropriate);
            var nobody = session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Nobody);
            var blanket = session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Blanket);
            var cp = session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Cp);
            var male = session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Male);
            var female = session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Female);
            var onePlus = session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.OnePlus);
            var performer = session.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Performer);
            var spam = session.ChatConnections.Count(x =>
                x.Result == ChatConnectionResultEnum.Spam1 ||
                x.Result == ChatConnectionResultEnum.Spam2 ||
                x.Result == ChatConnectionResultEnum.Spam3
            );
            obj.Add(inappropriate);
            obj.Add(hiddenInappropriate);
            obj.Add(nobody);
            obj.Add(blanket);
            obj.Add(cp);
            obj.Add(male);
            obj.Add(female);
            obj.Add(onePlus);
            obj.Add(performer);
            obj.Add(spam);
            obj.Add("session link");
            obj.Add("");
            obj.Add("");

            var connections = inappropriate + hiddenInappropriate + nobody + blanket + cp + male + female + onePlus +
                              performer;
            var lowQualityCount = inappropriate + hiddenInappropriate + nobody + blanket + cp;
            var lowQuality = Convert.ToDouble(lowQualityCount) / Convert.ToDouble(connections);
            var percentExplicit = Convert.ToDouble(inappropriate) / Convert.ToDouble(connections);

            obj.Add(percentExplicit);
            obj.Add(lowQuality);
            obj.Add(connections);

            obj.Add(idleTimeElapsed);
            var avgIdle = TimeSpan.FromMilliseconds(idleTimeElapsed.TotalMilliseconds / (connections - 1));
            obj.Add(avgIdle);

            obj.Add(talkTimeElapsed);
            var avgTalk = TimeSpan.FromMilliseconds(talkTimeElapsed.TotalMilliseconds / connections);
            obj.Add(avgTalk);

            obj.Add(cameraCount);
            obj.Add(cameraTimeElapsed);
            var avgCamera = TimeSpan.FromMilliseconds(cameraTimeElapsed.TotalMilliseconds / cameraCount);
            obj.Add(avgCamera);

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