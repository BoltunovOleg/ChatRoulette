using System.ComponentModel;

namespace ChatRoulette.Repository.Model
{
    public enum ChatConnectionResultEnum
    {
        [Description("Мужчина")] Male = 0,
        [Description("Женщина")] Female = 1,
        [Description("Более 1")] OnePlus = 2,
        [Description("Никого")] Nobody = 3,
        [Description("13 <")] Age13 = 4,
        [Description("16 <")] Age16 = 5,

        [Description("Надписи")] Text = 6,

        [Description("Косячно забанили")] FakeBan = 7,

        [Description("Дрочер")] Inappropriate = 8,
        [Description("Скрытый дрочер")] HiddenInappropriate = 9,

        [Description("Search cancelled.")] Error1 = 10,

        [Description("Please try again later. could not connect to parther.")]
        Error2 = 11,

        [Description(
            "The stream was unable to connect due to a network error. Make sure your connection isn't blocked by a firewall.")]
        Error3 = 12,

        [Description("Sorry, you cannot use the site right now. Please try again later.")]
        Error4 = 13,
        [Description("Browser reset.")] Error5 = 14,
        [Description("Browser crash.")] Error6 = 15,
        [Description("Face ID error.")] Error7 = 16,
        [Description("Sexykiss.org")] Spam1 = 17,
        [Description("CINDY.HOT23")] Spam2 = 18,
        [Description("Chatzik")] Spam3 = 19,
        [Description("Parnter disconnected")] PartnerDisconnected = 20,


        [Description("Children Porn")] Cp = 21,
        [Description("Blanket")] Blanket = 22,
        [Description("Guitar / painter etc")] Performer = 23,
        [Description("Anyone")] Anyone = 24
    }
}