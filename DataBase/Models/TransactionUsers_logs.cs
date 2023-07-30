namespace XBOT.DataBase.Models
{
    public class TransactionUsers_Logs // Сделать систему что если у пользователя мало сообщений за день, или же маленький уровень, то переводы ему будут недоступны
    {
        public ulong Id { get; set; }
        public ulong Amount { get; set; }
        public DateTime TimeTransaction { get; set; }
        public ulong? SenderId { get; set; }
        public User Sender { get; set; }
        public ulong RecipientId { get; set; }
        public User Recipient { get; set; }
        public TypeTransation Type { get; set; }
        public enum TypeTransation : byte
        {
            Transfer,
            Reputation,
            daily,
            Kazino
        }
    }
}
