using System;
using System.Collections.Generic;
using Shared.Common.Logic;

namespace TestFramework.TestHelpers
{
    public static class ExpectedTestResultConst
    {
        public static readonly string DontCare = "Don't Care";

        public static readonly string Hentet = "Hentet";
        public static readonly string IkkeHentet = "Ikke Hentet";

        public static readonly string Opprettet = "Opprettet";
        public static readonly string IkkeOpprettet = "Ikke Opprettet";

        public static readonly string Funnet = "Funnet";
        public static readonly string IkkeFunnet = "Ikke Funnet";

        public static readonly string Oppdatert = "Oppdatert";
        public static readonly string IkkeOppdatert = "Ikke Oppdatert";

        public static readonly string Pass = "Pass";
        public static readonly string Fail = "Fail";

        public static readonly string Slettet = "Slettet";
        public static readonly string IkkeSlettet = "Ikke Slettet";

        public static bool ExpectsHentet(this string expected, bool? @default = null)
        {
            var map = new Dictionary<string, bool>
            {
                { Hentet, true },
                { IkkeHentet, false }
            };
            return Get(expected, map, @default);
        }

        public static bool ExpectsOpprettet(this string expected, bool? @default = null)
        {
            var map = new Dictionary<string, bool>
            {
                { Opprettet, true },
                { IkkeOpprettet, false }
            };
            return Get(expected, map, @default);
        }

        public static bool ExpectsFunnet(this string expected, bool? @default = null)
        {
            var map = new Dictionary<string, bool>
            {
                { Funnet, true },
                { IkkeFunnet, false }
            };
            return Get(expected, map, @default);
        }

        public static bool ExpectsOppdatert(this string expected, bool? @default = null)
        {
            var map = new Dictionary<string, bool>
            {
                { Oppdatert, true },
                { IkkeOppdatert, false }
            };
            return Get(expected, map, @default);
        }

        public static bool ExpectsPass(this string expected, bool? @default = null)
        {
            var map = new Dictionary<string, bool>
            {
                { Pass, true },
                { Fail, false }
            };
            return Get(expected, map, @default);
        }

        public static bool ExpectsSlettet(this string expected, bool? @default = null)
        {
            var map = new Dictionary<string, bool>
            {
                { Slettet, true },
                { IkkeSlettet, false }
            };
            return Get(expected, map, @default);
        }

        public static bool ExpectsRegistrert(this string expected, bool? @default = null)
        {
            var map = new Dictionary<string, bool>
            {
                { "Registrert", true },
                { "Ikke Registrert", false },
            };
            return Get(expected, map, @default);
        }

        private static bool Get(string input, Dictionary<string, bool> map, bool? @default)
        {
            if (@default.HasValue && string.IsNullOrWhiteSpace(input))
            {
                return @default.Value;
            }

            if (!map.ContainsKey(input))
            {
                throw new ArgumentException($"The expected result was not valid. Was '{input}', but should have been one of {map.Keys.ToJson()}");
            }

            return map[input];
        }
    }
}
