﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAPR.Models
{
    public class Purchase
    {
        public int IdPurchase { get; set; }
        public List<Field> Fields { get; set; }
        public Rule BeforeRule { get; set; }
        public Rule AfterRule { get; set; }

    }
}