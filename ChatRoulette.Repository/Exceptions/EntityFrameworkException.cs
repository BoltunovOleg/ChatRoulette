using System;

namespace ChatRoulette.Repository.Exceptions
{
    public class EntityFrameworkException : Exception
    {
        public EntityFrameworkException(Exception ex) : base("Возникла критическая ошибка репозитория!", ex) { }
    }
}