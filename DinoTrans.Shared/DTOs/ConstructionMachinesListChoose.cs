using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs
{
    public class ConstructionMachinesListChoose
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string SerialNumber { get; set; }
        public int CompanyShipperId { get; set; }
        public string? Image { get; set; }
        public List<Dictionary<string,string>>? ListImages { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        public string ButtonText { get; set; } = "+ Chọn máy xây dựng";
        public string ButtonStyle { get; set; } = "btn-outline-success";
        public bool IsSelected { get; set; } = false;
        public string ButtonWidth = "260px";
    }
}
