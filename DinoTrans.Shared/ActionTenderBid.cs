using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared
{
    public class ActionTenderBid
    {
        public TenderBid TenderBid { get; set; }
        public TenderActionType ActionType { get; set; }
    }

    public enum TenderActionType
    {
        Add,
        Update,
        Delete
    }
}
