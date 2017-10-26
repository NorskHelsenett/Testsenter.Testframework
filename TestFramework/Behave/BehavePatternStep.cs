using Shared.Common.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TestFramework.Core;
using TestFramework.Interfaces;
using TestFramework.Resources;

namespace TestFramework.Behave
{
    [AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class BehavePatternStep : Attribute
    {
        public int Id { get; set; }
        public string Pattern { get; set; }
        public BehavePatternStep(string pattern) 
        {
            Pattern = pattern;
        }

        public BehavePatternStep(string pattern, int id)
        {
            Pattern = pattern;
            Id = id;
        }

        public bool Match(string text)
        {
            var re = new Regex(Pattern, RegexOptions.None);
            return re.IsMatch(text);
        }

        public List<string> GetMatchItems(string text)
        {
            var items = new List<string>();
            var re = new Regex(Pattern, RegexOptions.None);
            var match = re.Match(text);

            foreach (var group in match.Groups)
            {
                items.Add(group.ToString());
            }

            return items;
        }

        public static IExecuteTestStep FindStep(ITestInjector injector, TestStep testStep, IEnumerable<Assembly> assemblies)
        {
            int behaveStepId = 0;

            var matchString = testStep.Description.StartsWith("GUI - ")
                ? testStep.Description.Substring("GUI - ".Length) 
                : testStep.Description;

            foreach(var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(x => x.IsClass && TypeHasMatchingAttribute(x, matchString, ref behaveStepId));
                var type = types.FirstOrDefault();

                if (type == null)
                    continue;

                var instance = Activator.CreateInstance(type, injector);
                if (instance is BaseExecuteTestStep)
                    ((BaseExecuteTestStep)instance).BehaveStepId = behaveStepId;

                return (IExecuteTestStep)instance;
            }

            throw new NotImplementedException($"Can not find step '{matchString}'");
        }

        public static Dictionary<string, List<BehavePatternStep>> ListSteps(IEnumerable<Assembly> assemblies)
        {
            var dictionary = new Dictionary<string, List<BehavePatternStep>>();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(type => type.IsClass && TypeIsBehavePatternStep(type));

                foreach (var type in types)
                {
                    if (!dictionary.ContainsKey(type.Namespace))
                    {
                        dictionary[type.Namespace] = new List<BehavePatternStep>();
                    }

                    foreach (BehavePatternStep cAttribute in GetCustomAttributes(type, typeof(BehavePatternStep)))
                    {
                        dictionary[type.Namespace].Add(cAttribute);
                    }
                }
            }
            return dictionary;
        }

        private static bool TypeIsBehavePatternStep(Type type)
        {
            var attr = GetCustomAttributes(type, typeof(BehavePatternStep));
            return attr.Length > 0;
        }

        private static bool TypeHasMatchingAttribute(MemberInfo x, string match, ref int behaveStepId)
        {
            foreach(BehavePatternStep attribute in GetCustomAttributes(x, typeof(BehavePatternStep)))
            {
                if (attribute != null && attribute.Match(match))
                {
                    behaveStepId = attribute.Id;
                    return true;
                }
            }

            return false;
        }
    }
}
