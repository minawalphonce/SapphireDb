﻿using System.Collections.Generic;

namespace RealtimeDatabase.Models.Responses
{
    public class QueryResponse : ResponseBase
    {
        public IEnumerable<object> Collection { get; set; }
    }
}
