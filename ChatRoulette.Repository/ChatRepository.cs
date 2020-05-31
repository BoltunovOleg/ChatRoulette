using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ChatRoulette.Repository.Exceptions;
using ChatRoulette.Repository.Model;

namespace ChatRoulette.Repository
{
    public class ChatRepository : DbContext
    {
        public const int AllowedMinute = 15;
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatConnection> ChatConnections { get; set; }

        public ChatRepository() : base("MySqlConnection")
        {
        }

        public async Task<List<ChatSession>> GetAllSessions()
        {
            return await this.ChatSessions.ToListAsync();
        }

        public async Task<List<ChatSession>> GetUserSessions(int id)
        {
            return await this.ChatSessions.Where(x => x.UserNumber == id).Include(x=> x.ChatConnections).ToListAsync();
        }

        public async Task<ChatSession> GetSession(int id)
        {
            return await this.ChatSessions.FindAsync(id);
        }

        /// <summary>
        /// Get current opened session or create new
        /// </summary>
        /// <exception cref="CannotStartNewSessionException">Throws when can't create session now</exception>
        /// <exception cref="EntityFrameworkException">Throws when can't find or create chat session</exception>
        /// <returns>Current chat session</returns>^
        public async Task<ChatSession> GetOrCreateSession(int userNumber)
        {
            var dt = DateTime.Now;
            var dateStart = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
            ChatSession session;
            try
            {
                session = await this.ChatSessions
                    .Include(x => x.ChatConnections)
                    .FirstOrDefaultAsync(x => x.UserNumber == userNumber &&
                                              x.DateCreated.Year == dateStart.Year &&
                                              x.DateCreated.Month == dateStart.Month &&
                                              x.DateCreated.Day == dateStart.Day &&
                                              x.DateCreated.Hour == dateStart.Hour);
            }
            catch (Exception ex)
            {
                throw new EntityFrameworkException(ex);
            }

            if (session != null)
                return session;
            var convertedDateStart = TimeZoneInfo.ConvertTime(dateStart,
                TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
            if (dt.Minute > AllowedMinute)
                throw new CannotStartNewSessionException();
            session = new ChatSession
            {
                ChatConnections = new List<ChatConnection>(),
                DateCreated = dt,
                DateStart = convertedDateStart,
                DateEnd = null,
                DateClosed = null,
                IsShared = false,
                UserNumber = userNumber
            };
            try
            {
                this.ChatSessions.Add(session);
                await this.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new EntityFrameworkException(ex);
            }

            return session;
        }

        /// <summary>
        /// Close session
        /// </summary>
        /// <param name="session">Session</param>
        /// <returns>Session</returns>
        /// <exception cref="EntityFrameworkException"></exception>
        public async Task<ChatSession> CloseSessionAsync(ChatSession session)
        {
            int n = 1;
            if (session.DateStart.Hour >= 23)
                n = 0;
            var convertedDateStart = new DateTime(session.DateStart.Year, session.DateStart.Month,
                session.DateStart.Day,
                session.DateStart.Hour + n, 0, 0);
            try
            {
                session.DateClosed = DateTime.Now;
                session.IsShared = true;
                session.DateEnd = convertedDateStart;
                await this.SaveChangesAsync();
                return session;
            }
            catch (Exception ex)
            {
                throw new EntityFrameworkException(ex);
            }
        }

        /// <summary>
        /// Share session
        /// </summary>
        /// <param name="session">Session</param>
        /// <returns>Session</returns>
        /// <exception cref="EntityFrameworkException"></exception>
        public async Task<ChatSession> ShareSessionCompleteAsync(ChatSession session)
        {
            try
            {
                session.IsShared = true;
                await this.SaveChangesAsync();
                return session;
            }
            catch (Exception ex)
            {
                throw new EntityFrameworkException(ex);
            }
        }

        /// <summary>
        /// Add connection result to session
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="chatConnectionResultEnum">Result</param>
        /// <param name="externalId">ChatRoulette id</param>
        /// <returns>Chat connection object</returns>
        /// <exception cref="EntityFrameworkException"></exception>
        public async Task<ChatConnection> AddResultAsync(ChatSession session,
            ChatConnectionResultEnum chatConnectionResultEnum, string externalId)
        {
            try
            {
                var result = new ChatConnection
                {
                    DateCreated = DateTime.Now,
                    Result = chatConnectionResultEnum,
                    Session = session,
                    ExternalId = externalId
                };
                session.ChatConnections.Add(result);
                await this.SaveChangesAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new EntityFrameworkException(ex);
            }
        }
    }
}