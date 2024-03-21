using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace DinoTrans.Shared.Entities
{
    public class ContructionMachine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string SerialNumber { get;set; }
        public int CompanyShipperId { get; set; }
        public string? Image {  get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        public bool IsDeleted { get; set; }
        [NotMapped]
        public List<Dictionary<string,string>> ImageDeserializeJson
        {
            get
            {
                if (!string.IsNullOrEmpty(Image))
                {
                    return JsonConvert.DeserializeObject<List<Dictionary<string,string>>>(Image);
                }

                return new List<Dictionary<string, string>>();
            }
            set => Image = JsonConvert.SerializeObject(value);
        }
        [ForeignKey("CompanyShipperId")]
        public virtual Company? CompanyShipper { get; set; }
    }
}
