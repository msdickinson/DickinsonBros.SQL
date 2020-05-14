using System;
using System.Collections.Generic;
using System.Text;

namespace DickinsonBros.SQL.Runner.Services.AccountDB.Models
{
    public class QueueDTO
    {
        public int QueueId { get; set; }
        public string Payload { get; set; }
    }
}
