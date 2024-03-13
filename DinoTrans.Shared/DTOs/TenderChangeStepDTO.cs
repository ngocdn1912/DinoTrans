using DinoTrans.Shared.Contracts;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs
{
    public class TenderChangeStepDTO
    {
        public int CurrentStep { get; set; }
        public Tender Tender { get; set; }
        public List<int> ConstructionMachineIds { get; set; }
    }

    public class TenderBackStep2DTO
    {
        public int CurrentStep { get; set; }
        public ConvertStep2 InputStep2 { get; set; }
        public List<int> ConstructionMachineIds { get; set; }
        public bool IsReturnFromStep3 { get; set; } = false;
    }
}
