using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.ContructionMachine
{
    public class SearchConstructionMachineDTO
    {
        public List<Entities.ContructionMachine> contructionMachines { get;set; }
        public int TotalPage { get; set; }
    }
}
