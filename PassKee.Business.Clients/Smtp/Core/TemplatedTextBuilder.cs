using System.Text;

namespace PassKee.Business.Clients.Smtp.Core
{
    public class TemplatedTextBuilder
    {
        /// <summary>
        /// Helps building TemplatedText like Email Body or Email Subject. Replaces placeholders starting from the END of the template. 
        /// This is b/c user may accidently enter placeholder-identical strings, and we do NOT want to replace 
        /// user strings b/c they are not from template but from user data. 
        /// </summary>
        ///

        private string _template = null;
        private StringBuilder _message = null;
        private Dictionary<int, KeyValuePair<string, string>> _matches = null;

        public string Text;

        public TemplatedTextBuilder(string templateString): this(templateString, 16384)
        {
        }   
        
        public TemplatedTextBuilder(string templateString, int defaultCapacity)
        {
            _template = templateString;
            _message = new StringBuilder(_template, Math.Max(_template.Length, defaultCapacity)); 
            _matches = new Dictionary<int, KeyValuePair<string, string>>(16);
        }

        public void AddPlaceholder(string key, string value)
        {
            key = "{" + key + "}";
            int i = _template.Length - 1;
            int cnt = 0;
            // just a stupid protection from bugs leading to the endless loop - no more than 10 iterations, 
            // assume that same key cannot be > 100 times in a template. Hmm...
            // I've seen {host} at least 5 times in our "_EmailLayout.htm" file ... Anyways, 100 should be enough
            while (i >= 0 && cnt < 100)
            {
                cnt++;
                i = _template.LastIndexOf(key, i, StringComparison.CurrentCultureIgnoreCase); // search for KEY aka PLACEHOLDER
                if (i >= 0)
                {
                    // yes we have a new match!
                    var m = new KeyValuePair<string, string>(key, value);
                    if (_matches.ContainsKey(i))
                        // for now, let's throw. Feel free to remove this throw, or have another dictionary of already searched placeholder keys
                        throw new ApplicationException($"The same placeholder '{key}' cannot be processed twice! We are finding the same matches twice! Please correct your calling code");

                    _matches.Add(i, m);
                    // do we need to decrement i here???
                }
            }
        }

        public string Build()
        {

            // this is the one that actually modifies the StringBuilder replacing the placeholders with user values

            var sortedKeys = _matches.Keys.ToList();
            sortedKeys.Sort();
            sortedKeys.Reverse(); // indexes in the template string sorted in DESC order, i.e. starting from the END of template string

            foreach (int i in sortedKeys)
            {
                var match = _matches[i];
                //_message.Replace(match.Key, match.Value, i, match.Key.Length); << this works but it is CASE-SENSITIVE so I am going to use .Remove.Insert combination below

                if (string.Compare(match.Key, _message.ToString(i, match.Key.Length), true) != 0)
                {
                    // ERROR! We have a bug somewhere, b/c we do not have a placeholder where we expected it !!!
                    throw new ApplicationException($"Error - placeholder not found where expected! Position index: {i}, Expected placeholder: '{match.Key}', found text: '{_message.ToString(i, match.Key.Length)}'");
                }

                _message.Remove(i, match.Key.Length).Insert(i, match.Value); // this is case-insensitive replace 
            }

            Text = _message.ToString();
            return Text;
        }
    }
}
