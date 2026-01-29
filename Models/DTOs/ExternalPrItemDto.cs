namespace NCRManagementSystem.Models.DTOs
{
    public class ExternalPrItemDto
    {
        public string Banfn { get; set; } = string.Empty; // PR Number
        public string Bnfpo { get; set; } = string.Empty; // Item Number
        public string Txz01 { get; set; } = string.Empty; // Material Text/Description
        public string Matnr { get; set; } = string.Empty; // Material Number
        public decimal Menge { get; set; } // Quantity
        public string Meins { get; set; } = string.Empty; // Unit
        public string Werks { get; set; } = string.Empty; // Plant
    }
}
