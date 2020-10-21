namespace ChatRoulette.Core.Session
{
    public class ChatRouletteStatusParser
    {
            public const string CameraAndMicrophoneAccess = "You need to give access to your camera and microphone";
            public const string EnablingTheCamera = "Enabling the camera...";
            public const string WelcomeToChatRoulette = "Welcome to Chatroulette - Press '▶' to Start Chatting :)";
            public const string ConnectingToPartner = "Connecting to partner...";
            public const string YouDisconnected = "You disconnected. Next partner coming up...";
            public const string PartnerDisconnected = "Partner disconnected";
            public const string Searching = "Searching";

            public const string SmileAtTheCamera = "Smile at the camera :)";

            public const string SearchCancelled = "Search cancelled"; // error 1
            public const string CouldNotConnectToPartner = "Could not connect to partner."; // error 2

            public const string VideoStreamWasUnableToConnectFirewall =
                "The video stream was unable to connect due to a network error. Make sure your connection isn't blocked by a firewall."; // error 3

            public const string
                TryAgainLater = "Sorry, you cannot use the site right now. Please try again later."; // error 4


            // error 7
            public const string RejectedVideoQuality =
                "Search rejected because of a video quality. Check exposure and if your face is visible"; 
            public const string RejectedNoFace = "Search rejected because no face was found. Please try again.";
            public const string RejectedUnderageFace = "Search rejected because we detected a possible underage face.";
            public const string CouldNotDetectFace = "Could not detect your face, please try again in a moment.";
            public const string WeCouldNotDetectYourFace = "We could not detect your face, please try again in a moment.";


            public const string Ban = "Suspicious activity has been detected on your feed. We have shut it down. You may try again later, but please be aware that adult content is not permitted on the site. If you believe that the feed has been wrongly identified as adult content, we apologise and please bear with us while the system is being refined.";

        //public static Status Parse(string status)
        //{
        //    switch (status)
        //    {
        //        case CameraAndMicrophoneAccess:
        //        case EnablingTheCamera:
        //            return Status.EnableCamera;

        //        case WelcomeToChatRoulette:
        //        case SmileAtTheCamera:
        //            return Status.Start;

        //        case ConnectingToPartner:
        //        case Searching:
        //            return Status.Wait;

        //        case YouDisconnected:
        //            return Status.PutResult;

        //        case PartnerDisconnected:
        //            return Status.PartnerDisconnected;

        //        case Ban:
        //            return Status.Ban;

        //        case SearchCancelled:
        //            return Status.Error1;
        //        case CouldNotConnectToPartner:
        //            return Status.Error2;
        //        case VideoStreamWasUnableToConnectFirewall:
        //            return Status.Error3;
        //        case TryAgainLater:
        //            return Status.Error4;
        //        case RejectedVideoQuality:
        //        case RejectedNoFace:
        //        case RejectedUnderageFace:
        //        case CouldNotDetectFace:
        //        case WeCouldNotDetectYourFace:
        //            return Status.Error7;

        //        default:
        //            return Status.PartnerConnected;
        //    }
        //}
    }

    public enum Status
    {
        EnableCamera,
        Start,
        Wait,
        PartnerDisconnected,
        PartnerConnected,
        PutResult
    }
}