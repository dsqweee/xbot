namespace XBOT.DataBase.Models
{
    public class QiwiTransactions
    {
        public ulong Id { get; set; }
        public ulong discord_id { get; set; }
        public double invoice_ammount { get; set; }
        public DateTime invoice_date_add { get; set; }
    }
}
