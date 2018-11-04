﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Model
{
    public class MangaItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Resume { get; set; }
        public string Cover { get; set; }
        public string State { get; set; }
        public string Tags { get; set; }
        public string ChapterCount { get; set; }
    }
}
