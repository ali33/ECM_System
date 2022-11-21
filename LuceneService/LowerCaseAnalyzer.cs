using System.IO;
using Lucene.Net.Analysis;

namespace Ecm.LuceneService
{
    public class LowerCaseAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream stream = new KeywordTokenizer(reader);
            stream = new StopFilter(true, stream, StopAnalyzer.ENGLISH_STOP_WORDS_SET);
            stream = new LowerCaseFilter(stream);
            return stream;
        }

    }
}
