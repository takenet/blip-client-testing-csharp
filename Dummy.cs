using Lime.Protocol;
using System;

namespace Take.Blip.Client.Testing
{
    public static class Dummy
    {
        private static readonly Random _rnd;

        static Dummy()
        {
            _rnd = new Random();
        }

        public static Node MessengerUser(string id = null)
        {
            id = id ?? _rnd.Next(10000000, 50000000).ToString();
            return Node.Parse($"{id}@messenger.gw.msging.net");
        }

        public static Node SmsUser(string id = null)
        {
            id = id ?? $"55309{_rnd.Next(100000000).ToString()}";
            return Node.Parse($"{id}@take.io");
        }
    }
}
