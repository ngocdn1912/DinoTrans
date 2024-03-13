using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.ContructionMachine
{
    public class CreateContructionMachineDTO
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public string SerialNumber { get; set; }
        public int CompanyShipperId { get; set; }
        public List<string>? Image { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
    }
}
