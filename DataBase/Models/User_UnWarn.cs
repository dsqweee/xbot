namespace XBOT.DataBase
{
    public class User_UnWarn
    {
        public ulong Id { get; set; }
        public ulong Warn_Id { get; set; }
        public virtual User_Warn Warn { get; set; }
        public WarnStatus Status { get; set; }
        public ulong? AdminId { get; set; }
        public virtual User_Permission Admin { get; set; }
        public DateTime ReviewAdd { get; set; }
        public DateTime EndStatusSet { get; set; }

        /// <summary>
        /// <para>review - Рассматривается - варн на рассмотрении</para>
        ///<para>UnWarned - Незаслуженный - варн был на рассмотрении и оказался незаслуженным</para>
        ///<para>Rejected - Заслуженный - варн был выдан верно</para>
        ///<para>restart - Если варн нужно снять по везкой причине</para>
        ///<para>error - варн по ошибке</para>
        /// </summary>
        public enum WarnStatus : byte
        {
            review, // Рассматривается - варн на рассмотрении
            UnWarned, // Незаслуженный - варн был на рассмотрении и оказался незаслуженным
            Rejected, // Заслуженный - варн был выдан верно
            restart, // Если варн нужно снять по везкой причине
            error // варн по ошибке
        }
    }
}
