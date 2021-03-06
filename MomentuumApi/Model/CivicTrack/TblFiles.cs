﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MomentuumApi.Model.CivicTrack
{
    public class TblFiles
    {
        public int? Id { get; set; }
        public string Riding { get; set; }
        public string CaseItemId { get; set; }
        public string UserId  { get; set; }
        public string FileName { get; set; }
        public string TimeProcess { get; set; }
        public string Deleted { get; set; }
        public string Comments { get; set; }
        public string VoterId { get; set; }
    }
}
