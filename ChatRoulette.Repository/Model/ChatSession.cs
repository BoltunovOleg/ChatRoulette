using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ChatRoulette.Repository.Model
{
    public class ChatSession
    {
        [Key]
        public int Id { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? DateClosed { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public bool IsShared { get; set; }
        public int UserNumber { get; set; }
        public virtual List<ChatConnection> ChatConnections { get; set; }

        public override string ToString()
        {
            var splitter = "\t";
            var connections = "";
            if (this.ChatConnections != null)
            {
                connections = $"{splitter}|{splitter}подключений: {this.ChatConnections.Count}";
                var male =
                    $"{splitter}|{splitter}мужчин: {this.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Male)}";
                var female =
                    $"{splitter}|{splitter}женщин: {this.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Female)}";
                var inappropriate =
                    $"{splitter}|{splitter}непотребов: {this.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.Inappropriate)}";
                var hiddenInappropriate =
                    $"{splitter}|{splitter}скрытх непотребов: {this.ChatConnections.Count(x => x.Result == ChatConnectionResultEnum.HiddenInappropriate)}";

                connections += male;
                connections += female;
                connections += inappropriate;
                connections += hiddenInappropriate;
            }

            var result = "";
            if (this.DateStart.Date == this.DateEnd?.Date)
                result = $"{this.DateStart:dd.MM.yyy}{splitter}|{splitter}{this.DateStart:HH} - {this.DateEnd:HH}";
            else
                if (!this.DateEnd.HasValue)
                    result =$"{this.DateStart:dd.MM.yyy}{splitter}|{splitter}{this.DateStart:HH} - 00";
                else
                    result = "none";

            return result + connections;
        }
    }
}