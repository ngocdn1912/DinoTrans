using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.SearchDTO
{
    public class SearchModel
    {
        public string? SearchText { get;set; }
        public int pageIndex { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }
}
