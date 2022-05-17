using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using WellEngineered.CruiseControl.Core.SourceControl;

namespace WellEngineered.CruiseControl.Demos.Plugin.SourceControl
{
    public class IndexFileHistoryParser
        : IHistoryParser
    {
        public Modification[] Parse(TextReader history, DateTime from, DateTime to)
        {
            var modifications = new List<Modification>();
            var document = XDocument.Load(history);
            var fromDate = from.ToString("s");
            var toDate = to.ToString("s");
            foreach (XElement change in document.Descendants("change"))
            {
                var date = DateTime.Parse(
                    change.Attribute("date").Value);
                if ((date >= from) &&
                    (date <= to))
                {
                    var modification = new Modification
                    {
                        FileName = change.Attribute("file").Value,
                        ModifiedTime = date,
                        FolderName = change.Attribute("folder").Value,
                        Type = change.Attribute("type").Value
                    };
                    modifications.Add(modification);
                }
            }

            return modifications.ToArray();
        }
    }
}
