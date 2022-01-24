﻿using System.Collections.Generic;

namespace WebfrontCore.ViewModels
{
   
    public class ScoreboardInfo
    {
       public string ServerName { get; set; }
       public long ServerId { get; set; }
       public string MapName { get; set; }
       public string OrderByKey { get; set; }
       public bool ShouldOrderDescending { get; set; }
       public List<ClientScoreboardInfo> ClientInfo { get; set; }
    }
    
    public class ClientScoreboardInfo
    {
        public string ClientName { get; set; }
        public long ClientId { get; set; }
        public int Score { get; set; }
        public int Ping { get; set; }
        public int? Kills { get; set; }
        public int? Deaths { get; set; }
        public double? ScorePerMinute { get; set; }
        public double? Kdr { get; set; }
        public double? ZScore { get; set; }
    }
}
