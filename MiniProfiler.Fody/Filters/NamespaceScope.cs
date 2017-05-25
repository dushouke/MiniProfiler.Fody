using System;
using System.Linq;

namespace MiniProfiler.Fody.Filters {
    internal class NamespaceScope : IComparable<NamespaceScope> {
        public static NamespaceScope All = new NamespaceScope(string.Empty, MatchMode.SelfAndChildren); //empty namespace means all match

        private enum MatchMode {
            ExactMatch,
            OnlyChildren,
            SelfAndChildren
        }


        private readonly string _namespace;
        private readonly MatchMode _matchMode;

        private NamespaceScope(string ns, MatchMode matchMode) {
            _namespace = ns;
            _matchMode = matchMode;
        }

        public static NamespaceScope Parse(string inp) {
            string namespc;
            var matchMode = MatchMode.ExactMatch;
            if (inp.EndsWith(".*")) {
                matchMode = MatchMode.OnlyChildren;
                namespc = inp.Substring(0, inp.Length - 2);
            }
            else if (inp.EndsWith("+*")) {
                matchMode = MatchMode.SelfAndChildren;
                namespc = inp.Substring(0, inp.Length - 2);
            }
            else {
                if (inp.EndsWith(".") || inp.EndsWith("*")) {
                    throw new ApplicationException("Namespace must end with a literal or with .* or +* symbols.");
                }
                if (inp.Contains('*')) {
                    throw new ApplicationException("Namespace can only contain an asterisk at the end.");
                }
                namespc = inp;
            }

            if (string.IsNullOrWhiteSpace(namespc)) {
                throw new ApplicationException("Namespace cannot be empty if specified.");
            }

            return new NamespaceScope(namespc, matchMode);
        }

        public int CompareTo(NamespaceScope other) {
            //for ordering from more specific to least specific
            var dotCnt = _namespace.Count(chr => chr == '.');
            var otherDotCnt = other._namespace.Count(chr => chr == '.');

            if (dotCnt != otherDotCnt) {
                return Math.Sign(otherDotCnt - dotCnt);
            }

            //match mode order is exact, children, selfandchildren
            return _matchMode.CompareTo(other._matchMode);
        }

        public override string ToString() {
            return string.Format("{0}{1}", _namespace, _matchMode == MatchMode.OnlyChildren ? ".*" : (_matchMode == MatchMode.SelfAndChildren ? "+*" : ""));
        }

        public bool IsMatching(string ns) {
            //check for ALL
            if (string.IsNullOrEmpty(_namespace)) return true;

            switch (_matchMode) {
                case MatchMode.ExactMatch: {
                    return string.Equals(_namespace, ns, StringComparison.OrdinalIgnoreCase);
                }
                case MatchMode.OnlyChildren: {
                    if (!ns.StartsWith(_namespace, StringComparison.OrdinalIgnoreCase)) return false;
                    if (ns.Length == _namespace.Length) return false;
                    return ns[_namespace.Length] == '.';
                }
                case MatchMode.SelfAndChildren: {
                    if (!ns.StartsWith(_namespace, StringComparison.OrdinalIgnoreCase)) return false;
                    if (ns.Length == _namespace.Length) return true;
                    return ns[_namespace.Length] == '.';
                }
            }
            throw new ApplicationException("Unknown match mode");
        }
    }
}