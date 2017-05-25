using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MiniProfiler.Fody.Filters;
using MiniProfiler.Fody.Weavers;

namespace MiniProfiler.Fody.Helpers
{
    public class FodyConfigParser
    {
        private FodyConfigParser()
        {
        }

        private string _error;
        private bool _profilerConstructorsFlag;
        private bool _profilerPropertiesFlag;
        private IEnumerable<XElement> _filterConfigElements;

        public static FodyConfigParser Parse(XElement element)
        {
            var result = new FodyConfigParser();
            result.DoParse(element);

            return result;
        }

        private void DoParse(XElement element)
        {
            try
            {
                _profilerConstructorsFlag = bool.Parse(GetAttributeValueOrDefault(element, "profilerConstructors", bool.FalseString));
                _profilerPropertiesFlag = bool.Parse(GetAttributeValueOrDefault(element, "profilerProperties", bool.TrueString));

                _filterConfigElements = element.Descendants();
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }
        }


        public ProfilerConfiguration Result
        {
            get
            {
                var result = new ProfilerConfiguration(new DefaultProfilerFilter(_filterConfigElements), _profilerConstructorsFlag, _profilerPropertiesFlag);

                return result;
            }
        }

        public bool IsErroneous
        {
            get { return !string.IsNullOrEmpty(_error); }
        }

        public string Error
        {
            get { return _error; }
        }

        private static string GetAttributeValue(XElement element, string attributeName, bool isMandatory)
        {
            var attribute = element.Attribute(attributeName);
            if (isMandatory && (attribute == null || String.IsNullOrWhiteSpace(attribute.Value)))
            {
                throw new ApplicationException(String.Format("Tracer: attribute {0} is missing or empty.", attributeName));
            }

            return attribute != null ? attribute.Value : null;
        }

        private static string GetAttributeValueOrDefault(XElement element, string attributeName, string defaultValue)
        {
            var attribute = element.Attribute(attributeName);
            return attribute != null ? attribute.Value : defaultValue;
        }
    }
}