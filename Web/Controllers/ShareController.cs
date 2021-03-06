﻿using Data;
using Data.Enums;
using Data.Model;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class ShareController : ApiController
    {
        private readonly MicscanContext micscanContext = new MicscanContext();

        //public ShareController(MicscanContext micscanContext)
        //{
        //    this.micscanContext = micscanContext;
        //}

        // GET: api/Share
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Share/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Share
        [ValidateInput(false)]
        public async Task Post()
        {
            HttpContext httpContext = HttpContext.Current;
            SharedVulnerability sharedVulnerability = new SharedVulnerability();
            sharedVulnerability.Comment = httpContext.Request.Form["Comment"];
            sharedVulnerability.CommitMessage = httpContext.Request.Form["CommitMessage"];
            sharedVulnerability.CweId = int.TryParse(httpContext.Request.Form["CweId"], out int cweId) ? cweId : (int?)null;
            bool fixedWithCommit = bool.Parse(httpContext.Request.Form["FixedWithCommit"]);

            foreach (string key in httpContext.Request.Files.AllKeys)
            {
                int fileId = ParseFileId(key);
                SharedFile sharedFile = new SharedFile();
                HttpPostedFile file = httpContext.Request.Files[key];
                sharedFile.Name = file.FileName;
                int changeKind = int.Parse(httpContext.Request.Form[$"File{fileId}ChangeKind"]);
                sharedFile.ChangeKind = (ChangeKind)changeKind;
                using (BinaryReader binaryReader = new BinaryReader(file.InputStream))
                {
                    sharedFile.Content = binaryReader.ReadBytes(file.ContentLength);
                }
                if (fixedWithCommit)
                {
                    sharedFile.VulnerabilityState = key.StartsWith("CurrentFile") ? VulnerabilityState.AfterFix : VulnerabilityState.Vulnerable;
                }
                else
                {
                    sharedFile.VulnerabilityState = key.StartsWith("CurrentFile") ? VulnerabilityState.Vulnerable : VulnerabilityState.BeforeIntroduction;
                }
                sharedVulnerability.SharedFiles.Add(sharedFile);
            }

            micscanContext.SharedVulnerabilities.Add(sharedVulnerability);
            await micscanContext.SaveChangesAsync();
        }

        private int ParseFileId(string fileKey)
        {
            return int.Parse(fileKey.Replace("CurrentFile", "").Replace("PreviousFile", ""));
        }

        // PUT: api/Share/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Share/5
        public void Delete(int id)
        {
        }
    }
}
