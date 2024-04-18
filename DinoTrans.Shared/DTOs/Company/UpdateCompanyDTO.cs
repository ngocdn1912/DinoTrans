namespace DinoTrans.Shared.DTOs.Company
{
    public class UpdateCompanyDTO
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public float? ShipperFeePercentage { get; set; }
        public float? CarrierFeePercentage { get; set; }
        public bool? IsActive { get; set; }
    }
}
