using DinoTrans.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.SearchDTO
{
    public class SearchTenderActiveDTO:SearchModel
    {
        public SearchActiveByMachines searchLoads { get; set; }
        public SearchActiveByOffers searchOffers { get; set; }
    }
}
