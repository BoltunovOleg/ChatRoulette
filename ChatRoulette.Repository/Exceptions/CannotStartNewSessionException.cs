using System;

namespace ChatRoulette.Repository.Exceptions
{
    public class CannotStartNewSessionException : Exception
    {
        public override string Message =>
            $"Начинать новую сессию можно в первые {ChatRepository.AllowedMinute} минут каждого часа.";
    }
}