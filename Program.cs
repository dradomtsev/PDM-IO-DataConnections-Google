using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using GFile = Google.Apis.Drive.v3.Data.File;

namespace Archimatika.IO.Google.Sheets
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "Drive API .NET Quickstart";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (FileStream stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });



            string sFileID = "1R9wI7T2s7V5vz749x8KwyqQIGvJnpLpOkyzzjzjQeww";
            int iPageSize = 1000;
            string sFields = "*";

            FilesResource.GetRequest fFileReq = service.Files.Get(sFileID);
            fFileReq.Fields = "*";
            GFile gfResult = fFileReq.Execute();

            if (gfResult.Capabilities.CanReadRevisions == false)
                gfResult.Capabilities.CanReadRevisions = true;

            // 1_QfxWkF_NdvWPZALZONKzyzmHLkw71PkoCXfkFEtgqo - 1ATeam
            // 11W9OPD5ABoOa7ODHSmoWHxVCVNHyfWc14Qxk4gaeO5o - NotAdminLogins

            List<Revision> lAllrevisions = new List<Revision>();
            RevisionList rlResult = null;

            //var Result = GetRevisionsAsync(rlResult, service, sFileID, iPageSize, sFields, lAllrevisions);
            //rlResult = Result.Result.Item1;
            //lAllrevisions = Result.Result.Item2;
            var Result = GetRevisionsSync(rlResult, service, sFileID, iPageSize, sFields, lAllrevisions);
            rlResult = Result.Item1;
            lAllrevisions = Result.Item2;

            Console.WriteLine("Revisions:\n");
            int iCounter = 0;
            if (lAllrevisions != null && lAllrevisions.Count > 0)
                foreach (var rev in lAllrevisions)
                {
                    Console.WriteLine("{0} {1} {2} {3}", iCounter++, rev.Id, rev.ModifiedTime, rev.LastModifyingUser.DisplayName);
                }
            Console.Read();

            Console.WriteLine("Paste Revision ID that You want to download: ");
            string sRevID = Console.ReadLine();

            RevisionsResource.GetRequest gRevisionReq = service.Revisions.Get(sFileID, sRevID);
            gRevisionReq.Alt = DriveBaseServiceRequest<Revision>.AltEnum.Json;

        }

        static Tuple<RevisionList, List<Revision>> GetRevisionsSync(RevisionList rlResult, DriveService service, string sFileID, int iPageSize, string sFields, List<Revision> lAllrevisions)
        {
            Console.WriteLine("Start fetching revisions...\n");
            while (true)
            {
                if (rlResult != null && string.IsNullOrWhiteSpace(rlResult.NextPageToken))
                    break;
                RevisionsResource.ListRequest listRequest = service.Revisions.List(sFileID);
                listRequest.PageSize = iPageSize;
                listRequest.Fields = sFields;

                if (rlResult != null)
                    listRequest.PageToken = rlResult.NextPageToken;

                rlResult = listRequest.Execute();
                lAllrevisions.AddRange(rlResult.Revisions);
            }
            return Tuple.Create(rlResult, lAllrevisions);
        }

        static async Task<Tuple<RevisionList, List<Revision>>> GetRevisionsAsync(RevisionList rlResult, DriveService service, string sFileID, int iPageSize, string sFields, List<Revision> lAllrevisions)
        {
            Console.WriteLine("Start fetching revisions...\n");
            while (true)
            {
                if (rlResult != null && string.IsNullOrWhiteSpace(rlResult.NextPageToken))
                    break;
                RevisionsResource.ListRequest listRequest = service.Revisions.List(sFileID);
                listRequest.PageSize = iPageSize;
                listRequest.Fields = sFields;

                if (rlResult != null)
                    listRequest.PageToken = rlResult.NextPageToken;

                rlResult = await listRequest.ExecuteAsync();
                lAllrevisions.AddRange(rlResult.Revisions);
            }
            return Tuple.Create(rlResult, lAllrevisions);
        }
    }
}