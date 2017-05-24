using System.Collections.Generic;
using System.Xml.Linq;
using Mono.Cecil;

namespace MiniProfiler.Fody.Weavers
{
    internal class DefaultProfilerFilter: IProfilerFilter
    {
        private readonly IEnumerable<XElement> _filterConfigElements;

        public DefaultProfilerFilter(IEnumerable<XElement> filterConfigElements)
        {
            _filterConfigElements = filterConfigElements;
        }

        public bool ShouldAddProfiler(MethodDefinition definition)
        {
            //TODO
            return true;
        }
    }
}