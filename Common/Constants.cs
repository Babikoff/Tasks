namespace Common
{
    public class Constants
    {
        /// <summary>
        /// Задержка обработки сообщения при тестировании (msec)
        /// </summary>
#if DEBUG
        public const int TestQueueMessageConsumingDelay = 20000;
#else
        public const int TestQueueMessageConsumingDelay = 0;
#endif


    }
}
